using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.PeerToPeer;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChecksumNet.Model
{
    [ServiceContract]
    public interface IP2PService
    {
        event EventHandler<ReceivedDataEventArgs> ReceivedData;

        [OperationContract]
        string GetName();

        [OperationContract(IsOneWay = true)]
        void SendMessage(string message, PeerName from);
    }

    public class ReceivedDataEventArgs : EventArgs
    {
        public ReceivedDataEventArgs(string data, PeerName from)
        {
            Data = data;
            FromPeer = from;
        }
        public string Data { get; private set; }
        public PeerName FromPeer { get; private set; }
    }
}
