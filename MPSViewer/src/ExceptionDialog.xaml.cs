/**
 * @file ExceptionDialog.xaml.cs
 * @brief C# source code for a modal dialog box to show exception details.
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


//////////////////////////////////////////////////////////////////////////////
// MODAL DIALOG
//////////////////////////////////////////////////////////////////////////////

namespace MPSViewer
{
    /**
     * <summary>
     * Interaction logic for ExceptionDialog.xaml
     * </summary>
     */
    public sealed partial class ExceptionDialog : Window
    {
        /**
         * <summary>
         * The sole constructor for this class.
         * </summary>
         * 
         * <param name="type">
         * Type of the exception.
         * </param>
         * 
         * <param name="message">
         * Message attached to the exception.
         * </param>
         * 
         * <param name="stackTrace">
         * Stack trace associated with the exception.
         * </param>
         */
        public ExceptionDialog(
            string type,
            string message,
            string stackTrace
        )
        {
            InitializeComponent();

            Title += $" - {type}";
            exceptionMessage.Text = message;
            exceptionStackTrace.Text = stackTrace;
        }

        private void Window_Unloaded(object sender, EventArgs ae)
        {
        }
    }
}


//////////////////////////////////////////////////////////////////////////////
// END
//////////////////////////////////////////////////////////////////////////////
// Local Variables:
// mode: csharp
// End:
