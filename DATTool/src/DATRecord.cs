////////////////////////////////////////////////////////////////////////////////
// FILE     : DATRecord.cs
// SYNOPSIS : C# code for the CDATRecord internal class in this application.
// LICENSE  : MIT
////////////////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////////////////
// IMPORTS
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;


////////////////////////////////////////////////////////////////////////////////
// MAIN WINDOW
////////////////////////////////////////////////////////////////////////////////

namespace DATTool
{
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// Encapsulates file information extracted from DAT files.
        /// </summary>
        private sealed class CDATRecord
        {
            ////////////////////////////////////////////////////////////////////
            // MEMBER DATA
            ////////////////////////////////////////////////////////////////////
            
            /// <summary>
            /// The name of the file.
            /// </summary>            
            private string _fileName;
            public string FileName
            {
                get { return _fileName; }
                set { _fileName = value;  }
            }
            
            /// <summary>
            /// The file size in bytes.
            /// </summary>            
            private readonly ulong _fileSize;
            public ulong FileSize 
            { 
                get { return _fileSize; } 
            }
            
            /// <summary>
            /// The file's modification time.
            /// </summary>                        
            private readonly DateTime _fileMTime;
            public DateTime FileMTime 
            { 
                get { return _fileMTime; } 
            }
            
            /// <summary>
            /// True only if this file is in the new file category.
            /// </summary>                        
            private readonly bool _sourceIsNew;
            public bool SourceIsNew { 
                get { return _sourceIsNew; } 
            }
            
            /// <summary>
            /// The source drive of this file.
            /// </summary>                        
            private readonly string _sourceName;
            public string SourceName 
            { 
                get { return _sourceName; } 
            }
            
            /// <summary>
            /// The directory for this file.
            /// </summary>                        
            private readonly string _directoryName;
            public string DirectoryName 
            { 
                get { return _directoryName; }
            }
            
            /// <summary>
            /// The unique identifier in the SQLite database for this file.
            /// </summary>                        
            private int _id;
            public int ID 
            {
                get { return _id; }
                set { _id = value; }
            }

            ////////////////////////////////////////////////////////////////////
            // MEMBER FUNCTIONS
            ////////////////////////////////////////////////////////////////////

            /**
             * <summary>
             * The sole constructor for this class.
             * </summary>
             *
             * <param name="fileName">
             * The name of the file.
             * </param>
             *
             * <param name="fileSize">
             * The size of the file in bytes.
             * </param>
             *
             * <param name="fileMTime">
             * The file modification time.
             * </param>
             *
             * <param name="sourceIsNew">
             * True only if the file belongs in the new files category.
             * </param>
             *
             * <param name="sourceName">
             * The source drive for the file.
             * </param>
             *
             * <param name="directoryName">
             * The directory location of this file.
             * </param>
             *
             * <param name="id">
             * The unique identifier for this file.
             * </param>
             */
            public CDATRecord(
                string fileName,
                ulong fileSize,
                DateTime fileMTime,
                bool sourceIsNew,
                string sourceName,
                string directoryName,
                int id = 0
            )
            {
                _fileName = fileName;
                _fileSize = fileSize;
                _fileMTime = fileMTime;
                _sourceIsNew = sourceIsNew;
                _sourceName = sourceName;
                _directoryName = directoryName;
                _id = id;
            }

            /**
             * <summary>
             * Specific string representation for this class.
             * </summary>
             * 
             * <returns>
             * String for this object.
             * </returns>
             */
            public override string ToString()
            {
                return (
                    "<DatRecord: "
                    + $"dat_id={_id}, "
                    + $"file_name='{_fileName}', "
                    + $"file_size={_fileSize}, "
                    + "file_mtime="
                    + $"{((DateTimeOffset)(_fileMTime)).ToUnixTimeSeconds()}, "
                    + $"source_is_new={Convert.ToInt32(_sourceIsNew)}, "
                    + $"source_name='{_sourceName}', "
                    + $"directory_name='{_directoryName}'"
                    + ">"
                );
            }
        }
    }
}


////////////////////////////////////////////////////////////////////////////////
// END
////////////////////////////////////////////////////////////////////////////////
/**
 * @file
 * @brief C# code for the CDATRecord internal class in this application.
 *
 * @author Mohammad Haroon Khaliq
 * @date @showdate "%d %B %Y"
 * @copyright MIT License.
 */
// Local Variables:
// mode: csharp
// End:
