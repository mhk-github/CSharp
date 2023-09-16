//////////////////////////////////////////////////////////////////////////////
// FILE     : ChildWindow.xaml.cs
// SYNOPSIS : Child window C# source file.
// LICENSE  : MIT
//////////////////////////////////////////////////////////////////////////////


//////////////////////////////////////////////////////////////////////////////
// IMPORTS
//////////////////////////////////////////////////////////////////////////////

using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using log4net;


//////////////////////////////////////////////////////////////////////////////
// CHILD WINDOW
//////////////////////////////////////////////////////////////////////////////

namespace DATTool
{
    /// <summary>
    /// Interaction logic for ChildWindow.xaml.
    /// </summary>
    sealed partial class ChildWindow : Window
    {
        ///////////////////////////////////////////////////////////////////////
        // C# CONSTANTS 
        ///////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * The SQL query command to use on the database.
         * </summary>
         * 
         * @ingroup C_SHARP_CONSTANTS
         */
        private const string SQL_QUERY_COMMAND = (
            "SELECT dat_id,file_name,file_size,source_name,directory_name "
            + "FROM dats "
            + "WHERE file_name LIKE $fileName "
            + "AND source_name LIKE $sourceName "
            + "AND directory_name LIKE $directoryName "
            + "AND source_is_new = $sourceIsNew"
        );

        //////////////////////////////////////////////////////////////////////
        // MEMBER DATA
        //////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The SQL select command to use on the SQLite database.
        /// </summary>
        private readonly SQLiteCommand _sqlQueryCmd;

        /// <summary>
        /// The file name parameter in the SQL select prepared statement.
        /// </summary>
        private readonly SQLiteParameter _fileNameParam;

        /// <summary>
        /// The source name parameter in the SQL select prepared statement.
        /// </summary>
        private readonly SQLiteParameter _sourceNameParam;

        /// <summary>
        /// The directory name parameter in the SQL select prepared statement.
        /// </summary>
        private readonly SQLiteParameter _directoryNameParam;

        /// <summary>
        /// True if the files to search for are in the new files category.
        /// </summary>
        private readonly SQLiteParameter _sourceIsNewParam;

        /// <summary>
        /// The logging system.
        /// </summary>
        private readonly ILog _iLog;

        //////////////////////////////////////////////////////////////////////
        // MEMBER FUNCTIONS
        //////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * The sole constructor for this class.
         * </summary>
         *
         * <param name="sqliteConnection">
         * An open connection to a SQLite database.
         * </param>
         *
         * <param name="cws">
         * Specific set up information for this child window.
         * </param>
         *
         * <param name="iLog">
         * Logging system used throughout the application.
         * </param>
         */
        internal ChildWindow(
            SQLiteConnection sqliteConnection,
            in ChildWindowStruct cws,
            in ILog iLog
        )
        {
            InitializeComponent();

            _iLog = iLog;
#if DEBUG
            _iLog.Info("    DATTool Child [DEBUG] - Start");
#else
            _iLog.Info("    DATTool Child - Start");
#endif
            _iLog.Debug(
                $"      Enter - ChildWindow({sqliteConnection}, {cws}, {iLog})"
            );

            var sqlQueryCmd = new SQLiteCommand(
                SQL_QUERY_COMMAND,
                sqliteConnection
            );

            var fileNameParam = sqlQueryCmd.CreateParameter();
            fileNameParam.ParameterName = "$fileName";
            fileNameParam.IsNullable = false;
            sqlQueryCmd.Parameters.Add(fileNameParam);

            var sourceNameParam = sqlQueryCmd.CreateParameter();
            sourceNameParam.ParameterName = "$sourceName";
            sourceNameParam.IsNullable = false;
            sqlQueryCmd.Parameters.Add(sourceNameParam);

            var directoryNameParam = sqlQueryCmd.CreateParameter();
            directoryNameParam.ParameterName = "$directoryName";
            directoryNameParam.IsNullable = false;
            sqlQueryCmd.Parameters.Add(directoryNameParam);

            var sourceIsNewParam = sqlQueryCmd.CreateParameter();
            sourceIsNewParam.ParameterName = "$sourceIsNew";
            sourceIsNewParam.IsNullable = false;
            sqlQueryCmd.Parameters.Add(sourceIsNewParam);

            // Set up the child window
            Title += cws.titleExtension;
            _iLog.Debug($"        Title set to '{Title}'");

            statusBarText.Text = "Ready";

            Left = cws.topLeftX;
            Top = cws.topLeftY;
            Width = cws.width;
            Height = cws.height;

            _iLog.Debug(
                $"        Geometry set to dimensions {Width}x{Height} at "
                + $"position ({Left},{Top})"
            );

            _sqlQueryCmd = sqlQueryCmd;
            _fileNameParam = fileNameParam;
            _sourceNameParam = sourceNameParam;
            _directoryNameParam = directoryNameParam;
            _sourceIsNewParam = sourceIsNewParam;

            
            _iLog.Debug("        Data members:");
            _iLog.Debug($"          _sqlQueryCmd = {_sqlQueryCmd}");
            _iLog.Debug($"          _fileNameParam = {_fileNameParam}");
            _iLog.Debug($"          _sourceNameParam = {_sourceNameParam}");
            _iLog.Debug(
                $"          _directoryNameParam = {_directoryNameParam}"
            );
            _iLog.Debug($"          _sourceIsNewParam = {_sourceIsNewParam}");
            
            _iLog.Debug("      Leave - ChildWindow(...)");
        }

        /// <summary>
        /// The search function used on the database.
        /// </summary>
        async private void Search()
        {
            _iLog.Debug("        Enter - Search()");

            var file_text = file_name_textbox.Text;
            var source_text = source_name_textbox.Text;
            var directory_text = directory_name_textbox.Text;

            if (file_text.Length == 0)
            {
                MessageBox.Show(
                    "Give a file name or '%' !",
                    "Empty File Name Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            else if (source_text.Length == 0)
            {
                MessageBox.Show(
                    "Give a source name or '%' !",
                    "Empty Source Name Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            else if (directory_text.Length == 0)
            {
                MessageBox.Show(
                    "Give a directory name or '%' !",
                    "Empty Directory Name Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            else if (
                (file_text == "%")
                && (source_text == "%")
                && (directory_text == "%")
            )
            {
                MessageBox.Show(
                    "At least one text box must not have a wildcard '%' !",
                    "Wildcards Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            else
            {
                var source_new = source_is_new_checkbox.IsChecked;

                _iLog.Debug($"          File = '{file_text}'");
                _iLog.Debug($"          Source = '{source_text}'");
                _iLog.Debug($"          Directory = '{directory_text}'");
                _iLog.Debug($"          Source is new = {source_new}");

                _fileNameParam.Value = $"%{file_text}%";
                _sourceNameParam.Value = $"%{source_text}%";
                _directoryNameParam.Value = $"%{directory_text}%";
                _sourceIsNewParam.Value = source_new;

                statusBarText.Text = "Searching database ...";
                var dt = new DataTable();
                var stopwatch = Stopwatch.StartNew();
                using (
                    var sda = new SQLiteDataAdapter(_sqlQueryCmd)
                )
                {
                    await Task.Run(() => sda.Fill(dt));
                }
                stopwatch.Stop();
                dataGrid.ItemsSource = dt.DefaultView;
                var message = (
                    $"{dt.Rows.Count} row(s) found "
                    + $"[{stopwatch.ElapsedMilliseconds}ms]"
                );
                statusBarText.Text = message;
                _iLog.Debug($"          {message}");
            }
            
            _iLog.Debug("        Leave - Search()");
        }

        /**
         * <summary>
         * Handler for the search button clicked event.
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
        private void Search_Button_Click(object sender, RoutedEventArgs e)
        {
            _iLog.Debug($"      Enter - Search_Button_Click({sender}, {e})");

            Search();

            _iLog.Debug("      Leave - Search_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for keyboard press events.
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
        private void Grid_Key_Down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                _iLog.Debug($"      Enter - Grid_Key_Down({sender}, {e})");
                _iLog.Debug("        'Enter' key pressed");

                Search();

                _iLog.Debug($"      Leave - Grid_Key_Down({sender}, {e})");
            }
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
            _iLog.Debug($"      Enter - Window_Unloaded({sender}, {e})");

            // MHK: Nothing to do here for now.

            _iLog.Debug("      Leave - Window_Unloaded(...)");
            _iLog.Info("    DATTool Child - End");
        }

    }
}

//////////////////////////////////////////////////////////////////////////////
// END
//////////////////////////////////////////////////////////////////////////////
/**
 * @file
 * @brief Child window C# source file.
 *
 * @author Mohammad Haroon Khaliq
 * @date @showdate "%d %B %Y"
 * @copyright MIT License.
 */
// Local Variables:
// mode: csharp
// End:
