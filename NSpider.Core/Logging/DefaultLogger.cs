using NLog;
using NLog.Config;

namespace NSpider.Core.Logging
{
    public class DefaultLogger : ILogger
    {
        private Logger Log { get; }

        public DefaultLogger()
        {
            LogManager.Configuration =
                new XmlLoggingConfiguration("NLog.config");

            Log = LogManager.GetLogger("logger");
        }

        public void Debug(string message)
        {
            Log.Debug(message);
        }

        public void Error(string message)
        {
            Log.Error(message); 
        }

        public void Fatal(string message)
        {
            Log.Fatal(message);
        }

        public void Info(string message)
        {
            Log.Info(message);
        }

        public void Warn(string message)
        {
            Log.Warn(message);
        }
    }
}
