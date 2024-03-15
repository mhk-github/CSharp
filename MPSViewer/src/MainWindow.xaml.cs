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

using Microsoft.Win32;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

using log4net;


////////////////////////////////////////////////////////////////////////////////
// MAIN WINDOW
////////////////////////////////////////////////////////////////////////////////

namespace MPSViewer
{
    ////////////////////////////////////////////////////////////////////////////
    // CLASSES
    ////////////////////////////////////////////////////////////////////////////

    /**
     * <summary>
     * Interaction logic for MainWindow.xaml.
     * </summary>
     */
    public sealed partial class MainWindow : Window
    {
        ////////////////////////////////////////////////////////////////////////
        // STATIC
        ////////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * The logging system.
         * </summary>
         */
        private static readonly ILog s_log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        ////////////////////////////////////////////////////////////////////////
        // MEMBER DATA
        ////////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * True if at least one MPS file was processed successfully.
         * </summary>
         */
        private bool _successfulLoad;

        /**
         * <summary>
         * Holds data extracted from MPS files.
         * </summary>
         */
        private CMPS _mps;

        /**
         * <summary>
         * The x-coordinate of the top left corner of child windows. 
         * </summary>
         */
        private readonly int _childWindowTopLeftX;

        /**
         * <summary>
         * The y-coordinate of the top left corner of child windows. 
         * </summary>
         */
        private readonly int _childWindowTopLeftY;

        /**
         * <summary>
         * Maximum width for the client area of a child window.
         * </summary>
         */
        private readonly int _maxChildWindowRectWidth;

        /**
         * <summary>
         * Maximum width for the client area of a child window.
         * </summary>
         */
        private readonly int _maxChildWindowRectHeight;

        /**
         * <summary>
         * Amount of pixels to add to a client rect width to size the window.
         * </summary>
         */
        private readonly int _childWindowWidthBoost;

        /**
         * <summary>
         * Amount of pixels to add to a client rect height to size the window.
         * </summary>
         */
        private readonly int _childWindowHeightBoost;

        ////////////////////////////////////////////////////////////////////////
        // MEMBER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * The sole constructor for this class.
         * </summary>
         */
        public MainWindow()
        {
            InitializeComponent();
            // Set the process priority to the lowest

            Process.GetCurrentProcess().PriorityClass =
                ProcessPriorityClass.Idle;

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
            var childWindowTopLeftX = int.Parse(
                ConfigurationManager.AppSettings["ChildWindowTopLeftX"]
            );
            var childWindowTopLeftY = int.Parse(
                ConfigurationManager.AppSettings["ChildWindowTopLeftY"]
            );
            var maxChildWindowRectWidth = int.Parse(
                ConfigurationManager.AppSettings["MaxChildWindowRectWidth"]
            );
            var maxChildWindowRectHeight = int.Parse(
                ConfigurationManager.AppSettings["MaxChildWindowRectHeight"]
            );
            var childWindowWidthBoost = int.Parse(
                ConfigurationManager.AppSettings["ChildWindowWidthBoost"]
            );
            var childWindowHeightBoost = int.Parse(
                ConfigurationManager.AppSettings["ChildWindowHeightBoost"]
            );

            var logCfg = (
                ConfigurationManager.AppSettings["LoggingConfiguration"]
            );

            // Set up the logging system
            log4net.Config.XmlConfigurator.Configure(new FileInfo(logCfg));
#if DEBUG
            s_log.Info("MPSViewer [DEBUG] - Start");
#else
            s_log.Info("MPSViewer - Start");
#endif // DEBUG
            s_log.Info("  Configuration:");
            s_log.Info($"    Main window top left X = {mainWindowTopLeftX}");
            s_log.Info($"    Main window top left Y = {mainWindowTopLeftY}");
            s_log.Info($"    Main window width = {mainWindowWidth}");
            s_log.Info($"    Main window height = {mainWindowHeight}");
            s_log.Info($"    Child window top left X = {childWindowTopLeftX}");
            s_log.Info($"    Child window top left Y = {childWindowTopLeftY}");
            s_log.Info(
                "    Maximum child window rect width = "
                + $"{maxChildWindowRectWidth}"
            );
            s_log.Info(
                "    Maximum child window rect height = "
                + $"{maxChildWindowRectHeight}"
            );
            s_log.Info(
                $"    Child window width boost = {childWindowWidthBoost}"
            );
            s_log.Info(
                $"    Child window height boost = {childWindowHeightBoost}"
            );
            s_log.Info($"    Logging configuration = '{logCfg}'");
            s_log.Debug("  Enter - MainWindow()");
            s_log.Debug("    Process priority = 'Idle'");

            // Set up member data
            _successfulLoad = false;
            _mps = null;
            _childWindowTopLeftX = childWindowTopLeftX;
            _childWindowTopLeftY = childWindowTopLeftY;
            _maxChildWindowRectWidth = maxChildWindowRectWidth;
            _maxChildWindowRectHeight = maxChildWindowRectHeight;
            _childWindowWidthBoost = childWindowWidthBoost;
            _childWindowHeightBoost = childWindowHeightBoost;

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
                + $"position ({Left}, {Top})"
            );

            // MHK: Note - This is dealing with a known issue in C# with WPF
            Dispatcher.ShutdownStarted += Window_Unloaded;
            s_log.Debug("    Registered 'Window_Unloaded' event handler");

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
         * <param name="ea">
         * Arguments connected to this event.
         * </param>
         */
        private void Window_Unloaded(object sender, EventArgs ea)
        {
            s_log.Debug($"  Enter - Window_Unloaded({sender}, {ea})");

            s_log.Debug("  Leave - Window_Unloaded(...)");
            s_log.Info("MPSViewer - End");
        }

        /**
         * <summary>
         * Enables or disables specific buttons in the GUI.
         * </summary>
         * 
         * <param name="allow">
         * True to enable buttons or false otherwise.
         * </param>
         */
        private void EnableButtons(bool allow)
        {
            fileOpenButton.IsEnabled = allow;
            if (_successfulLoad)
            {
                structureButton.IsEnabled = allow;
                histogram1Button.IsEnabled = allow;
                histogram2Button.IsEnabled = allow;
                histogram3Button.IsEnabled = allow;
            }
        }

        /**
         * <summary>
         * Handler for opening files using a dialog box.
         * </summary>
         * 
         * <param name="sender">
         * Originator of this event.
         * </param>
         * 
         * <param name="rea">
         * Arguments connected to this event.
         * </param>
         */
        private void FileOpenButtonClick(object sender, RoutedEventArgs rea)
        {
            s_log.Debug($"  Enter - FileOpenButtonClick({sender}, {rea})");
            EnableButtons(false);
            try
            {
                var dlg = new OpenFileDialog()
                {
                    InitialDirectory = Environment.GetFolderPath(
                        Environment.SpecialFolder.MyComputer
                    ),
                    Filter = "MPS Files|*.mps|SIF files|*.sif|All files|*.*"
                };
                if (dlg.ShowDialog() == true)
                {
                    string mpsFileName = dlg.FileName;
                    try
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        _mps = new CMPS(mpsFileName);

                        fileNameText.Text = Path.GetFileName(mpsFileName);
                        problemNameText.Text = _mps.GetName();
                        numRowsText.Text =
                            _mps.GetRows().Count.ToString("N0");
                        numColumnsText.Text =
                            _mps.GetColumns().Count.ToString("N0");
                        numElementsText.Text =
                            _mps.GetElements().Count.ToString("N0");
                        var rhs = _mps.GetRHS();
                        var rhsTotal = rhs == null ? 0 : rhs.Count;
                        totalRhsText.Text = rhsTotal.ToString("N0");
                        var ranges = _mps.GetRanges();
                        var rangesTotal = ranges == null ? 0 : ranges.Count;
                        totalRangesText.Text = rangesTotal.ToString("N0");
                        var bounds = _mps.GetBounds();
                        var boundsTotal = bounds == null ? 0 : bounds.Count;
                        totalBoundsText.Text = boundsTotal.ToString("N0");

                        statusBarText.Text = $"Loaded '{mpsFileName}'";
                        _successfulLoad = true;
                        s_log.Debug($"    Loaded file '{mpsFileName}'");
                    }
                    catch (MPSException)
                    {
                        statusBarText.Text = $"Failed to load '{mpsFileName}'";
                        throw;
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;

                    }
                }
            }
            catch (Exception e)
            {
                s_log.Error($"    EXCEPTION: {e} !");
                var ed = new ExceptionDialog(
                    e.GetType().ToString(),
                    e.Message,
                    e.StackTrace
                );
                ed.ShowDialog();
            }
            EnableButtons(true);
            s_log.Debug("  Leave - FileOpenButtonClick(...)");
        }

        /**
         * <summary>
         * Handler for button to open a kernel view of the MPS data.
         * </summary>
         * 
         * <param name="sender">
         * Originator of this event.
         * </param>
         * 
         * <param name="rea">
         * Arguments connected to this event.
         * </param>
         */
        private void StructureButtonClick(object sender, RoutedEventArgs rea)
        {
            s_log.Debug($"  Enter - StructureButtonClick({sender}, {rea})");

            var cw = new ChildWindowKernel(
                fileNameText.Text, 
                _mps,
                _childWindowTopLeftX,
                _childWindowTopLeftY,
                _maxChildWindowRectWidth,
                _maxChildWindowRectHeight,
                _childWindowWidthBoost,
                _childWindowHeightBoost,
                s_log
            )
            {
                Owner = this
            };
            s_log.Debug($"    Created kernel child window {cw}");

            cw.ShowDialog();

            s_log.Debug("  Leave - StructureButtonClick(...)");
        }

        /**
         * <summary>
         * Handler for button to open the histogram for absolute values.
         * </summary>
         * 
         * <param name="sender">
         * Originator of this event.
         * </param>
         * 
         * <param name="rea">
         * Arguments connected to this event.
         * </param>
         */
        private void Histogram1ButtonClick(object sender, RoutedEventArgs rea)
        {
            s_log.Debug($"  Enter - Histogram1ButtonClick({sender}, {rea})");

            var cw = new ChildWindowHistogram1(
                fileNameText.Text, 
                _mps, 
                _childWindowTopLeftX,
                _childWindowTopLeftY,
                s_log
            )
            {
                Owner = this
            };
            s_log.Debug($"    Created histogram 1 child window {cw}");

            cw.ShowDialog();

            s_log.Debug("  Leave - Histogram1ButtonClick(...)");
        }

        /**
         * <summary>
         * Handler for button to open the histogram for row tallies.
         * </summary>
         * 
         * <param name="sender">
         * Originator of this event.
         * </param>
         * 
         * <param name="rea">
         * Arguments connected to this event.
         * </param>
         */
        private void Histogram2ButtonClick(object sender, RoutedEventArgs rea)
        {
            s_log.Debug($"  Enter - Histogram2ButtonClick({sender}, {rea})");

            var cw = new ChildWindowHistogram2(
                fileNameText.Text,
                _mps,
                _childWindowTopLeftX,
                _childWindowTopLeftY,
                s_log
            )
            {
                Owner = this
            };
            s_log.Debug($"    Created histogram 2 child window {cw}");

            cw.ShowDialog();

            s_log.Debug("  Leave - Histogram2ButtonClick(...)");
        }

        /**
         * <summary>
         * Handler for button to open the histogram for column tallies.
         * </summary>
         * 
         * <param name="sender">
         * Originator of this event.
         * </param>
         * 
         * <param name="rea">
         * Arguments connected to this event.
         * </param>
         */
        private void Histogram3ButtonClick(object sender, RoutedEventArgs rea)
        {
            s_log.Debug($"  Enter - Histogram3ButtonClick({sender}, {rea})");

            var cw = new ChildWindowHistogram3(
                fileNameText.Text,
                _mps,
                _childWindowTopLeftX,
                _childWindowTopLeftY,
                s_log
            )
            {
                Owner = this
            };
            s_log.Debug($"    Created histogram 3 child window {cw}");

            cw.ShowDialog();

            s_log.Debug("  Leave - Histogram3ButtonClick(...)");
        }
    }
}


////////////////////////////////////////////////////////////////////////////////
// END
////////////////////////////////////////////////////////////////////////////////
// Local Variables:
// mode: csharp
// End:
