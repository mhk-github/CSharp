/**
 * @file App.xaml.cs
 * @brief Implementation C# file for this application.
 *
 * @author Mohammad Haroon Khaliq
 * @date @showdate "%d %B %Y"
 * @copyright MIT License.
 */


////////////////////////////////////////////////////////////////////////////////
// IMPORTS
////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using System.Windows;


////////////////////////////////////////////////////////////////////////////////
// APPLICATION
////////////////////////////////////////////////////////////////////////////////

/**
 * <summary>
 * The namespace for this application.
 * </summary>
 */
namespace ProcessMaster
{
    /**
     * <summary>
     * Interaction logic for App.xaml
     * </summary>
     */
    public sealed partial class App : Application
    {
        ////////////////////////////////////////////////////////////////////////
        // CONSTANTS
        ////////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * Name of a local mutex to ensure only one application instance runs.
         * </summary>
         */
        private const string LOCAL_MUTEX_NAME = "ProcessMasterCsharpWPF";


        ////////////////////////////////////////////////////////////////////////
        // STATIC
        ////////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * A local mutex to ensure only one application instance runs.
         * </summary>
         */
        private static Mutex s_mutexSingleInstance = null;


        ////////////////////////////////////////////////////////////////////////
        // MEMBER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////

        /**
         * <summary>
         * Runs on application startup to ensure only one application instance.
         * </summary>
         * 
         * <param name="e">
         * Arguments connected to this event.
         * </param>
         */
        protected override void OnStartup(StartupEventArgs e)
        {
            s_mutexSingleInstance = new Mutex(
                true,
                LOCAL_MUTEX_NAME,
                out bool bCreated
            );
            if (!bCreated)
            {
                Current.Shutdown();
            }

            base.OnStartup(e);
        }

    }

}


////////////////////////////////////////////////////////////////////////////////
// END
////////////////////////////////////////////////////////////////////////////////
/**
 * @mainpage %ProcessMaster
 *
 * A C# WPF .NET Framework application for setting process priorities.
 */
// Local Variables:
// mode: csharp
// End:
