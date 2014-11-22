using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.PeerToPeer;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace ChecksumNet.Model
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class P2PService : IP2PService
    {
        private string username;

        public P2PService(string username)
        {
            this.username = username;
        }

        public event EventHandler<ReceivedDataEventArgs> ReceivedData;

        public string GetName()
        {
            return username;
        }

        public void SendMessage(string message, PeerName from)
        {
            ReceivedData(from, new ReceivedDataEventArgs(message, from));
        }
    }
}
