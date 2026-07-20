using System;
using System.Windows.Threading;

namespace UMRO.DvhAnalysis.Script
{
    // Uses the UI's Dispatcher (set by the main window)
    // to run actions with a priority of Background
    public static class BackgroundRunner
    {
        // This must be set by the main window before use
        public static Dispatcher Dispatcher { get; set; }

        public static void Run(Action action)
        {
            if (Dispatcher == null)
            {
                throw new ApplicationException("No Dispatcher set.");
            }

            Dispatcher.BeginInvoke(action, DispatcherPriority.Background, null);
        }
    }
}
