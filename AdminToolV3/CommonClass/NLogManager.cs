using NLog;
using NLog.Fluent;
using NLog.Web;
using System.Text;

namespace AdminToolV3.CommonClass
{
    public class NLogManager : Singleton<NLogManager>
    {
        private readonly Logger logger;

        public NLogManager()
        {
            logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
        }

        public void Log(string log)
        {
            logger.Log(NLog.LogLevel.Error, log);
        }
    }
}
