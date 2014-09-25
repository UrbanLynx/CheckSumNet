using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace ChecksumNet.Model
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class P2PService : IP2PService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string username;

        public P2PService(string username)
        {
            this.username = username;
        }

        public string GetName()
        {
            return username;
        }

        public void SendMessage(string message, string from)
        {
            logger.Debug("Сообщение пришло от {0}: {1}", from, message);
            //hostReference.DisplayMessage(message, from);
        }
    }
}
