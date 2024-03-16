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
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;

using log4net;


//////////////////////////////////////////////////////////////////////////////
// MAIN WINDOW
//////////////////////////////////////////////////////////////////////////////

namespace ImageView
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
            var desktopWidth = int.Parse(
                ConfigurationManager.AppSettings["DesktopWidth"]
            );
            var desktopHeight = int.Parse(
                ConfigurationManager.AppSettings["DesktopHeight"]
            );
            var mainWindowTopLeftX = int.Parse(
                ConfigurationManager.AppSettings["MainWindowTopLeftX"]
            );
            var mainWindowTopLeftY = int.Parse(
                ConfigurationManager.AppSettings["MainWindowTopLeftY"]
            );
            var maxWindowWidth = int.Parse(
                ConfigurationManager.AppSettings["MaxWindowWidth"]
            );
            var maxWindowHeight = int.Parse(
                ConfigurationManager.AppSettings["MaxWindowHeight"]
            );
            var boostWindowHeightIfBiggerX = int.Parse(
                ConfigurationManager.AppSettings["BoostWindowHeightIfBiggerX"]
            );
            var boostWindowHeightIfBiggerY = int.Parse(
                ConfigurationManager.AppSettings["BoostWindowHeightIfBiggerY"]
            );
            var logCfg =
                ConfigurationManager.AppSettings["LoggingConfiguration"];

            log4net.Config.XmlConfigurator.Configure(
                new System.IO.FileInfo(logCfg)
            );
#if DEBUG
            s_log.Info("ImageView [DEBUG] - Start");
#else
            s_log.Info("ImageView - Start");
#endif
            s_log.Info("  Configuration:");
            s_log.Info($"    Desktop width = {desktopWidth}");
            s_log.Info($"    Desktop height = {desktopHeight}");
            s_log.Info($"    Main window top left X = {mainWindowTopLeftX}");
            s_log.Info($"    Main window top left Y = {mainWindowTopLeftY}");
            s_log.Info($"    Maximum window width = {maxWindowWidth}");
            s_log.Info($"    Maximum window height = {maxWindowHeight}");
            s_log.Info(
                $"    Boost width (bigger X) = {boostWindowHeightIfBiggerX}"
            );
            s_log.Info(
                $"    Boost height (bigger Y) = {boostWindowHeightIfBiggerY}"
            );
            s_log.Info($"    Logging configuration = '{logCfg}'");
            s_log.Debug("  Enter - MainWindow()");
            s_log.Debug("    Process priority = 'Idle'");

            // Abort if no image file stated in the command line
            var command_line = Environment.GetCommandLineArgs();
            if (command_line.Length != 2)
            {
                var errorMsg = "One image file is needed !";
                s_log.Error($"    {errorMsg}");
                MessageBox.Show(
                    errorMsg,
                    "Command Line Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                throw new ApplicationException(errorMsg);
            }

            // Input the image and size its display window
            var imgPath = command_line[1];
            s_log.Info($"    Image path = '{imgPath}'");
            var image = Image.FromFile(imgPath);
            var imgWidth = image.Width;
            var imgHeight = image.Height;
            double width = imgWidth;
            double height = imgHeight;
            if (width > maxWindowWidth)
            {
                height *= maxWindowWidth / width;
                width = maxWindowWidth;
            }
            if (height > maxWindowHeight)
            {
                width *= maxWindowHeight / height;
                height = maxWindowHeight;
            }
            // MHK: Note - Width and height are just canvas dimensions.
            if (width > height)
            {
                height += boostWindowHeightIfBiggerX;
            }
            else
            {
                height += boostWindowHeightIfBiggerY;
            }
            s_log.Debug($"      Width = {imgWidth}");
            s_log.Debug($"      Height = {imgHeight}");
            s_log.Debug($"      Window width = {width}");
            s_log.Debug($"      Window height = {height}");

            Title += $" - {imgPath} [{imgWidth}x{imgHeight}]";
            Left = mainWindowTopLeftX;
            Top = mainWindowTopLeftY;
            Width = width;
            Height = height;
            imageShown.Source = new BitmapImage(new Uri(imgPath));

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
            s_log.Info("ImageView - End");
        }
    }
}

//////////////////////////////////////////////////////////////////////////////
// END
//////////////////////////////////////////////////////////////////////////////
// Local Variables:
// mode: csharp
// End:
