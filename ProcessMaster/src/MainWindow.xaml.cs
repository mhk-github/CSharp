////////////////////////////////////////////////////////////////////////////////
// FILE     : MainWindow.xaml.cs
// SYNOPSIS : Main application window C# source file.
// LICENSE  : MIT
////////////////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////////////////
// IMPORTS
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

using log4net;


////////////////////////////////////////////////////////////////////////////////
// MAIN WINDOW
////////////////////////////////////////////////////////////////////////////////

namespace ProcessMaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        ///////////////////////////////////////////////////////////////////////
        // STATIC
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The logging system.
        /// </summary>
        private static readonly ILog s_log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        ///////////////////////////////////////////////////////////////////////
        // MEMBER DATA
        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The list of processes to set to idle priority.
        /// </summary>
        private readonly string[] _idleProcessList;

        /// <summary>
        /// The list of processes to set to high priority.
        /// </summary>
        private readonly string[] _highProcessList;
        
        /// <summary>
        /// The number of milliseconds to wait before checking processes.
        /// </summary>
        private readonly int _sleepTime;

        ///////////////////////////////////////////////////////////////////////
        // MEMBER FUNCTIONS
        ///////////////////////////////////////////////////////////////////////

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
            s_log.Info($"    Sleep time (ms) = '{sleepTime}'");
            s_log.Info($"    Idle process list: '{idleProcesses}'");
            s_log.Info($"    High process list: '{highProcesses}'");
            s_log.Debug("  Enter - MainWindow()");
            s_log.Debug("    'ProcessMaster' priority set to Idle");

            // ...

            // Set the member data
            _idleProcessList = idleProcesses.Split(';');
            _highProcessList = highProcesses.Split(';');
            _sleepTime = sleepTime;

            // Set up the main window
#if DEBUG
            Title += $" [DEBUG]";
            s_log.Debug($"    Title set to '{Title}'");
#endif // DEBUG
            Left = mainWindowTopLeftX;
            Top = mainWindowTopLeftY;
            Width = mainWindowWidth;
            Height = mainWindowHeight;

            foreach (var e in _idleProcessList)
            {
                idleBox.Items.Add(e);
            }

            foreach (var e in _highProcessList)
            {
                highBox.Items.Add(e);
            }

            s_log.Debug(
                $"    Geometry set to dimensions {Width}x{Height} at "
                + $"position ({Left},{Top})"
            );

            // MHK: Note - This is dealing with a known issue in C# with WPF
            Dispatcher.ShutdownStarted += Window_Unloaded;
            s_log.Debug("    Registered 'Window_Unloaded' event handler");

            CheckProcesses();

            s_log.Debug("  Leave - MainWindow()");
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
         * <param name="e">
         * Arguments connected to this event.
         * </param>
         */
        private void Window_Unloaded(object sender, EventArgs e)
        {
            s_log.Debug($"  Enter - Window_Unloaded({sender}, {e})");

            s_log.Debug("  Leave - Window_Unloaded(...)");
            s_log.Debug("  Leave - CheckProcesses()");
            s_log.Info("ProcessMaster - End");
        }

        /// <summary>
        /// Periodically checks processes and sets their priority class.
        /// </summary>
        async private void CheckProcesses()
        {
            /**
             * <summary>
             * Local function updating process priorities as needed.
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
            string UpdateProcessPriorities(
                string[] processNames, 
                ProcessPriorityClass ppc
            )
            {
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
                                    $"'{p.ProcessName}' [{p.Id}] priority set"
                                    + $" to {ppc}"
                                );
                                s_log.Debug($"    {msg}");
                            }
                        }
                        catch (InvalidOperationException ioe)
                        {
                            s_log.Warn(
                                $"    Exception {ioe.GetType()} with message "
                                + $"'{ioe.Message}' !"
                            );
                        }
                        catch (Exception e)
                        {
                            s_log.Fatal(
                                $"    Exception {e.GetType()} with message "
                                + $"'{e.Message}' !"
                            );
                            throw;
                        }
                    }
                }

                return msg;
            }

            s_log.Debug("  Enter - CheckProcesses()");
            while (true)
            {
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
                var sbText = results[0] ?? results[1];
                if (sbText != null)
                {
                    // Update status bar with any non-null message
                    statusBarText.Text = (
                        $"{DateTime.Now:HH:mm:ss}: {sbText}"
                    );
                }
                   
                await Task.Delay(_sleepTime);
            }
        }
    }

}


////////////////////////////////////////////////////////////////////////////////
// END
////////////////////////////////////////////////////////////////////////////////
/**
 * @file
 * @brief Implementation C# file for the main window of this application.
 *
 * @author Mohammad Haroon Khaliq
 * @date @showdate "%d %B %Y"
 * @copyright MIT License.
 */
// Local Variables:
// mode: csharp
// End:
