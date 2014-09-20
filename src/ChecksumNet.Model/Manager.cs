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
        }

        private void SendData(byte[] data)
        {
            provider.Send(data);
        }

        public void ReceiveData(byte[] data)
        {
                RemoteHost.Checksum = data;
                CompareChecksums();
        }
        public void CompareChecksums()
        {
            Console.WriteLine("MD5 of remote computer: "+BitConverter.ToString(RemoteHost.Checksum).Replace("-","").ToLower());
        }

        public HostData Sender { get; set; }
        public HostData RemoteHost { get; set; }
    }
}
