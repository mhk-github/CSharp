////////////////////////////////////////////////////////////////////////////////
// FILE     : Files.cs
// SYNOPSIS : C# code for DAT and archived compiled DAT files.
// LICENSE  : MIT
////////////////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////////////////
// IMPORTS
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;


////////////////////////////////////////////////////////////////////////////////
// MAIN WINDOW
////////////////////////////////////////////////////////////////////////////////

namespace DATTool
{
    public sealed partial class MainWindow : Window
    {
        ////////////////////////////////////////////////////////////////////////
        // CLASS CONSTANTS 
        ////////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * A regular expression to find directories.
         * </summary>
         * 
         * @ingroup CLASS_CONSTANTS
         */
        private static readonly Regex RE_DIRECTORY = new Regex(
            @"^\s*Directory:\s+(.*?)\s*$"
        );

        /**
         * <summary>
         * A regular expression that matches directories.
         * </summary>
         * 
         * @ingroup CLASS_CONSTANTS
         */
        private static readonly Regex RE_DIRECTORY_LINE = new Regex(
            @"^\s*Directory:\s+"
        );

        /**
         * <summary>
         * A regular expression for Windows file attributes.
         * </summary>
         * 
         * @ingroup CLASS_CONSTANTS
         */
        private static readonly Regex RE_FILE_ATTRIBUTES = new Regex(
            @"^-[a-][r-]--\s+" 
        );

        /**
         * <summary>
         * A regular expression for files.
         * </summary>
         * 
         * @ingroup CLASS_CONSTANTS
         */
        private static readonly Regex RE_FILE = new Regex(
            @"^-[a-][r-]--\s+(\d{2})\/(\d{2})\/(\d{2})\s+(\d+):(\d{2})\s+"
            + @"(AM|PM)\s+(\d+)\s+(.*?)\s*$" 
        );

        /**
         * <summary>
         * A regular expression for lines to ignore.
         * </summary>
         * 
         * @ingroup CLASS_CONSTANTS
         */
        private static readonly Regex RE_IGNORE = new Regex(
            @"^(?:d[ar-]{4}|Mode|[-]{4})\s+" 
        );

        /**
         * <summary>
         * A regular expression to find a Windows drive.
         * </summary>
         * 
         * @ingroup CLASS_CONSTANTS
         */
        private static readonly Regex RE_WINDOWS_DRIVE = new Regex(
            @"^\S:"
        );

        /**
         * <summary>
         * A regular expression for compiled DAT objects.
         * </summary>
         * 
         * @ingroup CLASS_CONSTANTS
         */
        private static readonly Regex RE_DATRECORD = new Regex(
            @"^<DatRecord: dat_id=\d+, file_name='(.+?)', file_size=(\d+), "
            + @"file_mtime=(\d+), source_is_new=(\d), source_name='(.+?)', "
            + @"directory_name='(.+?)'>$" 
        );

        ////////////////////////////////////////////////////////////////////////
        // MEMBER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * Parses DAT files to create compiled DAT archives.
         * </summary>
         *
         * <param name="pathDAT">
         * Canonical path to a DAT file.
         * </param>
         *
         * <param name="isNew">
         * True only if this DAT file belongs in the new files category.
         * </param>
         */
        private void CompileDAT(string pathDAT, bool isNew)
        {
            s_log.Debug($"    Enter - CompileDAT('{pathDAT}', {isNew})");

            var dataList = new List<CDATRecord>();
            var source = Path.GetFileNameWithoutExtension(pathDAT);
            var lines = File.ReadAllLines(pathDAT);

            string directory = null;
            string posix_directory = null;
            var maybe_multiline_directory = false;
            var maybe_multiline_file = false;
            foreach (var text in lines)
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    if (RE_FILE_ATTRIBUTES.IsMatch(text))
                    {
                        maybe_multiline_directory = false;
                        maybe_multiline_file = true;
                        if (!string.IsNullOrWhiteSpace(directory))
                        {
                            posix_directory = (
                                RE_DIRECTORY.Match(
                                    directory
                                ).Groups[1].Captures[0].Value
                            ).Replace("\\", "/");
                            if (RE_WINDOWS_DRIVE.IsMatch(posix_directory))
                            {
                                posix_directory = posix_directory.Substring(2);
                                directory = null;
                            }
                        }

                        var fileMatched = RE_FILE.Match(text);
                        var groups = fileMatched.Groups;
                        var month = int.Parse(groups[1].Captures[0].Value);
                        var day = int.Parse(groups[2].Captures[0].Value);
                        var year = int.Parse(groups[3].Captures[0].Value);
                        var hour = int.Parse(groups[4].Captures[0].Value);
                        var minute = int.Parse(groups[5].Captures[0].Value);
                        var isPM = (groups[6].Captures[0].Value == "PM");
                        var fileSize = ulong.Parse(
                            groups[7].Captures[0].Value
                        );
                        var fileName = groups[8].Captures[0].Value;

                        // Get a 4-digit year in the correct century
                        year = (year < _cutOff ? 2000 + year : 1900 + year);

                        // Use a 24-hour clock
                        if (isPM && (hour!= 12))
                        {
                            hour += 12;
                        }
                        else if (hour == 12)
                        {
                            hour = 0;
                        }

                        var fileMTime = new DateTime(
                            year,
                            month,
                            day,
                            hour,
                            minute,
                            0
                        );

                        dataList.Add(
                            new CDATRecord(
                                fileName,
                                fileSize,
                                fileMTime,
                                isNew,
                                source,
                                posix_directory
                            )
                        );
                    }
                    else if (RE_DIRECTORY_LINE.IsMatch(text))
                    {
                        directory = text;
                        maybe_multiline_directory = true;
                        maybe_multiline_file = false;
                    }
                    else if (RE_IGNORE.IsMatch(text))
                    {
                        maybe_multiline_directory = false;
                        maybe_multiline_file = false;
                    }
                    else 
                    {
                        if (maybe_multiline_directory)
                        {
                            directory += text;
                        }
                        else if (maybe_multiline_file)
                        {
                            dataList.Last().FileName += text;
                        }
                    }
                }
            }

            if (dataList.Count > 0)
            {
                // Write CDAT file in UTF-8 format
                var cdrStrings = dataList.Select(
                    x => x.ToString()
                );
                var cdatFile = pathDAT + ".cdat";
                File.WriteAllText(
                    cdatFile,
                    string.Join("\n", cdrStrings),
                    Encoding.UTF8
                );

                // Create the CDAT archive then delete the CDAT file
                var zipFile = cdatFile + ".zip";
                if (File.Exists(zipFile))
                {
                    // Remove any existing ZIP file to prevent exception
                    File.Delete(zipFile);
                }
                using (
                    var zf = ZipFile.Open(zipFile, ZipArchiveMode.Create)
                )
                {
                    zf.CreateEntryFromFile(
                        cdatFile, 
                        Path.GetFileName(cdatFile), 
                        CompressionLevel.Optimal
                    );
                }
                File.Delete(cdatFile);
            }
 
            s_log.Debug(
                $"      Created {dataList.Count} CDATRecord objects from "
                + $"'{pathDAT}'"
            );

            s_log.Debug($"    Leave - CompileDAT('{pathDAT}', {isNew})");
        }

        /**
         * <summary>
         * Reads in a compiled DAT archive file.
         * </summary>
         *
         * <param name="pathCDATArchive">
         * Canonical path to a compiled DAT archive file.
         * </param>
         *
         * <returns>
         * A list of CDATRecord objects.
         * </returns>
         */
        private List<CDATRecord> ImportCDATArchive(string pathCDATArchive)
        {
            s_log.Debug($"    Enter - ImportCDATArchive('{pathCDATArchive}')");

            var retVal = new List<CDATRecord>();
            using (var archive = ZipFile.OpenRead(pathCDATArchive))
            {
                s_log.Debug(
                    $"      Opened ZIP file '{pathCDATArchive}' for reading"
                );
                foreach (var entry in archive.Entries)
                {
                    // Archived compiled DAT files are always in UTF-8 format
                    using (
                        var reader = new StreamReader(
                            entry.Open (),
                            Encoding.UTF8
                        )
                    )
                    {
                        s_log.Debug(
                            $"        Reading entry '{entry.FullName}'"
                        );
                        while (reader.Peek() >= 0)
                        {
                            var matched = RE_DATRECORD.Match(
                                reader.ReadLine()
                            );
                            if (matched.Success)
                            {
                                var matchedGroups = matched.Groups;
                                var fileName = (
                                    matchedGroups[1].Captures[0].Value
                                );
                                var fileSize = ulong.Parse(
                                    matchedGroups[2].Captures[0].Value
                                );
                                var fileMTime = (
                                    DateTimeOffset.FromUnixTimeSeconds(
                                        long.Parse(
                                            matchedGroups[3].Captures[0].Value
                                        )
                                    ).DateTime
                                );
                                var sourceIsNew = (
                                    matchedGroups[4].Captures[0].Value == "1"
                                );
                                var sourceName = (
                                    matchedGroups[5].Captures[0].Value
                                );
                                var directoryName = (
                                    matchedGroups[6].Captures[0].Value
                                );
                                retVal.Add(
                                    new CDATRecord(
                                        fileName,
                                        fileSize,
                                        fileMTime,
                                        sourceIsNew,
                                        sourceName,
                                        directoryName
                                    )
                                );
                            }
                        }
                    }
                }
            }
            s_log.Debug(
                $"      Imported {retVal.Count} CDATRecord objects from "
                + $"'{pathCDATArchive}'"
            );

            s_log.Debug($"    Leave - ImportCDATArchive('{pathCDATArchive}')");
            return retVal;
        }
    }
}


////////////////////////////////////////////////////////////////////////////////
// END
////////////////////////////////////////////////////////////////////////////////
/**
 * @file
 * @brief C# code DAT and achived compiled DAT files.
 *
 * @author Mohammad Haroon Khaliq
 * @date @showdate "%d %B %Y"
 * @copyright MIT License.
 */
// Local Variables:
// mode: csharp
// End:
