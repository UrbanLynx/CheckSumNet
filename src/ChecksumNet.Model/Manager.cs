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
            LocalHost = new HostData();
        }

        public void SetConnection()
        {
            provider.SetConnection();
            LocalHost = new HostData(provider.LocalEP);
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
            LocalHost.Checksum = Checksum.CalculateChecksum(filename);
            //CompareChecksums();
            SendData(LocalHost.Checksum);
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

        public HostData LocalHost { get; set; }
        public HostData RemoteHost { get; set; }

        public delegate void UpdateData();

        public event UpdateData DataUpdate;
    }
}
