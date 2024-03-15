/**
 * @file ChildWindowHistogram1.xaml.cs
 * @brief C# source code for a child window displaying a histogram of absolute
 * values.
 *
 * @author Mohammad Haroon Khaliq
 * @date @showdate "%d %B %Y"
 * @copyright MIT License.
 */


//////////////////////////////////////////////////////////////////////////////
// IMPORTS
//////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;

using ScottPlot.Statistics;
using log4net;


//////////////////////////////////////////////////////////////////////////////
// CHILD WINDOW
//////////////////////////////////////////////////////////////////////////////

namespace MPSViewer
{
    /**
     * <summary>
     * Interaction logic for ChildWindowHistogram1.xaml.
     * </summary>
     */
    public sealed partial class ChildWindowHistogram1 : Window
    {
        //////////////////////////////////////////////////////////////////////
        // CONSTANTS
        //////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * Number of buckets the histogram uses.
         * </summary>
         */
        private const int NUM_BUCKETS = 11;

        //////////////////////////////////////////////////////////////////////
        // MEMBER DATA
        //////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * The logging system.
         * </summary>
         */
        private readonly ILog _iLog;

        //////////////////////////////////////////////////////////////////////
        // MEMBER FUNCTIONS
        //////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * The sole constructor for this class.
         * </summary>
         *
         * <param name="fileName">
         * Name of the file.
         * </param>
         *
         * <param name="mps">
         * MPS data.
         * </param>
         *
         * <param name="topLeftX">
         * Child window top left x-coordinate.
         * </param>
         *
         * <param name="topLeftY">
         * Child window top left y-coordinate.
         * </param>
         *
         * <param name="iLog">
         * Logging system used throughout the application.
         * </param>
         */
        public ChildWindowHistogram1(
            string fileName,
            CMPS mps,
            int topLeftX,
            int topLeftY,
            ILog iLog
        )
        {
            InitializeComponent();

            _iLog = iLog;
#if DEBUG
            _iLog.Info("    MPSViewer (Histogram 1) [DEBUG] - Start");
#else
            _iLog.Info("    MPSViewer (Histogram 1) - Start");
#endif
            _iLog.Debug(
                $"      Enter - ChildWindowHistogram1({fileName}, "
                + $"{mps.GetType()}, {topLeftX}, {topLeftY}, {iLog})"
            );

#if DEBUG
            Title += $" [DEBUG] - {fileName}";
#else
            Title += $" - {fileName}";
#endif
            _iLog.Debug($"        Child window title set to '{Title}'");

            Left = topLeftX;
            Top = topLeftY;
            _iLog.Debug(
                $"        Child window position set to ({topLeftX}, "
                + $"{topLeftY})"
            );

            var elements = mps.GetElements();
            var data = new double[elements.Count];
            var values = elements.Values;
            var max = 0.0;
            var i = 0;
            foreach (var v in values)
            {
                var item = Math.Log10(Math.Abs(v));
                if (item > max)
                {
                    max = item;
                }
                data[i] = item;
                i++;
            }
            var hist = new Histogram(0.0, max, NUM_BUCKETS);
            foreach (var e in data)
            {
                hist.Add(e);
            }

            histogram1.Plot.Add.Bars(hist.Bins, hist.Counts);
            histogram1.Plot.YLabel("Count");
            histogram1.Plot.XLabel("Log10(Absolute Value)");

            _iLog.Debug("      Leave - ChildWindowHistogram1(...)");
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
            _iLog.Info("    MPSViewer (Histogram 1) - End");
        }
    }
}


//////////////////////////////////////////////////////////////////////////////
// END
//////////////////////////////////////////////////////////////////////////////
// Local Variables:
// mode: csharp
// End:
