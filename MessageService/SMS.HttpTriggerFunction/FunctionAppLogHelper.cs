using System.IO;
using NLog;
using NLog.Config;

namespace SMS.HttpTriggerFunction
{
    public static class FunctionAppLogHelper

    {
        public static void GetLogConfiguration(ILogger log, string path)

        {
            if (log == null)
            {

                var xmlConfigPath = Path.Combine(path, "nlog.config");

                LogManager.Configuration = new XmlLoggingConfiguration(xmlConfigPath);

            }

        }

    }
}
