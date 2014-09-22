using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChecksumNet.Model
{
    public class Manager
    {
        private NetProvider provider = new NetProvider();

        public Manager()
        {
            Sender = new HostData();
        }

        public void SetConnection()
        {
            provider.SetConnection();
            RemoteHost = new HostData(provider.RemoteEP);
            DataUpdate();
        }

        public void StartListening()
        {
            provider.onDataReceived += ReceiveData;
            provider.StartListening();
        }
        public void NewChecksum(string filename)
        {
            Sender.Checksum = Checksum.CalculateChecksum(filename);
            //CompareChecksums();
            SendData(Sender.Checksum);
            DataUpdate();
        }

        private void SendData(byte[] data)
        {
            provider.Send(data);
        }

        public void ReceiveData(byte[] data)
        {
            RemoteHost.Checksum = data;
            CompareChecksums();
            DataUpdate();
        }
        public void CompareChecksums()
        {
            Console.WriteLine("MD5 of remote computer: "+BitConverter.ToString(RemoteHost.Checksum).Replace("-","").ToLower());
        }

        public HostData Sender { get; set; }
        public HostData RemoteHost { get; set; }

        public delegate void UpdateData();

        public event UpdateData DataUpdate;

        private Authentication authentication = new Authentication();

        public void Comare(string inputLogin, string password)
        {
            bool isAuthentication = authentication.AuthenticationCompare(inputLogin, password);

        }

    }
}
