////////////////////////////////////////////////////////////////////////////////
// FILE     : SQL.cs
// SYNOPSIS : C# code for SQL performed in the MainWindow class.
// LICENSE  : MIT
////////////////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////////////////
// IMPORTS
////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Windows;


////////////////////////////////////////////////////////////////////////////////
// MAIN WINDOW
////////////////////////////////////////////////////////////////////////////////

namespace DATTool
{
    public sealed partial class MainWindow : Window
    {
        ////////////////////////////////////////////////////////////////////////
        // C# CONSTANTS 
        ////////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * Specific SQL command to drop a table.
         * </summary>
         * 
         * @ingroup C_SHARP_CONSTANTS
         */
        private const string SQL_DROP_TABLE_COMMAND = (
            "DROP TABLE IF EXISTS dats"
        );

        /**
         * <summary>
         * Specific SQL command used to make a prepared insert statement.
         * </summary>
         * 
         * @ingroup C_SHARP_CONSTANTS
         */
        private const string SQL_INSERT_COMMAND = (
            "INSERT INTO dats("
            + "file_name, "
            + "file_size, "
            + "file_mtime, "
            + "source_is_new, "
            + "source_name, "
            + "directory_name"
            + ") VALUES ("
            + "$fileName, "
            + "$fileSize, "
            + "$fileMTime, "
            + "$sourceIsNew, "
            + "$sourceName, "
            + "$directoryName"
            + ")"
        );

        ////////////////////////////////////////////////////////////////////////
        // MEMBER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////

        /**
          * <summary>
          * Creates a new database from a collection of CDATRecord objects.
          * </summary>
          *
          * <param name="data">
          * Collection of CDATRecord objects.
          * </param>
          */
        private void CreateNewDatabase(IEnumerable<CDATRecord> data)
        {
            s_log.Debug($"    Enter - CreateNewDatabase({data})");

            using (var transaction = _sqliteConnection.BeginTransaction())
            {
                _sqlDropTableCommand.ExecuteNonQuery();
                _sqlCreateTableCommand.ExecuteNonQuery();
                foreach (var entry in data)
                {
                    _fileNameParam.Value = entry.FileName;
                    _fileSizeParam.Value = entry.FileSize;
                    _fileMTimeParam.Value = entry.FileMTime;
                    _sourceIsNewParam.Value = entry.SourceIsNew;
                    _sourceNameParam.Value = entry.SourceName;
                    _directoryNameParam.Value = entry.DirectoryName;
                    _sqlInsertCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }

            s_log.Debug("    Leave - CreateNewDatabase(...)");
        }
    }
}


////////////////////////////////////////////////////////////////////////////////
// END
////////////////////////////////////////////////////////////////////////////////
/**
 * @file
 * @brief C# code for SQL performed in the MainWindow class.
 *
 * @author Mohammad Haroon Khaliq
 * @date @showdate "%d %B %Y"
 * @copyright MIT License.
 */
// Local Variables:
// mode: csharp
// End:
