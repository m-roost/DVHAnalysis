using EclipsePlugInRunner.Scripting;
using System.IO;
using System.Reflection;
using System.Windows;
using UMRO.DvhAnalysis.Logging.NLog;
using VMS.TPS.Common.Model.API;

namespace UMRO.DvhAnalysis.Runner
{
    public partial class App : System.Windows.Application
    {
        private Logging.ILogger _logger;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configPath = Path.Combine(assemblyDir, "NLog.config");
            AppLog.Configure(configPath);

            _logger = AppLog.GetLogger("MainLogger");

            _logger.Info("\n\n\n\n\n\n\n\n");

            _logger.Info("Script RUNNER started");

            ScriptRunner.Run(new VMS.TPS.Script());

            _logger.Info("Script RUNNER started");

        }

        // Dummy method to force referencing ESAPI (fixes an exception)
        public void Dummy(PlanSetup plan) { }
    }
}
