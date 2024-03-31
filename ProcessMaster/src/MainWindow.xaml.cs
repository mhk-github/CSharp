/**
 * @file MainWindow.xaml.cs
 * @brief Implementation C# file for the main window of this application.
 *
 * @author Mohammad Haroon Khaliq
 * @date @showdate "%d %B %Y"
 * @copyright MIT License.
 */


////////////////////////////////////////////////////////////////////////////////
// IMPORTS
////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using log4net;


////////////////////////////////////////////////////////////////////////////////
// MAIN WINDOW
////////////////////////////////////////////////////////////////////////////////

namespace ProcessMaster
{
    /**
     * <summary>
     * Interaction logic for MainWindow.xaml
     * </summary>
     */
    public sealed partial class MainWindow : Window
    {
        ////////////////////////////////////////////////////////////////////////
        // MEMBER DATA
        ////////////////////////////////////////////////////////////////////////

        // STATIC //////////////////////////////////////////////////////////////

        /**
         * <summary>
         * The logging system.
         * </summary>
         */
        private static readonly ILog s_log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        // NON-STATIC /////////////////////////////////////////////////////////

        /**
         * <summary>
         * The list of processes to set to idle priority.
         * </summary>
         */
        private readonly string[] _idleProcessList;

        /**
         * <summary>
         * The list of processes to set to high priority.
         * </summary>
         */
        private readonly string[] _highProcessList;

        ////////////////////////////////////////////////////////////////////////
        // MEMBER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////

        // STATIC //////////////////////////////////////////////////////////////

        /**
         * <summary>
         * Updates process priorities as needed.
         * </summary>
         * 
         * <param name="processNames">
         * List of process names.
         * </param>
         * 
         * <param name="ppc">
         * Process priority to enforce.
         * </param>
         * 
         * <returns>
         * Either null or a message for the last process updated.
         * </returns>
         */
        private static string UpdateProcessPriorities(
            string[] processNames,
            ProcessPriorityClass ppc
        )
        {
            s_log.Debug(
                $"      Enter - UpdateProcessPriorities({processNames}, {ppc})"
            );

            string msg = null;
            foreach (var entry in processNames)
            {
                var processesArray = Process.GetProcessesByName(entry);
                foreach (var p in processesArray)
                {
                    // Process may already have exited so cater for that
                    try
                    {
                        if (p.PriorityClass != ppc)
                        {
                            p.PriorityClass = ppc;
                            msg = (
                                $"'{p.ProcessName}' [{p.Id}] priority set to "
                                + $"{ppc}"
                            );
                            s_log.Debug($"        {msg}");
                        }
                    }
                    catch (InvalidOperationException ioe)
                    {
                        s_log.Warn(
                            $"        {ioe.GetType()} with message "
                            + $"'{ioe.Message}' !"
                        );
                    }
                    catch (Win32Exception we)
                    {
                        s_log.Error(
                            $"        {we.GetType()} with message "
                            + $"'{we.Message}' !"
                        );
                    }
                    catch (Exception e)
                    {
                        s_log.Fatal(
                            $"        Exception {e.GetType()} with message "
                            + $"'{e.Message}' !"
                        );
                        throw;
                    }
                }
            }

            s_log.Debug("      Leave - UpdateProcessPriorities(...)");
            return msg;
        }

        // NON-STATIC //////////////////////////////////////////////////////////

        /**
         * <summary>
         * The sole constructor for this class.
         * </summary>
         */
        public MainWindow()
        {
            InitializeComponent();

            // Set the process priority to the lowest
            var thisProcess = Process.GetCurrentProcess();
            thisProcess.PriorityClass = ProcessPriorityClass.Idle;

            // Get all configured values
            var mainWindowTopLeftX = int.Parse(
                ConfigurationManager.AppSettings["MainWindowTopLeftX"]
            );
            var mainWindowTopLeftY = int.Parse(
                ConfigurationManager.AppSettings["MainWindowTopLeftY"]
            );
            var mainWindowWidth = int.Parse(
                ConfigurationManager.AppSettings["MainWindowWidth"]
            );
            var mainWindowHeight = int.Parse(
                ConfigurationManager.AppSettings["MainWindowHeight"]
            );

            var logCfg = (
                ConfigurationManager.AppSettings["LoggingConfiguration"]
            );

            var idleProcesses = (
                ConfigurationManager.AppSettings["IdleProcessList"]
            );
            var highProcesses = (
                ConfigurationManager.AppSettings["HighProcessList"]
            );

            var sleepTime = int.Parse(
                ConfigurationManager.AppSettings["SleepTime"]
            );

            // Set up the logging system
            log4net.Config.XmlConfigurator.Configure(new FileInfo(logCfg));
#if DEBUG
            s_log.Info($"ProcessMaster [DEBUG] - Start [{thisProcess.Id}]");
#else
            s_log.Info($"ProcessMaster - Start [{thisProcess.Id}]");
#endif // DEBUG
            s_log.Info("  Configuration:");
            s_log.Info($"    Main window top left X = {mainWindowTopLeftX}");
            s_log.Info($"    Main window top left Y = {mainWindowTopLeftY}");
            s_log.Info($"    Main window width = {mainWindowWidth}");
            s_log.Info($"    Main window height = {mainWindowHeight}");
            s_log.Info($"    Logging configuration = '{logCfg}'");
            s_log.Info($"    Sleep time (ms) = {sleepTime}");
            s_log.Info($"    Idle process list: '{idleProcesses}'");
            s_log.Info($"    High process list: '{highProcesses}'");

            s_log.Debug("  Enter - MainWindow()");
            s_log.Debug("    'ProcessMaster' priority set to Idle");

            // Set up the main window
#if DEBUG
            Title += $" [DEBUG]";
            s_log.Debug($"    Title set to '{Title}'");
#endif // DEBUG
            Left = mainWindowTopLeftX;
            Top = mainWindowTopLeftY;
            Width = mainWindowWidth;
            Height = mainWindowHeight;
            s_log.Debug(
                $"    Geometry set to dimensions {Width}x{Height} at "
                + $"position ({Left},{Top})"
            );

            // Set the text blocks for processes
            idleBox.Text = idleProcesses.Replace('|', '\n');
            highBox.Text = highProcesses.Replace('|', '\n');

            // MHK: Note - This is dealing with a known issue in C# with WPF
            Dispatcher.ShutdownStarted += Window_Unloaded;
            s_log.Debug("    Registered 'Window_Unloaded' event handler");

            // Set the member data
            _idleProcessList = idleProcesses.Split('|');
            _highProcessList = highProcesses.Split('|');

            // Start the timer
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(sleepTime)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            s_log.Debug(
                "    Registered 'Timer_Tick' event handler for timer events "
                + $"each {sleepTime}ms"
            );

            s_log.Debug("  Leave - MainWindow()");
        }

        /**
         * <summary>
         * Handler for the timer event. 
         * </summary>
         * 
         * <param name="sender">
         * Originator of this event.
         * </param>
         * 
         * <param name="ea">
         * Arguments connected to this event.
         * </param>
         */
        private void Timer_Tick(object sender, EventArgs ea)
        {
            s_log.Debug($"  Enter - Timer_Tick({sender}, {ea})");
            var result = Task.Run(() => CheckProcesses()).Result;
            if (result != null)
            {
                statusBarText.Text = $"{DateTime.Now:HH:mm:ss}: {result}";

            }
            s_log.Debug("  Leave - Timer_Tick(...)");
        }

        /**
         * <summary>
         * Handler for the Unloaded event.
         * </summary>
         *
         * <param name="sender">
         * Originator of this event.
         * </param>
         *
         * <param name="ea">
         * Arguments connected to this event.
         * </param>
         */
        private void Window_Unloaded(object sender, EventArgs ea)
        {
            s_log.Debug($"  Enter - Window_Unloaded({sender}, {ea})");
            s_log.Debug("  Leave - Window_Unloaded(...)");
            s_log.Info("ProcessMaster - End");
        }

        /**
         * <summary>
         * Periodically checks processes and sets their priority class.
         * </summary>
         * 
         * <returns>
         * A task string for one process with an updated priority or null.
         * </returns>
         */
        private async Task<string> CheckProcesses()
        {
            s_log.Debug("    Enter - CheckProcesses()");

            var taskIdle = Task.Run(
                () => 
                UpdateProcessPriorities(
                    _idleProcessList,
                    ProcessPriorityClass.Idle
                )
            );
            var taskHigh = Task.Run(
                () =>
                UpdateProcessPriorities(
                    _highProcessList,
                    ProcessPriorityClass.High
                )
            );
            var results = await Task.WhenAll(taskIdle, taskHigh);

            s_log.Debug("    Leave - CheckProcesses()");
            return results[0] ?? results[1];
        }
    }
}


////////////////////////////////////////////////////////////////////////////////
// END
////////////////////////////////////////////////////////////////////////////////
// Local Variables:
// mode: csharp
// End:
