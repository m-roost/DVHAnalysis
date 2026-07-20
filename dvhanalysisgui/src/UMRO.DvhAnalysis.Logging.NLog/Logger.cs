using System;
using NLog;
using NLog.Config;

namespace UMRO.DvhAnalysis.Logging.NLog
{
    public class Logger : ILogger
    {
        private readonly global::NLog.Logger _logger;

        public Logger(string name, string configPath)
        {
            // Load configuration manually (see https://stackoverflow.com/a/16062987/1383366)
            // The reason we do this is to ensure that the correct configuration file
            // is loaded for a plug-in script; otherwise, another script's config file may be used
            LogManager.Configuration = new XmlLoggingConfiguration(configPath);
            _logger = LogManager.GetLogger(name);
        }

        // Lightweight constructor for use after the configuration has already been loaded
        // (e.g., via AppLog.Configure). It does NOT reload the config, so any class can obtain
        // its own named logger cheaply without re-parsing NLog.config.
        public Logger(string name)
        {
            _logger = LogManager.GetLogger(name);
        }

        public void Trace(string message) => _logger.Trace(message);

        public void Trace(string message, Exception exception) => _logger.Trace(exception, message);

        public void Debug(string message) => _logger.Debug(message);

        public void Debug(string message, Exception exception) => _logger.Debug(exception, message);

        public void Info(string message) => _logger.Info(message);

        public void Info(string message, Exception exception) => _logger.Info(exception, message);

        public void Warn(string message) => _logger.Warn(message);

        public void Warn(string message, Exception exception) => _logger.Warn(exception, message);

        public void Error(string message) => _logger.Error(message);

        public void Error(string message, Exception exception) => _logger.Error(exception, message);

        public void Fatal(string message) => _logger.Fatal(message);

        public void Fatal(string message, Exception exception) => _logger.Fatal(exception, message);
    }

    // Application-wide logging facade. Configure once at startup (Script.Run), then any class
    // can obtain a logger via AppLog.GetLogger(...). Keeps NLog specifics out of the callers.
    public static class AppLog
    {
        // Loads the NLog configuration and (optionally) publishes the log directory so the
        // NLog.config file target can reference it as ${gdc:item=logDir}.
        public static void Configure(string configPath, string logDir = null)
        {
            LogManager.Configuration = new XmlLoggingConfiguration(configPath);

            if (!string.IsNullOrWhiteSpace(logDir))
            {
                GlobalDiagnosticsContext.Set("logDir", logDir);
            }
        }

        public static ILogger GetLogger(string name) => new Logger(name);
    }
}
