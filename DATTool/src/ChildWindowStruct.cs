////////////////////////////////////////////////////////////////////////////////
// FILE     : ChildWindowStruct.cs
// SYNOPSIS : C# struct for for setting up the child window.
// LICENSE  : MIT
////////////////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////////////////
// STRUCT
////////////////////////////////////////////////////////////////////////////////

namespace DATTool
{
    /// <summary>
    /// A struct to help set up a child window in this application.
    /// </summary>
    internal readonly struct ChildWindowStruct
    {
        ////////////////////////////////////////////////////////////////////////
        // STRUCT DATA
        ////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// String to add to the title in the child window title bar.
        /// </summary>
        readonly public string titleExtension;

        /// <summary>
        /// The x position of the top left corner.
        /// </summary>
        readonly public int topLeftX;

        /// <summary>
        /// The y position of the top left corner.
        /// </summary>
        readonly public int topLeftY;

        /// <summary>
        /// The window width.
        /// </summary>
        readonly public int width;

        /// <summary>
        /// The window height.
        /// </summary>
        readonly public int height;

        ////////////////////////////////////////////////////////////////////////
        // STRUCT FUNCTIONS
        ////////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * The fully qualified constructor for this structure. 
         * </summary>
         * 
         * <param name="titleExt">
         * String to append to the title in the window title bar.
         * </param>
         * 
         * <param name="tlX">
         * Top left corner x position.
         * </param>
         *
         * <param name="tlY">
         * Top left corner y position.
         * </param>
         * 
         * <param name="w">
         * Width of window in pixels.
         * </param>
         * 
         * <param name="h">
         * Height of window in pixels.
         * </param>
         */
        public ChildWindowStruct(
            string titleExt, 
            int tlX, 
            int tlY, 
            int w, 
            int h
        )
        {
            titleExtension = titleExt;
            topLeftX = tlX;
            topLeftY = tlY;
            width = w;
            height = h;
        }

        /**
         * <summary>
         * Specific string representation for this structure.
         * </summary>
         * 
         * <returns>
         * String for this structure.
         * </returns>
         */
        public override string ToString()
        {
            return (
                "<StructWindowDimensions: "
                + $"titleExt='{titleExtension}', "
                + $"topLeftX={topLeftX}, "
                + $"topLeftY={topLeftY}, "
                + $"width={width}, "
                + $"height={height}"
                + ">"
            );
        }
    }
}


////////////////////////////////////////////////////////////////////////////////
// END
////////////////////////////////////////////////////////////////////////////////
/**
 * @file
 * @brief C# struct for for setting up the child window.
 *
 * @author Mohammad Haroon Khaliq
 * @date @showdate "%d %B %Y"
 * @copyright MIT License.
 */
// Local Variables:
// mode: csharp
// End:
