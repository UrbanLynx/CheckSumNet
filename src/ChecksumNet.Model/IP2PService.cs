using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.PeerToPeer;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChecksumNet.Model
{
    // внешний интерфейс равноправного участника сети, контракт сервиса WCF
    [ServiceContract]
    public interface IP2PService 
    {
        event EventHandler<ReceivedDataEventArgs> ReceivedData; // событие получения данных

        // публичные операции локального сервиса, контракты операций WCF
        [OperationContract]
        string GetName(); // получение имени пользователя 

        [OperationContract(IsOneWay = true)]
        void SendMessage(string message, PeerName from); // отправка сообщения пользователю
    }

    // передаваемые аргументы, контракт данных WCF
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
