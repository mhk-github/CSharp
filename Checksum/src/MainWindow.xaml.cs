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
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;

using log4net;


////////////////////////////////////////////////////////////////////////////////
// MAIN WINDOW
////////////////////////////////////////////////////////////////////////////////

namespace Checksum
{
    /**
     * <summary>
     * Interaction logic for MainWindow.xaml.
     * </summary>
     */
    public sealed partial class MainWindow : Window
    {
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
         * Digest for SHA1.
         * </summary> 
         */
        private readonly Sha1Digest _sha1;

        /**
         * <summary>
         * Digest for SHA224.
         * </summary> 
         */
        private readonly Sha224Digest _sha224;

        /**
         * <summary>
         * Digest for SHA256.
         * </summary> 
         */
        private readonly Sha256Digest _sha256;

        /**
         * <summary>
         * Digest for SHA384.
         * </summary> 
         */
        private readonly Sha384Digest _sha384;

        /**
         * <summary>
         * Digest for SHA512.
         * </summary> 
         */
        private readonly Sha512Digest _sha512;

        /**
         * <summary>
         * Digest for SHA3_224.
         * </summary> 
         */
        private readonly Sha3Digest _sha3_224;

        /**
         * <summary>
         * Digest for SHA3_256.
         * </summary> 
         */
        private readonly Sha3Digest _sha3_256;

        /**
         * <summary>
         * Digest for SHA3_384.
         * </summary> 
         */
        private readonly Sha3Digest _sha3_384;

        /**
         * <summary>
         * Digest for SHA3_512.
         * </summary> 
         */
        private readonly Sha3Digest _sha3_512;

        /**
         * <summary>
         * Digest for MD5.
         * </summary> 
         */
        private readonly MD5Digest _md5;

        /**
         * <summary>
         * Refers to the currently selected digest.
         * </summary> 
         */
        private IDigest _activeDigest;

        /**
         * <summary>
         * Canonical path of file to calculate a hash for.
         * </summary> 
         */
        private string _filePath;

        /**
         * <summary>
         * Fixed size StringBuilder to reduce heap usage.
         * </summary> 
         */
        private readonly StringBuilder _stringBuilder;

        /**
         * <summary>
         * Fixed size byte array to reduce heap usage for hash values.
         * </summary> 
         */
        private readonly byte[] _hashBytes;

        /**
         * <summary>
         * Fixed size byte array to reduce heap usage for file reads.
         * </summary> 
         */
        private readonly byte[] _buffer;

        /**
         * <summary>
         * Number of bytes to attempt to intake from files per read operation.
         * </summary> 
         */
        private readonly int _fileReadSize;

        
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
            var maxHashBytes = int.Parse(
                ConfigurationManager.AppSettings["MaxHashBytes"]
            );
            var fileReadSize = int.Parse(
                ConfigurationManager.AppSettings["FileReadSize"]
            );

            var logCfg = (
                ConfigurationManager.AppSettings["LoggingConfiguration"]
            );

            // Set up the logging system
            log4net.Config.XmlConfigurator.Configure(new FileInfo(logCfg));
#if DEBUG
            s_log.Info("Checksum [DEBUG] - Start");
#else
            s_log.Info("Checksum - Start");
#endif // DEBUG
            s_log.Info("  Configuration:");
            s_log.Info($"    Main window top left X = {mainWindowTopLeftX}");
            s_log.Info($"    Main window top left Y = {mainWindowTopLeftY}");
            s_log.Info($"    Main window width = {mainWindowWidth}");
            s_log.Info($"    Main window height = {mainWindowHeight}");
            s_log.Info($"    Maximum number of hash bytes = {maxHashBytes}");
            s_log.Info($"    File read buffer size = {fileReadSize}");
            s_log.Info($"    Logging configuration = '{logCfg}'");
            s_log.Debug("  Enter - MainWindow()");
            s_log.Debug("    Process priority = 'Idle'");

            // Set up the data members
            _sha1 = new Sha1Digest();
            _sha224 = new Sha224Digest();
            _sha256 = new Sha256Digest();
            _sha384 = new Sha384Digest();
            _sha512 = new Sha512Digest();
            _sha3_224 = new Sha3Digest(224);
            _sha3_256 = new Sha3Digest(256);
            _sha3_384 = new Sha3Digest(384);
            _sha3_512 = new Sha3Digest(512);
            _md5 = new MD5Digest();
            s_log.Debug("    Created all digest objects");

            _activeDigest = _sha256;
            s_log.Debug("    Default digest set to SHA-256");

            _filePath = string.Empty;
            s_log.Debug($"    File initialized to '{_filePath}'");

            // 2 hexadecimal characters will represent a byte value
            _stringBuilder = new StringBuilder(maxHashBytes * 2);
            s_log.Debug(
                "    Created StringBuilder of capacity "
                + $"{_stringBuilder.Capacity} characters"
            );

            _hashBytes = new byte[maxHashBytes];
            s_log.Debug(
                $"    Allocated hash buffer of {_hashBytes.Length} bytes"
            );

            _buffer = new byte[fileReadSize];
            s_log.Debug(
                $"    Allocated file read buffer of {_buffer.Length} bytes"
            );

            _fileReadSize = fileReadSize;
            s_log.Debug($"    File read size set to {_fileReadSize}");

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
            
            s_log.Info("Checksum - End");
        }

        /**
         * <summary>
         * Handler for when SHA-1 radio button is clicked.
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
        private void SHA1_Radio_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - SHA1_Radio_Button_Click({sender}, {e})"
            );

            _activeDigest = _sha1;
            statusBarText.Text = "Hash SHA-1 selected";
            s_log.Debug("    Active digest set to SHA-1");

            s_log.Debug("  Leave - SHA1_Radio_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when SHA-224 radio button is clicked.
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
        private void SHA224_Radio_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - SHA224_Radio_Button_Click({sender}, {e})"
            );

            _activeDigest = _sha224;
            statusBarText.Text = "Hash SHA-224 selected";
            s_log.Debug("    Active digest set to SHA-224");

            s_log.Debug("  Leave - SHA224_Radio_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when SHA-256 radio button is clicked.
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
        private void SHA256_Radio_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - SHA256_Radio_Button_Click({sender}, {e})"
            );

            _activeDigest = _sha256;
            statusBarText.Text = "Hash SHA-256 selected";
            s_log.Debug("    Active digest set to SHA-256");

            s_log.Debug("  Leave - SHA256_Radio_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when SHA-384 radio button is clicked.
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
        private void SHA384_Radio_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - SHA384_Radio_Button_Click({sender}, {e})"
            );

            _activeDigest = _sha384;
            statusBarText.Text = "Hash SHA-384 selected";
            s_log.Debug("    Active digest set to SHA-384");

            s_log.Debug("  Leave - SHA384_Radio_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when SHA-512 radio button is clicked.
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
        private void SHA512_Radio_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - SHA512_Radio_Button_Click({sender}, {e})"
            );

            _activeDigest = _sha512;
            statusBarText.Text = "Hash SHA-512 selected";
            s_log.Debug("    Active digest set to SHA-512");

            s_log.Debug("  Leave - SHA512_Radio_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when SHA3-224 radio button is clicked.
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
        private void SHA3_224_Radio_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - SHA3_224_Radio_Button_Click({sender}, {e})"
            );

            _activeDigest = _sha3_224;
            statusBarText.Text = "Hash SHA3-224 selected";
            s_log.Debug("    Active digest set to SHA3-224");

            s_log.Debug("  Leave - SHA3_224_Radio_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when SHA3-256 radio button is clicked.
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
        private void SHA3_256_Radio_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - SHA3_256_Radio_Button_Click({sender}, {e})"
            );

            _activeDigest = _sha3_256;
            statusBarText.Text = "Hash SHA3-256 selected";
            s_log.Debug("    Active digest set to SHA3-256");

            s_log.Debug("  Leave - SHA3_256_Radio_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when SHA3-384 radio button is clicked.
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
        private void SHA3_384_Radio_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - SHA3_384_Radio_Button_Click({sender}, {e})"
            );

            _activeDigest = _sha3_384;
            statusBarText.Text = "Hash SHA3-384 selected";
            s_log.Debug("    Active digest set to SHA3-384");

            s_log.Debug("  Leave - SHA3_384_Radio_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when SHA3-512 radio button is clicked.
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
        private void SHA3_512_Radio_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - SHA3_512_Radio_Button_Click({sender}, {e})"
            );

            _activeDigest = _sha3_512;
            statusBarText.Text = "Hash SHA3-512 selected";
            s_log.Debug("    Active digest set to SHA3-512");

            s_log.Debug("  Leave - SHA3_512_Radio_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when MD5 radio button is clicked.
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
        private void MD5_Radio_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - MD5_Radio_Button_Click({sender}, {e})"
            );

            _activeDigest = _md5;
            statusBarText.Text = "Hash MD5 selected";
            s_log.Debug("    Active digest set to MD5");

            s_log.Debug("  Leave - MD5_Radio_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when the open file button is clicked.
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
        private void Open_File_Button_Click(
            object sender,
            RoutedEventArgs rea
        )
        {
            s_log.Debug(
                $"  Enter - Open_File_Button_Click({sender}, {rea})"
            );

            try
            {
                var ofDlg = new OpenFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(
                        Environment.SpecialFolder.MyComputer
                    ),
                    Filter = "All files(*.*)| *.*"
                };
                if (ofDlg.ShowDialog() == true)
                {
                    _filePath = ofDlg.FileName;
                    calculateHashButton.IsEnabled = true;
                    statusBarText.Text = $"Selected '{_filePath}'";
                    s_log.Debug($"    File set to '{_filePath}'");
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

            s_log.Debug("  Leave - Open_File_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for when the calculate hash button is clicked.
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
        private void Calculate_Hash_Button_Click(
            object sender,
            RoutedEventArgs rea
        )
        {
            s_log.Debug(
                $"  Enter - Calculate_Hash_Button_Click({sender}, {rea})"
            );

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                using (var fs = File.OpenRead(_filePath))
                {
                    var bytesRead = 0;
                    while (true)
                    {
                        bytesRead = fs.Read(_buffer, 0, _fileReadSize);
                        if (bytesRead == 0)
                        {
                            break;
                        }
                        _activeDigest.BlockUpdate(_buffer, 0, bytesRead);
                    }
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

            _activeDigest.DoFinal(_hashBytes, 0);
            _activeDigest.Reset();

            var hashSize = _activeDigest.GetDigestSize();
            for (var i = 0; i < hashSize; ++i)
            {
                _stringBuilder.AppendFormat("{0:x2}", _hashBytes[i]);
            }
            var hexString = _stringBuilder.ToString();
            _stringBuilder.Clear();

            var statedHash = statedChecksum.Text;
            if (hexString == statedHash)
            {
                calculatedChecksum.Content = "Match";
                calculatedChecksum.Background = Brushes.Green;
                s_log.Debug(
                    $"    Stated hash '{statedHash}' for '{_filePath}' matches"
                    + $" calculated hash '{hexString}' "
                    + $"[{_activeDigest.AlgorithmName}]"
                );
            }
            else 
            {
                calculatedChecksum.Content = "Mismatch";
                calculatedChecksum.Background = Brushes.Red;
                s_log.Warn(
                    $"    Stated hash '{statedHash}' for '{_filePath}' "
                    + $"does not match calculated hash '{hexString}' ! "
                    + $"[{_activeDigest.AlgorithmName}]"
                );
            }
            statusBarText.Text = $"Computed {_activeDigest.AlgorithmName}";
            Mouse.OverrideCursor = null;

            s_log.Debug("  Leave - Calculate_Hash_Button_Click(...)");
        }
        
    }
    
}


///////////////////////////////////////////////////////////////////////////////
// END
///////////////////////////////////////////////////////////////////////////////
// Local Variables:
// mode: csharp
// End:
