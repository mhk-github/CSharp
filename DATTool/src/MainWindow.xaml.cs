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
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using log4net;


////////////////////////////////////////////////////////////////////////////////
// MAIN WINDOW
////////////////////////////////////////////////////////////////////////////////

namespace DATTool
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
        /// The SQLite database connection.
        /// </summary>
        private readonly SQLiteConnection _sqliteConnection;

        /// <summary>
        /// The SQL drop table command in this application.
        /// </summary>
        private readonly SQLiteCommand _sqlDropTableCommand;

        /// <summary>
        /// The SQL create table commmand in this application.
        /// </summary>
        private readonly SQLiteCommand _sqlCreateTableCommand;

        /// <summary>
        /// The SQL insert prepared statement in this application.
        /// </summary>
        private readonly SQLiteCommand _sqlInsertCommand;

        /// <summary>
        /// The file name parameter for the insert prepared statement.
        /// </summary>
        private readonly SQLiteParameter _fileNameParam;

        /// <summary>
        /// The file size parameter for the insert prepared statement.
        /// </summary>
        private readonly SQLiteParameter _fileSizeParam;

        /// <summary>
        /// A file modification time parameter in an insert prepared statement.
        /// </summary>
        private readonly SQLiteParameter _fileMTimeParam;

        /// <summary>
        /// A boolean parameter in the insert prepared statement for new files.
        /// </summary>
        private readonly SQLiteParameter _sourceIsNewParam;

        /// <summary>
        /// The source name parameter for the insert prepared statement.
        /// </summary>
        private readonly SQLiteParameter _sourceNameParam;

        /// <summary>
        /// The directory name parameter for the insert prepared statement.
        /// </summary>
        private readonly SQLiteParameter _directoryNameParam;

        /// <summary>
        /// Stores setup information for a child window.
        /// </summary>
        private readonly ChildWindowStruct _childWindowStruct;
        
        /// <summary>
        /// Path to the directory containing old DAT files.
        /// </summary>
        private readonly string _oldDatFileDirectory;

        /// <summary>
        /// Path to the directory containing new DAT files.
        /// </summary>
        private readonly string _newDatFileDirectory;

        /// <summary>
        /// Used to decide if years are 1900s or 2000s.
        /// </summary>
        private readonly int _cutOff;

        ///////////////////////////////////////////////////////////////////////
        // MEMBER FUNCTIONS
        ///////////////////////////////////////////////////////////////////////
        
        /**
         * <summary>
         * The sole constructor for this class.
         * </summary>
         *
         * <exception cref="ArgumentException">
         * Thrown if the SQLite connection string is wrong.
         * </exception>
         *
         * <exception cref="SQLiteException">
         * Thrown if a connection to the SQLite database cannot be opened.
         * </exception>
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
            var childWindowWidth = int.Parse(
                ConfigurationManager.AppSettings["ChildWindowWidth"]
            );
            var childWindowHeight = int.Parse(
                ConfigurationManager.AppSettings["ChildWindowHeight"]
            );

            var logCfg = (
                ConfigurationManager.AppSettings["LoggingConfiguration"]
            );

            var oldDATFileDirectory = (
                ConfigurationManager.AppSettings["OldDATFileDirectory"]
            );
            var newDATFileDirectory = (
                ConfigurationManager.AppSettings["NewDATFileDirectory"]
            );

            var sqliteDatabaseFile = (
                ConfigurationManager.AppSettings["SQLiteDatabaseFile"]
            );
            var sqliteVersion = int.Parse(
                ConfigurationManager.AppSettings["SQLiteVersion"]
            );

            var cutOff = int.Parse(
                ConfigurationManager.AppSettings["CutOff"]
            );
            var maxSourceNameLength = int.Parse(
                ConfigurationManager.AppSettings["MaxSourceNameLength"]
            );
            var maxDirectoryNameLength = int.Parse(
                ConfigurationManager.AppSettings["MaxDirectoryNameLength"]
            );
            var maxFileNameLength = int.Parse(
                ConfigurationManager.AppSettings["MaxFileNameLength"]
            );

            // Set up the logging system
            log4net.Config.XmlConfigurator.Configure(new FileInfo(logCfg));
#if DEBUG
            s_log.Info("DATTool [DEBUG] - Start");
#else
            s_log.Info("DATTool - Start");
#endif // DEBUG
            s_log.Info("  Configuration:");
            s_log.Info($"    Main window top left X = {mainWindowTopLeftX}");
            s_log.Info($"    Main window top left Y = {mainWindowTopLeftY}");
            s_log.Info($"    Main window width = {mainWindowWidth}");
            s_log.Info($"    Main window height = {mainWindowHeight}");
            s_log.Info($"    Child window top left X = {childWindowTopLeftX}");
            s_log.Info($"    Child window top left Y = {childWindowTopLeftY}");
            s_log.Info($"    Child window width = {childWindowWidth}");
            s_log.Info($"    Child window height = {childWindowHeight}");
            s_log.Info($"    Logging configuration = '{logCfg}'");
            s_log.Info(
                $"    Old DAT file directory = '{oldDATFileDirectory}'"
            );
            s_log.Info(
                $"    New DAT file directory = '{newDATFileDirectory}'"
            );
            s_log.Info($"    Cutoff year (20th or 21st Century) = {cutOff}");
            s_log.Info($"    Max source name length = {maxSourceNameLength}");
            s_log.Info(
                $"    Max directory name length = {maxDirectoryNameLength}"
            );
            s_log.Info($"    Max file name length = {maxFileNameLength}");
            s_log.Info($"    SQLite database file = '{sqliteDatabaseFile}'");
            s_log.Info($"    SQLite version = {sqliteVersion}");
            s_log.Debug("  Enter - MainWindow()");
            s_log.Debug("    Process priority = 'Idle'");

            // Get a connection to the database open
            var sqliteConnStr = (
                $"Data Source={sqliteDatabaseFile};Version={sqliteVersion};"
            );
            s_log.Debug($"    SQLite connection string = '{sqliteConnStr}'");
            var sqliteConnection = new SQLiteConnection(sqliteConnStr);
            try
            {
                sqliteConnection.Open();
            }
            catch (ArgumentException ae)
            {
                s_log.Fatal(
                    $"    Bad database connection string '{sqliteConnStr}' !"
                    + $" [{ae.Message}]"
                );
                throw;
            }
            catch (SQLiteException sqle)
            {
                s_log.Fatal(
                    $"    Database '{sqliteDatabaseFile}' open failed ! "
                    + $"[{sqle.Message}]"
                );
                throw;
            }
            s_log.Debug($"    Opened database '{sqliteDatabaseFile}'");

            var sqlDropTableCommand = new SQLiteCommand(
                SQL_DROP_TABLE_COMMAND, 
                sqliteConnection
            );

            var sqlCreateTableCommand = new SQLiteCommand(
                (
                    "CREATE TABLE dats ("
                    + "dat_id INTEGER PRIMARY KEY, "
                    + $"file_name VARCHAR({maxFileNameLength}) NOT NULL, "
                    + "file_size INTEGER NOT NULL, "
                    + "file_mtime DATETIME NOT NULL, "
                    + "source_is_new BOOLEAN NOT NULL, "
                    + $"source_name VARCHAR({maxSourceNameLength}) NOT NULL, "
                    + $"directory_name VARCHAR({maxDirectoryNameLength}) NOT NULL"
                    + ")"
                ),
                sqliteConnection
            );

            var sqlInsertCommand = new SQLiteCommand(
                SQL_INSERT_COMMAND, 
                sqliteConnection
            );

            var fileNameParam = sqlInsertCommand.CreateParameter();
            fileNameParam.ParameterName = "$fileName";
            fileNameParam.IsNullable = false;
            sqlInsertCommand.Parameters.Add(fileNameParam);

            var fileSizeParam = sqlInsertCommand.CreateParameter();
            fileSizeParam.ParameterName = "$fileSize";
            fileSizeParam.IsNullable = false;
            sqlInsertCommand.Parameters.Add(fileSizeParam);

            var fileMTimeParam = sqlInsertCommand.CreateParameter();
            fileMTimeParam.ParameterName = "$fileMTime";
            fileMTimeParam.IsNullable = false;
            sqlInsertCommand.Parameters.Add(fileMTimeParam);

            var sourceIsNewParam = sqlInsertCommand.CreateParameter();
            sourceIsNewParam.ParameterName = "$sourceIsNew";
            sourceIsNewParam.IsNullable = false;
            sqlInsertCommand.Parameters.Add(sourceIsNewParam);

            var sourceNameParam = sqlInsertCommand.CreateParameter();
            sourceNameParam.ParameterName = "$sourceName";
            sourceNameParam.IsNullable = false;
            sqlInsertCommand.Parameters.Add(sourceNameParam);

            var directoryNameParam = sqlInsertCommand.CreateParameter();
            directoryNameParam.ParameterName = "$directoryName";
            directoryNameParam.IsNullable = false;
            sqlInsertCommand.Parameters.Add(directoryNameParam);

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

            newDirectoryTextbox.Text = newDATFileDirectory;
            s_log.Debug(
                $"    New DAT directory set to '{newDATFileDirectory}'"
            );
            oldDirectoryTextbox.Text = oldDATFileDirectory;
            s_log.Debug(
                $"    Old DAT directory set to '{oldDATFileDirectory}'"
            );

            // Set the data members
            _sqliteConnection = sqliteConnection;
            _sqlDropTableCommand = sqlDropTableCommand;
            _sqlCreateTableCommand = sqlCreateTableCommand;
            _sqlInsertCommand = sqlInsertCommand;
            _fileNameParam = fileNameParam;
            _fileSizeParam = fileSizeParam;
            _fileMTimeParam = fileMTimeParam;
            _sourceIsNewParam = sourceIsNewParam;
            _sourceNameParam = sourceNameParam;
            _directoryNameParam = directoryNameParam;
            _childWindowStruct = new ChildWindowStruct(
#if DEBUG
                $" [DEBUG] - {sqliteDatabaseFile}",
#else
                $" - {sqliteDatabaseFile}",
#endif // DEBUG
                childWindowTopLeftX,
                childWindowTopLeftY,
                childWindowWidth,
                childWindowHeight
            );
            _oldDatFileDirectory = oldDATFileDirectory;
            _newDatFileDirectory = newDATFileDirectory;
            _cutOff = cutOff;

            s_log.Debug("    Data members:");
            s_log.Debug($"      _sqliteConnection = {_sqliteConnection}");
            s_log.Debug(
                $"      _sqlDropTableCommand = {_sqlDropTableCommand}"
            );
            s_log.Debug(
                $"      _sqlCreateTableCommand = {_sqlCreateTableCommand}"
            );
            s_log.Debug($"      _sqlInsertCommand = {_sqlInsertCommand}");
            s_log.Debug($"      _fileNameParam = {_fileNameParam}");
            s_log.Debug($"      _fileSizeParam = {_fileSizeParam}");
            s_log.Debug($"      _fileMTimeParam = {_fileMTimeParam}");
            s_log.Debug($"      _sourceIsNewParam = {_sourceIsNewParam}");
            s_log.Debug($"      _sourceNameParam = {_sourceNameParam}");
            s_log.Debug($"      _directoryNameParam = {_directoryNameParam}");
            s_log.Debug($"      _childWindowStruct = {_childWindowStruct}");
            s_log.Debug(
                $"      _oldDatFileDirectory = '{_oldDatFileDirectory}'"
            );
            s_log.Debug(
                $"      _newDatFileDirectory = '{_newDatFileDirectory}'"
            );
            s_log.Debug($"      _cutOff = {_cutOff}");

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

            _sqliteConnection.Close();
            s_log.Debug("    Closed connection to database");
      
            s_log.Debug("  Leave - Window_Unloaded(...)");
            s_log.Info("DATTool - End");
        }

        /**
         * <summary>
         * Handler for creating a new database.
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
        private async void Create_Database_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - Create_Database_Button_Click({sender}, {e})"
            );

            createDatabaseButton.IsEnabled = false;
            viewDatabaseButton.IsEnabled = false;

            var oldDir = oldDirectoryTextbox.Text;
            var oldDATFiles = Directory.EnumerateFiles(
                oldDir,
                "*.dat"
            ).Select(
                oldDATFile => (name: oldDATFile, isNew: false)
            );
            var numOldDATFiles = oldDATFiles.Count();
            s_log.Debug($"    Found {numOldDATFiles} old DAT files");

            var newDir = newDirectoryTextbox.Text;
            var newDATFiles = Directory.EnumerateFiles(
                newDir,
                "*.dat"
            ).Select(
                newDATFile => (name: newDATFile, isNew: true)
            );
            var numNewDATFiles = newDATFiles.Count();
            s_log.Debug($"    Found {numNewDATFiles} new DAT files");

            var sortedDATEntries = (
                from entry 
                in oldDATFiles.Concat(newDATFiles)
                orderby new FileInfo(entry.name).Length descending
                select entry
            );

            var totalDATFiles = sortedDATEntries.Count();
            progressBar.Minimum = 0;
            progressBar.Maximum = totalDATFiles;
            progressBar.Value = 0;
            statusBarText.Text = $"Compiling {totalDATFiles} DAT files ...";

            var stopwatch = Stopwatch.StartNew();
            var allCompileTasks = (
                sortedDATEntries.Select(
                    f => Task.Run(() => CompileDAT(f.name, f.isNew))
                )
            );
            var processingCompileTasks = allCompileTasks.Select(
                async t =>
                {
                    await t;
                    progressBar.Value += 1;
                }
            ).ToArray();
            await Task.WhenAll(processingCompileTasks);
            stopwatch.Stop();

            var timeElapsedCompile = stopwatch.ElapsedMilliseconds;
            s_log.Debug(
                $"    Compiled {totalDATFiles} DAT files in "
                + $"{timeElapsedCompile}ms"
            );

            var sortedArchiveFiles = (
                from entry
                in (
                    Directory.EnumerateFiles(
                        newDir,
                        "*.zip"
                    ).Concat(
                        Directory.EnumerateFiles(
                            oldDir,
                            "*.zip"
                        )
                    )
                )
                orderby new FileInfo(entry).Length descending
                select entry
            );
            var numArchiveFiles = sortedArchiveFiles.Count();
            s_log.Debug(
                $"    {numArchiveFiles} compiled DAT archive files found"
            );

            progressBar.Minimum = 0;
            progressBar.Maximum = numArchiveFiles;
            progressBar.Value = 0;
            statusBarText.Text = (
                $"Importing {numArchiveFiles} compiled DAT archives ..."
            );

            stopwatch.Restart();
            var allArchiveImportTasks = sortedArchiveFiles.Select(
                f => Task.Run(() => ImportCDATArchive(f))
            );
            var processingArchiveImportTasks = allArchiveImportTasks.Select(
                async t =>
                {
                    var retVal = await t;
                    progressBar.Value += 1;
                    return retVal;
                }
            ).ToArray();
            var results = await Task.WhenAll(processingArchiveImportTasks);
            stopwatch.Stop();
            var timeElapsedImportArchives = stopwatch.ElapsedMilliseconds;
            s_log.Debug(
                $"    Imported {numArchiveFiles} compiled DAT archive files"
                + $" in {timeElapsedImportArchives}ms"
            );

            var allData = results.SelectMany(
                x => x
            );
            var numItems = allData.Count();

            progressBar.Value = 0;
            progressBar.IsIndeterminate = true;
            statusBarText.Text = (
                $"Inserting {numItems} rows into database ..."
            );

            stopwatch.Restart();
            await Task.Run(() => CreateNewDatabase(allData));
            stopwatch.Stop();

            progressBar.IsIndeterminate = false;
            progressBar.Value = 0;

            var timeElapsedCreateDatabase = stopwatch.ElapsedMilliseconds;
            s_log.Debug(
                $"    New database with {numItems} entries created in "
                + $"{timeElapsedCreateDatabase}ms"
            );
            statusBarText.Text = "New database created";

            createDatabaseButton.IsEnabled = true;
            viewDatabaseButton.IsEnabled = true;

            s_log.Debug("  Leave - Create_Database_Button_Click(...)");
        }

        /**
         * <summary>
         * Handler for viewing a database.
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
        private void View_Database_Button_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            s_log.Debug(
                $"  Enter - View_Database_Button_Click({sender}, {e})"
            );

            createDatabaseButton.IsEnabled = false;
            viewDatabaseButton.IsEnabled = false;

            progressBar.Minimum = 0;
            progressBar.Maximum = 1;
            progressBar.Value = 0;

            statusBarText.Text = "Opening database view ...";

            var cw = new ChildWindow(
                _sqliteConnection,
                _childWindowStruct,
                s_log
            )
            {
                Owner = this
            };
            cw.ShowDialog();

            statusBarText.Text = "Ready";

            createDatabaseButton.IsEnabled = true;
            viewDatabaseButton.IsEnabled = true;

            s_log.Debug("  Leave - View_Database_Button_Click(...)");
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
