/**
 * @file ChildWindowKernel.xaml.cs
 * @brief C# source code for a child window displaying MPS kernels.
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
using System.Windows.Media;
using System.Windows.Media.Imaging;

using log4net;


//////////////////////////////////////////////////////////////////////////////
// CHILD WINDOW
//////////////////////////////////////////////////////////////////////////////

namespace MPSViewer
{
    /**
     * <summary>
     * Interaction logic for ChildWindowKernel.xaml.
     * </summary>
     */
    public sealed partial class ChildWindowKernel : Window
    {
        //////////////////////////////////////////////////////////////////////
        // CONSTANTS
        //////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * 24-bit RGB (8bpp) pixel format for the client area bitmap.
         * </summary>
         */
        private static readonly PixelFormat PF = PixelFormats.Rgb24;

        /**
         * <summary>
         * DPI for the horizontal axis.
         * </summary>
         */
        private const double DPI_X = 96.0;

        /**
         * <summary>
         * DPI for the vertical axis.
         * </summary>
         */
        private const double DPI_Y = 96.0;

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
         * <param name="maxWidth">
         * Maximum width of the client area of the child window.
         * </param>
         *
         * <param name="maxHeight">
         * Maximum height of the client area of the child window.
         * </param>
         *
         * <param name="widthBoost">
         * Number of pixels to add to the width for sizing the window.
         * </param>
         *
         * <param name="heightBoost">
         * Number of pixels to add to the width for sizing the window.
         * </param>
         *
         * <param name="iLog">
         * Logging system used throughout the application.
         * </param>
         */
        public ChildWindowKernel(
            string fileName,
            CMPS mps,
            int topLeftX,
            int topLeftY,
            int maxWidth,
            int maxHeight,
            int widthBoost,
            int heightBoost,
            ILog iLog
        )
        {
            InitializeComponent();

            _iLog = iLog;
#if DEBUG
            _iLog.Info("    MPSViewer (Kernel) [DEBUG] - Start");
#else
            _iLog.Info("    MPSViewer (Kernel) - Start");
#endif
            _iLog.Debug(
                $"      Enter - ChildWindowKernel({fileName}, {mps.GetType()},"
                + $" {topLeftX}, {topLeftY}, {maxWidth}, {maxHeight}, "
                + $"{widthBoost}, {heightBoost}, {iLog})"
            );

            // ...
            double calculatedWidth = mps.GetColumns().Count;
            double calculatedHeight = mps.GetRows().Count;
            _iLog.Debug($"        True width = {calculatedWidth}");
            _iLog.Debug($"        True height = {calculatedHeight}");
            var widthFactor = 1.0;
            var heightFactor = 1.0;
            if (calculatedWidth > maxWidth)
            {
                var reductionFactor = maxWidth / calculatedWidth;
                widthFactor = reductionFactor;
                heightFactor = reductionFactor;
                calculatedWidth = maxWidth;
                calculatedHeight *= reductionFactor;
            }

            if (calculatedHeight > maxHeight)
            {
                var reductionFactor = maxHeight / calculatedHeight;
                widthFactor *= reductionFactor;
                heightFactor *= reductionFactor;
                calculatedWidth *= reductionFactor;
                calculatedHeight = maxHeight;
            }
            _iLog.Debug($"        Calculated width = {calculatedWidth}");
            _iLog.Debug($"        Calculated height = {calculatedHeight}");
            _iLog.Debug($"        Width factor = {widthFactor}");
            _iLog.Debug($"        Height factor = {heightFactor}");

            var width = (int)calculatedWidth + 1;
            var height = (int)calculatedHeight + 1;
            _iLog.Debug($"        Width assigned = {width}");
            _iLog.Debug($"        Height assigned = {height}");

            var rawStride = ((width * PF.BitsPerPixel) + 7) / 8;
            var rawImage = new byte[rawStride * height];
            _iLog.Debug($"        PixelFormat bpp = {PF.BitsPerPixel}");
            _iLog.Debug($"        Bitmap raw stride = {rawStride}");
            _iLog.Debug($"        Image byte array size = {rawImage.Length}");
            foreach (var (rowId, columnId) in mps.GetElements().Keys)
            {
                var row_y_in_bytes = ((int)(rowId * heightFactor) * rawStride);
                var x_pos_in_row_y = ((int)((columnId * widthFactor) * 3));
                var index = row_y_in_bytes + x_pos_in_row_y;
                rawImage[index] = 255;
                rawImage[index + 1] = 255;
                rawImage[index + 2] = 255;
            }

            var bmpSrc = BitmapSource.Create(
                width,
                height,
                DPI_X,
                DPI_Y,
                PF,
                null,
                rawImage,
                rawStride
            );
            kernelImage.Width = width;
            kernelImage.Height = height;
            kernelImage.Source = bmpSrc;

#if DEBUG
            Title += $" [DEBUG] - {fileName}";
#else
            Title += $" - {fileName}";
#endif
            _iLog.Debug($"        Child window title set to '{Title}'");

            Left = topLeftX;
            Top = topLeftY;
            Width = width + widthBoost;
            Height = height + heightBoost;
            _iLog.Debug(
                 "        Child window geometry set to dimensions "
                 + $"{Width}x{Height} at position ({Left}, {Top})"
             );

            _iLog.Debug("      Leave - ChildWindowKernel(...)");
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
            _iLog.Info("    MPSViewer (Kernel) - End");
        }
    }
}


//////////////////////////////////////////////////////////////////////////////
// END
//////////////////////////////////////////////////////////////////////////////
// Local Variables:
// mode: csharp
// End:
