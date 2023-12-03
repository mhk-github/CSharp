/**
 * @file MainWindow.xaml.cs
 * @brief Implementation C# file for the main window of this application.
 *
 * @author Mohammad Haroon Khaliq
 * @date @showdate "%d %B %Y"
 * @copyright MIT License.
 */


///////////////////////////////////////////////////////////////////////////////
// IMPORTS
///////////////////////////////////////////////////////////////////////////////

using Microsoft.Win32;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

using log4net;


////////////////////////////////////////////////////////////////////////////////
// MAIN WINDOW
////////////////////////////////////////////////////////////////////////////////

namespace BackgroundChanger
{
    /**
     * <summary>
     * Interaction logic for MainWindow.xaml.
     * </summary>
     */
    public sealed partial class MainWindow : Window
    {
        ///////////////////////////////////////////////////////////////////////
        // DLL
        ///////////////////////////////////////////////////////////////////////

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfo(
            uint uiAction, 
            uint uiParam, 
            String pvParam, 
            uint fWinIni
        );


        ///////////////////////////////////////////////////////////////////////
        // CONSTANTS 
        ///////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * Constant used for setting the desktop wallpaper.
         * </summary>
         */
        private const uint SPI_SETDESKWALLPAPER = 0x14;

        /**
         * <summary>
         * Constant used for fill desktop background.
         * </summary>
         */
        private const int FILL = 10;

        /**
         * <summary>
         * Constant used for fit desktop background.
         * </summary>
         */
        private const int FIT = 6;

        /**
         * <summary>
         * Constant used for stretch desktop background.
         * </summary>
         */
        private const int STRETCH = 2;

        /**
         * <summary>
         * Constant used for centre desktop background.
         * </summary>
         */
        private const int CENTRE = 0;

        /**
         * <summary>
         * Constant used for tiling the desktop background.
         * </summary>
         */
        private const int TILE_ON = 1;

        /**
         * <summary>
         * Constant used for turning off tiling the desktop background.
         * </summary>
         */
        private const int TILE_OFF = 0;

        /**
         * <summary>
         * Specific registry key for the desktop.
         * </summary>
         */
        private const string REGISTRY_KEY = @"Control Panel\Desktop";

        /**
         * <summary>
         * Specific registry key for the desktop wallpaper style.
         * </summary>
         */
        private const string WALLPAPER_STYLE_KEY = "WallpaperStyle";

        /**
         * <summary>
         * Specific registry key for tiling the desktop wallpaper.
         * </summary>
         */
        private const string TILE_WALLPAPER_KEY = "TileWallpaper";


        ///////////////////////////////////////////////////////////////////////
        // STATIC
        ///////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * The logging system.
         * </summary>
         */
        private static readonly ILog s_log = LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );


        ///////////////////////////////////////////////////////////////////////
        // MEMBER DATA
        ///////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * Canonical path of image file to use.
         * </summary>
         */
        private string _file;


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

            // Get command line arguments
            string[] args = Environment.GetCommandLineArgs();

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

            var recurse = ConfigurationManager.AppSettings["Recurse"];
            var filePatterns = ConfigurationManager.AppSettings["FilePatterns"];

            var logCfg = (
                ConfigurationManager.AppSettings["LoggingConfiguration"]
            );

            // Set up the logging system
            log4net.Config.XmlConfigurator.Configure(new FileInfo(logCfg));
#if DEBUG
            s_log.Info($"BackgroundChanger [DEBUG] - Start [{thisProcess.Id}]");
#else
            s_log.Info($"BackgroundChanger - Start [{thisProcess.Id}]");
#endif // DEBUG
            s_log.Info("  Command line arguments:");
            foreach (var s in args)
            {
                s_log.Info($"    '{s}'");
            }
            s_log.Info("  Configuration:");
            s_log.Info($"    Main window top left X = {mainWindowTopLeftX}");
            s_log.Info($"    Main window top left Y = {mainWindowTopLeftY}");
            s_log.Info($"    Main window width = {mainWindowWidth}");
            s_log.Info($"    Main window height = {mainWindowHeight}");
            s_log.Info($"    Recurse = '{recurse}'");
            s_log.Info($"    File patterns = '{filePatterns}'");
            s_log.Info($"    Logging configuration = '{logCfg}'");
            s_log.Debug("  Enter - MainWindow()");
            s_log.Debug("    'BackgroundChanger' priority set to Idle");

            // Set up the listbox of image files
            SearchOption searchOption;
            if (recurse == "true")
            {
                searchOption = SearchOption.AllDirectories;
            }
            else
            {
                searchOption = SearchOption.TopDirectoryOnly;
            }
            var patterns = filePatterns.Split(';');
            for (var i = 1; i < args.Length; i++)
            {
                foreach (var pattern in patterns)
                {
                    try
                    {
                        var files = Directory.GetFiles(
                            args[i],
                            pattern,
                            searchOption
                        );
                        foreach (var f in files)
                        {
                            var newItem = new ListBoxItem();
                            newItem.Content = f;
                            ImagesListBox.Items.Add(newItem);
                        }
                    }
                    catch (Exception e)
                    {
                        s_log.Fatal(
                            $"    Unexpected exception {e} with message "
                            + $"'{e.Message}' !"
                        );
                        throw;
                    }
                }
            }

            // Set up the radio buttons
            var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY);
            var tiled = int.Parse((string)key.GetValue(TILE_WALLPAPER_KEY));
            if (tiled == TILE_ON)
            {
                Tile_Radio_Button.IsChecked = true;
            }
            else 
            {
                var position = int.Parse(
                    (string)key.GetValue(WALLPAPER_STYLE_KEY)
                );
                switch (position)
                {
                    case FILL:
                        Fill_Radio_Button.IsChecked = true;
                        break;
                    case FIT:
                        Fit_Radio_Button.IsChecked = true;
                        break;
                    case STRETCH:
                        Stretch_Radio_Button.IsChecked = true;
                        break;
                    case CENTRE:
                        Centre_Radio_Button.IsChecked = true;
                        break;
                }
            }

            // Set up member data
            _file = "";

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
         * <param name="e">
         * Arguments connected to this event.
         * </param>
         */
        private void Window_Unloaded(object sender, EventArgs e)
        {
            s_log.Debug($"  Enter - Window_Unloaded({sender}, {e})");

            s_log.Debug("  Leave - Window_Unloaded(...)");
            s_log.Info("BackgroundChanger - End");
        }

        /**
         * <summary>
         * Changes the desktop background.
         * </summary>
         */
        private void ChangeBackground()
        {
            s_log.Debug("    Enter - ChangeBackground()");

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0U, _file, 0U);
            s_log.Debug($"      Background set to '{_file}'");

            s_log.Debug("    Leave - ChangeBackground()");
        }

        /**
         * <summary>
         * Handler for when fill radio button is clicked.
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
        private void Fill_Radio_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - Fill_Radio_Button_Click({sender}, {e})"
            );

            var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true);
            key.SetValue(WALLPAPER_STYLE_KEY, FILL.ToString());
            key.SetValue(TILE_WALLPAPER_KEY, TILE_OFF.ToString());
            if (_file != "")
            {
                ChangeBackground();
            }

            s_log.Debug("  Leave - Fill_Radio_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when fit radio button is clicked.
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
        private void Fit_Radio_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - Fit_Radio_Button_Click({sender}, {e})"
            );

            var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true);
            key.SetValue(WALLPAPER_STYLE_KEY, FIT.ToString());
            key.SetValue(TILE_WALLPAPER_KEY, TILE_OFF.ToString());
            if (_file != "")
            {
                ChangeBackground();
            }

            s_log.Debug("  Leave - Fit_Radio_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when stretch radio button is clicked.
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
        private void Stretch_Radio_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - Stretch_Radio_Button_Click({sender}, {e})"
            );

            var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true);
            key.SetValue(WALLPAPER_STYLE_KEY, STRETCH.ToString());
            key.SetValue(TILE_WALLPAPER_KEY, TILE_OFF.ToString());
            if (_file != "")
            {
                ChangeBackground();
            }

            s_log.Debug("  Leave - Stretch_Radio_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when centre radio button is clicked.
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
        private void Centre_Radio_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - Centre_Radio_Button_Click({sender}, {e})"
            );

            var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true);
            key.SetValue(WALLPAPER_STYLE_KEY, CENTRE.ToString());
            key.SetValue(TILE_WALLPAPER_KEY, TILE_OFF.ToString());
            if (_file != "")
            {
                ChangeBackground();
            }

            s_log.Debug("  Leave - Centre_Radio_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when tile radio button is clicked.
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
        private void Tile_Radio_Button_Click(object sender, RoutedEventArgs e)
        {
            s_log.Debug(
                $"  Enter - Tile_Radio_Button_Click({sender}, {e})"
            );

            var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true);
            key.SetValue(TILE_WALLPAPER_KEY, TILE_ON.ToString());
            if (_file != "")
            {
                ChangeBackground();
            }

            s_log.Debug("  Leave - Tile_Radio_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when an image is selected in the listbox.
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
        private void ImagesListBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - ImagesListBox_SelectionChanged({sender}, {e})"
            );

            _file = ((ListBoxItem)ImagesListBox.SelectedItem).Content.ToString();
            s_log.Debug($"    Selected '{_file}'");
            ChangeBackground();

            s_log.Debug("  Leave - ImagesListBox_SelectionChanged(...)");
        }

        /**
         * <summary>
         * Handler for when the window state changes.
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
        private void Window_StateChanged(
            object sender,
            EventArgs e
        )
        {
            s_log.Debug($"  Enter - Window_StateChanged({sender}, {e})");

            if (WindowState == WindowState.Normal)
            {
                if (_file != "")
                {
                    ChangeBackground();
                }
            }

            s_log.Debug("  Leave - Window_StateChanged(...)");
        }

    }
}


///////////////////////////////////////////////////////////////////////////////
// END
///////////////////////////////////////////////////////////////////////////////
// Local Variables:
// mode: csharp
// End:
