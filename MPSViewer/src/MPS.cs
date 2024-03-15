/**
 * @file MPS.cs
 * @brief Contains all code for reading, validating and storing MPS file data.
 *
 * @author Mohammad Haroon Khaliq
 * @date @showdate "%d %B %Y"
 * @copyright MIT License.
 */


////////////////////////////////////////////////////////////////////////////////
// IMPORTS
////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


namespace MPSViewer
{
    ////////////////////////////////////////////////////////////////////////////
    // CLASSES
    ////////////////////////////////////////////////////////////////////////////

    // EXCEPTIONS //////////////////////////////////////////////////////////////

    /**
     * <summary>
     * Raised for all errors in opening or validating MPS files.
     * </summary>
     */
    public sealed class MPSException : Exception
    {
        /**
         * <summary>
         * The sole constructor for this class
         * </summary>
         * 
         * <param name="message">
         * Provided error message.
         * </param>
         */
        public MPSException(string message) : base(message)
        {
        }
    }


    // DATA ////////////////////////////////////////////////////////////////////

    /**
     * <summary>
     * Holds data extracted from MPS files.
     * </summary>
     */
    public sealed class CMPS
    {
        ////////////////////////////////////////////////////////////////////////
        // CLASS CONSTANTS
        ////////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * All permitted row types in MPS format.
         * </summary>
         */
        private static readonly HashSet<string> SET_ROW_TYPES = (
            new HashSet<string>() { "E", "G", "L", "N" }
        );

        /**
         * <summary>
         * All permitted bounds types in MPS format.
         * </summary>
         */
        private static readonly HashSet<string> SET_BOUNDS_TYPES = (
            new HashSet<string>() { "FR", "FX", "LO", "MI", "PL", "UP" }
        );

        /**
         * <summary>
         * Regular expression for NAME section line of MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_NAME_SECTION = new Regex(
            @"^NAME\s+(.*?)\s*$"
        );

        /**
         * <summary>
         * Regular expression for ROWS section head of MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_ROWS_SECTION = new Regex(
            @"^ROWS\s*$"
        );

        /**
         * <summary>
         * Regular expression for COLUMNS section head of MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_COLUMNS_SECTION = new Regex(
            @"^COLUMNS\s*$"
        );

        /**
         * <summary>
         * Regular expression for RHS section head of MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_RHS_SECTION = new Regex(
            @"^RHS\s*$"
        );

        /**
         * <summary>
         * Regular expression for RANGES section head of MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_RANGES_SECTION = new Regex(
            @"^RANGES\s*$"
        );

        /**
         * <summary>
         * Regular expression for BOUNDS section head of MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_BOUNDS_SECTION = new Regex(
            @"^BOUNDS\s*$"
        );

        /**
         * <summary>
         * Regular expression for ENDATA section marker of MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_ENDATA_SECTION = new Regex(
            @"^ENDATA\s*$"
        );

        /**
         * <summary>
         * Regular expression for a line of row data in MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_ROWS_DATA = new Regex(
            @"^\s*(\S)\s+(\S+)\s*$"
        );

        /**
         * <summary>
         * Regular expression for line of 2-column column data in MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_COLUMNS_2_DATA = new Regex(
            @"^\s*(\S+)\s+(\S+)\s+([0-9eE.+-]+)\s+(\S+)\s+([0-9eE.+-]+)\s*$"
        );

        /**
         * <summary>
         * Regular expression for line of 1-column column data in MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_COLUMNS_1_DATA = new Regex(
            @"^\s*(\S+)\s+(\S+)\s+([0-9eE.+-]+)\s*$"
        );

        /**
         * <summary>
         * Regular expression for line of 2-column RHS data in MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_RHS_2_DATA = new Regex(
            @"^\s*(\S+)\s+(\S+)\s+([0-9eE.+-]+)\s+(\S+)\s+([0-9eE.+-]+)\s*$"
        );

        /**
         * <summary>
         * Regular expression for line of 1-column RHS data in MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_RHS_1_DATA = new Regex(
            @"^\s*(\S+)\s+(\S+)\s+([0-9eE.+-]+)\s*$"
        );

        /**
         * <summary>
         * Alternate regular expression for 2-column RHS data in MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_RHS_2_DATA_EXTRA = new Regex(
            @"^\s*(\S+)\s+([0-9eE.+-]+)\s+(\S+)\s+([0-9eE.+-]+)\s*$"
        );

        /**
         * <summary>
         * Alternate regular expression for 1-column RHS data in MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_RHS_1_DATA_EXTRA = new Regex(
            @"^\s*(\S+)\s+([0-9eE.+-]+)\s*$"
        );

        /**
         * <summary>
         * Regular expression for line of 2-column ranges data in MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_RANGES_2_DATA = new Regex(
            @"^\s*(\S*)\s+(\S+)\s+([0-9eE.+-]+)\s+(\S+)\s+([0-9eE.+-]+)\s*$"
        );

        /**
         * <summary>
         * Regular expression for line of 1-column ranges data in MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_RANGES_1_DATA = new Regex(
            @"^\s*(\S*)\s+(\S+)\s+([0-9eE.+-]+)\s*$"
        );

        /**
         * <summary>
         * Regular expression for line of bounds data in MPS file.
         * </summary>
         */
        private static readonly Regex RE_MPS_BOUNDS_DATA = new Regex(
            @"^\s*(\S+)\s+(\S*)\s+(\S+)\s+([0-9eE.+-]+)\s*$"
        );

        ////////////////////////////////////////////////////////////////////////
        // MEMBER DATA
        ////////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * The problem name.
         * </summary>
         */
        private readonly string _mpsProblemName;

        /**
         * <summary>
         * A dictionary for information about each row.
         * </summary>
         */
        private readonly Dictionary<string, (int, string)> _mpsRows;

        /**
         * <summary>
         * A dictionary for all information about each column.
         * </summary>
         */
        private readonly Dictionary<string, int> _mpsColumns;

        /**
         * <summary>
         * A dictionary for all information about non-zero elements.
         * </summary>
         */
        private readonly Dictionary<(int, int), double> _mpsElements;

        /**
         * <summary>
         * A dictionary for all information about the problem right-hand side.
         * </summary>
         */
        private readonly Dictionary<string, double> _mpsRHS;

        /**
         * <summary>
         * A dictionary for all information about ranges in the problem.
         * </summary>
         */
        private readonly Dictionary<string, double> _mpsRanges;

        /**
         * <summary>
         * A dictionary for all information about bounds in the problem.
         * </summary>
         */
        private readonly Dictionary<string, List<(string, double)>> _mpsBounds;

        ////////////////////////////////////////////////////////////////////////
        // MEMBER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////

        // STATIC //////////////////////////////////////////////////////////////

        /**
         * <summary>
         * Extracts the MPS problem name from the NAME section.
         * </summary>
         * 
         * <param name="input">
         * The NAME section line of the MPS file.
         * </param>
         * 
         * <returns>
         * The string for the MPS problem name.
         * </returns>
         */
        private static string ExtractName(string input)
        {
            return (
                RE_MPS_NAME_SECTION.Match(input).Groups[1].Captures[0].Value
            );
        }

        /**
         * <summary>
         * Extracts row data from MPS ROWS section.
         * </summary>
         * 
         * <param name="data">
         * List of strings of MPS ROWS data.
         * </param>
         * 
         * <returns>
         * Dictionary<string, (int, string)> representing MPS row information.
         * </returns>
         * 
         * <exception cref="MPSException">
         * Thrown if there is any error in validating MPS rows.
         * </exception>
         */
        private static Dictionary<string, (int, string)>
            ExtractRows(List<string> data)
        {
            var rowData = new Dictionary<string, (int, string)>();
            var currentRowId = 0;
            foreach (var line in data)
            {
                var matched = RE_MPS_ROWS_DATA.Match(line);
                if (matched.Success)
                {
                    var type = matched.Groups[1].Captures[0].Value;
                    if (SET_ROW_TYPES.Contains(type))
                    {
                        var rowName = matched.Groups[2].Captures[0].Value;
                        if (rowData.ContainsKey(rowName))
                        {
                            var (_, rowType) = rowData[rowName];

                            throw new MPSException(
                                $"Duplicate '{rowName}' (type='{rowType}') "
                                + $"found in '{line}' in MPS ROWS section !"
                            );
                        }
                        rowData.Add(rowName, (currentRowId, type));
                        currentRowId++;
                    }
                    else 
                    {
                        throw new MPSException(
                            $"Unknown row type '{type}' in '{line}' in MPS "
                            + "ROWS section !"
                        );
                    }
                }
                else 
                {
                    throw new MPSException(
                        $"Cannot parse '{line}' in MPS ROWS section !"
                    );
                }

            }

            return (rowData);
        }

        /**
         * <summary>
         * Extracts column and non-zero element data from MPS COLUMNS section.
         * </summary>
         * 
         * <param name="data">
         * List of strings of MPS COLUMNS data.
         * </param>
         * 
         * <param name="rows">
         * Dictionary for all information about MPS rows.
         * </param>
         * 
         * <returns>
         * (Dictionary<string, int>, Dictionary<(int, int), double>) tuple for 
         * column information and non-zero element information respectively.
         * </returns>
         * 
         * <exception cref="MPSException">
         * Thrown if there is any error in validating MPS columns.
         * </exception>
         */
        private static (Dictionary<string, int>, Dictionary<(int, int), double>)
            ExtractColumns(
                List<string> data, 
                Dictionary<string, (int, string)> rows
            )
        {
            var columnData = new Dictionary<string, int>();
            var elementData = new Dictionary<(int, int), double>();
            var currentColumnId = 0;
            Match matched;
            foreach (var line in data)
            {
                if ((matched = RE_MPS_COLUMNS_2_DATA.Match(line)).Success)
                {
                    var columnName = matched.Groups[1].Captures[0].Value;
                    var rowName1 = matched.Groups[2].Captures[0].Value;
                    int rowId1;
                    try
                    {
                        (rowId1, _) = rows[rowName1];

                    }
                    catch (KeyNotFoundException)
                    {
                        throw new MPSException(
                            $"Unknown row '{rowName1}' in '{line}' in MPS "
                            + "COLUMNS section !"
                        );
                    }
                    var element1 = matched.Groups[3].Captures[0].Value;

                    var rowName2 = matched.Groups[4].Captures[0].Value;
                    int rowId2;
                    try
                    {
                        (rowId2, _) = rows[rowName2];

                    }
                    catch (KeyNotFoundException)
                    {
                        throw new MPSException(
                            $"Unknown row '{rowName2}' in '{line}' in MPS "
                            + "COLUMNS section !"
                        );
                    }
                    var element2 = matched.Groups[5].Captures[0].Value;

                    if (!columnData.ContainsKey(columnName))
                    {
                        columnData.Add(columnName, currentColumnId);
                        currentColumnId++;
                    }

                    var columnId = columnData[columnName];
                    var elementValue = Double.Parse(element1);
                    if (elementValue != 0.0)
                    {
                        elementData.Add((rowId1, columnId), elementValue);
                    }
                    elementValue = Double.Parse(element2);
                    if (elementValue != 0.0)
                    {
                        elementData.Add((rowId2, columnId), elementValue);
                    }
                }
                else if ((matched = RE_MPS_COLUMNS_1_DATA.Match(line)).Success)
                {
                    var columnName = matched.Groups[1].Captures[0].Value;
                    var rowName = matched.Groups[2].Captures[0].Value;
                    int rowId;
                    try
                    {
                        (rowId, _) = rows[rowName];

                    }
                    catch (KeyNotFoundException)
                    {
                        throw new MPSException(
                            $"Unknown row '{rowName}' in '{line}' in MPS "
                            + "COLUMNS section !"
                        );
                    }
                    var element = matched.Groups[3].Captures[0].Value;

                    if (!columnData.ContainsKey(columnName))
                    {
                        columnData.Add(columnName, currentColumnId);
                        currentColumnId++;
                    }

                    var columnId = columnData[columnName];
                    var elementValue = Double.Parse(element);
                    if (elementValue != 0.0)
                    {
                        elementData.Add((rowId, columnId), elementValue);
                    }
                }
                else 
                {
                    throw new MPSException(
                        $"Cannot parse '{line}' in MPS COLUMNS section !"
                    );
                }
            }
            
            return (columnData, elementData);
        }

        /**
         * <summary>
         * Extracts right-hand side data from MPS RHS section.
         * </summary>
         * 
         * <param name="data">
         * List of strings of MPS RHS data.
         * </param>
         * 
         * <param name="rows">
         * Dictionary for all information about MPS rows.
         * </param>
         * 
         * <returns>
         * Dictionary<string, double> representing RHS data or null.
         * </returns>
         * 
         * <exception cref="MPSException">
         * Thrown if there is any error in validating MPS right-hand sides.
         * </exception>
         */
        private static Dictionary<string, double> 
            ExtractRHS(List<string> data, Dictionary<string, (int, string)> rows)
        {
            var rhsData = new Dictionary<string, double>();
            string rhsName = null;
            Match matched;
            foreach (var line in data)
            {
                if ((matched = RE_MPS_RHS_2_DATA.Match(line)).Success)
                {
                    if (rhsName == null)
                    {
                        rhsName = matched.Groups[1].Captures[0].Value;
                    }
                    else if (rhsName != matched.Groups[1].Captures[0].Value)
                    {
                        break;
                    }

                    var nameRow1 = matched.Groups[2].Captures[0].Value;
                    if (!rows.ContainsKey(nameRow1))
                    {
                        throw new MPSException(
                            $"Unknown row '{nameRow1}' in '{line}' in MPS RHS "
                            + "section !"
                        );
                    }
                    var rhsValue1 = matched.Groups[3].Captures[0].Value;

                    var nameRow2 = matched.Groups[4].Captures[0].Value;
                    if (!rows.ContainsKey(nameRow2))
                    {
                        throw new MPSException(
                            $"Unknown row '{nameRow2}' in '{line}' in MPS RHS "
                            + "section !"
                        );
                    }
                    var rhsValue2 = matched.Groups[5].Captures[0].Value;

                    rhsData[nameRow1] = Double.Parse(rhsValue1);
                    rhsData[nameRow2] = Double.Parse(rhsValue2);
                }
                else if ((matched = RE_MPS_RHS_1_DATA.Match(line)).Success)
                {
                    if (rhsName == null)
                    {
                        rhsName = matched.Groups[1].Captures[0].Value;
                    }
                    else if (rhsName != matched.Groups[1].Captures[0].Value)
                    {
                        break;
                    }

                    var nameRow = matched.Groups[2].Captures[0].Value;
                    if (!rows.ContainsKey(nameRow))
                    {
                        throw new MPSException(
                            $"Unknown row '{nameRow}' in '{line}' in MPS RHS "
                            + "section !"
                        );
                    }
                    var rhsValue = matched.Groups[3].Captures[0].Value;

                    rhsData[nameRow] = Double.Parse(rhsValue);
                }
                else if (
                    (matched = RE_MPS_RHS_2_DATA_EXTRA.Match(line)).Success
                )
                {
                    var nameRow1 = matched.Groups[1].Captures[0].Value;
                    if (!rows.ContainsKey(nameRow1))
                    {
                        throw new MPSException(
                            $"Unknown row '{nameRow1}' in '{line}' in MPS RHS "
                            + "section !"
                        );
                    }
                    var rhsValue1 = matched.Groups[2].Captures[0].Value;

                    var nameRow2 = matched.Groups[3].Captures[0].Value;
                    if (!rows.ContainsKey(nameRow2))
                    {
                        throw new MPSException(
                            $"Unknown row '{nameRow2}' in '{line}' in MPS RHS "
                            + "section !"
                        );
                    }
                    var rhsValue2 = matched.Groups[4].Captures[0].Value;

                    rhsData[nameRow1] = Double.Parse(rhsValue1);
                    rhsData[nameRow2] = Double.Parse(rhsValue2);
                }
                else if (
                    (matched = RE_MPS_RHS_1_DATA_EXTRA.Match(line)).Success
                )
                {
                    var nameRow = matched.Groups[1].Captures[0].Value;
                    if (!rows.ContainsKey(nameRow))
                    {
                        throw new MPSException(
                            $"Unknown row '{nameRow}' in '{line}' in MPS RHS "
                            + "section !"
                        );
                    }
                    var rhsValue = matched.Groups[2].Captures[0].Value;

                    rhsData[nameRow] = Double.Parse(rhsValue);
                }
                else 
                {
                    throw new MPSException(
                        $"Cannot parse '{line}' in MPS RHS section !"
                    );
                }
            }

            return (rhsData);
        }

        /**
         * <summary>
         * Extracts ranges data from MPS RANGES section.
         * </summary>
         * 
         * <param name="data">
         * List of strings of MPS RANGES data.
         * </param>
         * 
         * <param name="rows">
         * Dictionary for all information about MPS rows.
         * </param>
         * 
         * <returns>
         * Dictionary<string, double> of ranges data or null.
         * </returns>
         * 
         * <exception cref="MPSException">
         * Thrown if there is any error in validating MPS ranges.
         * </exception>
         */
        private static Dictionary<string, double>
            ExtractRanges(
                List<string> data, 
                Dictionary<string, (int, string)> rows
            )
        {
            var rangesData = new Dictionary<string, double>();
            string rangesName = null;
            Match matched;
            foreach (var line in data)
            {
                if ((matched = RE_MPS_RANGES_2_DATA.Match(line)).Success)
                {
                    if (rangesName == null)
                    {
                        rangesName = matched.Groups[1].Captures[0].Value;
                    }
                    else if (rangesName != matched.Groups[1].Captures[0].Value)
                    {
                        break;
                    }

                    var nameRow1 = matched.Groups[2].Captures[0].Value;
                    if (!rows.ContainsKey(nameRow1))
                    {
                        throw new MPSException(
                            $"Unknown row '{nameRow1}' in '{line}' in MPS "
                            + "RANGES section !"
                        );
                    }
                    var value1 = matched.Groups[3].Captures[0].Value;

                    var nameRow2 = matched.Groups[4].Captures[0].Value;
                    if (!rows.ContainsKey(nameRow2))
                    {
                        throw new MPSException(
                            $"Unknown row '{nameRow2}' in '{line}' in MPS "
                            + "RANGES section !"
                        );
                    }
                    var value2 = matched.Groups[5].Captures[0].Value;

                    rangesData[nameRow1] = Double.Parse(value1);
                    rangesData[nameRow2] = Double.Parse(value2);
                }
                else if ((matched = RE_MPS_RANGES_1_DATA.Match(line)).Success)
                {
                    if (rangesName == null)
                    {
                        rangesName = matched.Groups[1].Captures[0].Value;
                    }
                    else if (rangesName != matched.Groups[1].Captures[0].Value)
                    {
                        break;
                    }

                    var nameRow = matched.Groups[2].Captures[0].Value;
                    if (!rows.ContainsKey(nameRow))
                    {
                        throw new MPSException(
                            $"Unknown row '{nameRow}' in '{line}' in MPS "
                            + "RANGES section !"
                        );
                    }
                    var value = matched.Groups[3].Captures[0].Value;

                    rangesData[nameRow] = Double.Parse(value);
                }
                else 
                {
                    throw new MPSException(
                        $"Cannot parse '{line}' in MPS RANGES section !"
                    );
                }
            }

            return (rangesData);
        }

        /**
         * <summary>
         * Extracts bounds data from MPS BOUNDS section.
         * </summary>
         * 
         * <param name="data">
         * List of strings of MPS BOUNDS data.
         * </param>
         * 
         * <param name="columns">
         * Dictionary for all information about MPS columns.
         * </param>
         * 
         * <returns>
         * Dictionary<string, List<(string, double)>> for bounds data or null.
         * </returns>
         * 
         * <exception cref="MPSException">
         * Thrown if there is any error in validating MPS bounds.
         * </exception>
         */
        private static Dictionary<string, List<(string, double)>> 
            ExtractBounds(
                List<string> data, 
                Dictionary<string, int> columns
            )
        {
            var boundsData = new Dictionary<string, List<(string, double)>>();
            string boundsName = null;
            Match matched;
            foreach (var line in data)
            {
                if ((matched = RE_MPS_BOUNDS_DATA.Match(line)).Success)
                {
                    var boundsType = matched.Groups[1].Captures[0].Value;
                    if (!SET_BOUNDS_TYPES.Contains(boundsType))
                    {
                        throw new MPSException(
                            $"Unknown bounds type '{boundsType}' in '{line}' "
                            + "in MPS BOUNDS section !"
                        );
                    }

                    if (boundsName == null)
                    {
                        boundsName = matched.Groups[2].Captures[0].Value;
                    }
                    else if (boundsName != matched.Groups[2].Captures[0].Value)
                    {
                        break;
                    }

                    var columnName = matched.Groups[3].Captures[0].Value;
                    if (!columns.ContainsKey(columnName))
                    {
                        throw new MPSException(
                            $"Unknown column '{columnName}' in '{line}' in "
                            + "MPS BOUNDS section !"
                        );
                    }

                    var value = matched.Groups[4].Captures[0].Value;
                    if (!boundsData.ContainsKey(columnName))
                    {
                        boundsData[columnName] = new List<(string, double)>();
                    }
                    boundsData[columnName].Add(
                        (boundsType, Double.Parse(value))
                    );
                }
                else 
                {
                    throw new MPSException(
                        $"Cannot parse '{line}' in MPS BOUNDS section !"
                    );
                }
            }

            return (boundsData);
        }

        // NON-STATIC //////////////////////////////////////////////////////////

        /**
         * <summary>
         * The sole constructor for this class.
         * </summary>
         * 
         * <param name="mpsFileName">
         * Canonical path of MPS file.
         * </param>

         * <exception cref="MPSException">
         * Thrown for any MPS-related error.
         * </exception>
         */
        public CMPS(string mpsFileName)
        {
            string nameLine = null;
            var rowsLines = new List<string>();
            var columnsLines = new List<string>();
            var rhsLines = new List<string>();
            var rangesLines = new List<string>();
            var boundsLines = new List<string>();
            using (var sr = new StreamReader(mpsFileName, Encoding.UTF8))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (RE_MPS_NAME_SECTION.IsMatch(line))
                    {
                        nameLine = line;
                        break;
                    }
                }
                if (nameLine == null)
                {
                    throw new MPSException(
                        $"No NAME section in file '{mpsFileName}' !"
                    );
                }

                List<string> activeList = null;
                var hasROWS = false;
                while ((line = sr.ReadLine()) != null)
                {
                    if (RE_MPS_ROWS_SECTION.IsMatch(line))
                    {
                        hasROWS = true;
                        activeList = rowsLines;
                        break;
                    }
                }
                if (!hasROWS)
                {
                    throw new MPSException(
                        $"No ROWS section in file '{mpsFileName}' !"
                    );
                }

                var hasCOLUMNS = false;
                while ((line = sr.ReadLine()) != null)
                {
                    if (
                        string.IsNullOrWhiteSpace(line)
                        || line.StartsWith("*")
                        || line.StartsWith("&")
                    )
                    {
                        continue;
                    }
                    else if (RE_MPS_COLUMNS_SECTION.IsMatch(line))
                    {
                        hasCOLUMNS = true;
                        activeList = columnsLines;
                        break;
                    }
                    activeList.Add(line);
                }
                if (!hasCOLUMNS)
                {
                    throw new MPSException(
                        $"No COLUMNS section in file '{mpsFileName}' !"
                    );
                }
                else if (rowsLines.Count == 0)
                {
                    throw new MPSException(
                        $"No ROWS data in file '{mpsFileName}' !"
                    );
                }

                var hasRHS = false;
                while ((line = sr.ReadLine()) != null)
                {
                    if (
                        string.IsNullOrWhiteSpace(line)
                        || line.StartsWith("*")
                        || line.StartsWith("&")
                    )
                    {
                        continue;
                    }
                    else if (RE_MPS_RHS_SECTION.IsMatch(line))
                    {
                        hasRHS = true;
                        activeList = rhsLines;
                        break;
                    }
                    activeList.Add(line);
                }
                if (!hasRHS)
                {
                    throw new MPSException(
                        $"No RHS section in file '{mpsFileName}' !"
                    );
                }
                else if (columnsLines.Count == 0)
                {
                    throw new MPSException(
                        $"No COLUMNS data in file '{mpsFileName}' !"
                    );
                }

                var hasENDATA = false;
                while ((line = sr.ReadLine()) != null)
                {
                    if (
                        string.IsNullOrWhiteSpace(line)
                        || line.StartsWith("*")
                        || line.StartsWith("&")
                    )
                    {
                        continue;
                    }
                    else if (RE_MPS_RANGES_SECTION.IsMatch(line))
                    {
                        activeList = rangesLines;
                        continue;
                    }
                    else if (RE_MPS_BOUNDS_SECTION.IsMatch(line))
                    {
                        activeList = boundsLines;
                        continue;
                    }
                    else if (RE_MPS_ENDATA_SECTION.IsMatch(line))
                    {
                        hasENDATA = true;
                        break;
                    }
                    activeList.Add(line);
                }
                if (!hasENDATA)
                {
                    throw new MPSException(
                        $"No ENDATA section in file '{mpsFileName}' !"
                    );
                }
            }

            var mpsName = ExtractName(nameLine);
            var mpsRows = ExtractRows(rowsLines);
            var (mpsColumns, mpsElements) = ExtractColumns(
                columnsLines, 
                mpsRows
            );
            var mpsRHS = rhsLines.Count == 0 
                ? null 
                : ExtractRHS(rhsLines, mpsRows);
            var mpsRanges = rangesLines.Count == 0 
                ? null 
                : ExtractRanges(rangesLines, mpsRows);
            var mpsBounds = boundsLines.Count == 0 
                ? null 
                : ExtractBounds(boundsLines, mpsColumns);

            _mpsProblemName = mpsName;
            _mpsRows = mpsRows;
            _mpsColumns = mpsColumns;
            _mpsElements = mpsElements;
            _mpsRHS = mpsRHS;
            _mpsRanges = mpsRanges;
            _mpsBounds = mpsBounds;
        }

        /**
         * <summary>
         * Gets the MPS problem name.
         * </summary>
         * 
         * <returns>
         * string
         * </returns>
         */
        public string GetName()
        {
            return (_mpsProblemName);
        }

        /**
         * <summary>
         * Get the MPS rows data.
         * </summary>
         * 
         * <returns>
         * Dictionary<string, (int, string)>
         * </returns>
         * */
        public Dictionary<string, (int, string)>
            GetRows()
        {
            return (_mpsRows);
        }

        /**
         * <summary>
         * Gets the MPS columns data.
         * </summary>
         * 
         * <returns>
         * Dictionary<string, int>
         * </returns>
         */
        public Dictionary<string, int>
            GetColumns()
        {
            return (_mpsColumns);
        }

        /**
         * <summary>
         * Gets the non-zero elements found in the MPS file.
         * </summary>
         * 
         * <returns>
         * Dictionary<(int, int), double>
         * </returns>
         */
        public Dictionary<(int, int), double>
            GetElements()
        {
            return (_mpsElements);
        }

        /**
         * <summary>
         * Gets the MPS right-hand side or returns null.
         * </summary>
         * 
         * <returns>
         * Dictionary<string, double> or null
         * </returns>
         */
        public Dictionary<string, double>
            GetRHS()
        {
            return (_mpsRHS);
        }

        /**
         * <summary>
         * Gets the MPS ranges data or returns null.
         * </summary>
         * 
         * <returns>
         * Dictionary<string, double> or null
         * </returns>
         */
        public Dictionary<string, double>
            GetRanges()
        {
            return (_mpsRanges);
        }

        /**
         * <summary>
         * Gets the MPS bounds data or returns null.
         * </summary>
         * 
         * <returns>
         * Dictionary<string, List<(string, double)>> or null
         * </returns>
         */
        public Dictionary<string, List<(string, double)>>
            GetBounds()
        {
            return (_mpsBounds);
        }

        /**
         * <summary>
         * Specific string representation for objects of this class.
         * </summary>
         * 
         * <returns>
         * This object as a string.
         * </returns>
         */
        public override string ToString()
        {
            return (
                "<CMPS: "
                + $"name='{_mpsProblemName}', "
                + $"rows={_mpsRows}, "
                + $"columns={_mpsColumns}, "
                + $"elements={_mpsElements}, "
                + $"rhs={_mpsRHS}, "
                + $"ranges={_mpsRanges}, "
                + $"bounds={_mpsBounds}"
                + ">"
            );
        }
    }
}


////////////////////////////////////////////////////////////////////////////////
// END
////////////////////////////////////////////////////////////////////////////////
// Local Variables:
// mode: csharp
// End:
