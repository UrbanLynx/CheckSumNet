using System;
using NLog;

namespace ChecksumNet.Model
{
    public class ModelManager
    {
        private NetProvider provider = new NetProvider();
        private Authentication authentication = new Authentication();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ModelManager()
        {
            logger.Info("Application is running.");
            LocalHost = new HostData();
        }

        public void SetConnection()
        {
            provider.SetConnection();
            LocalHost = new HostData(provider.LocalEP);
            RemoteHost = new HostData(provider.RemoteEP);
            IsConnected = true;
            DataUpdate();
        }

        /*public void StartListening()
        {
            provider.onDataReceived += ReceiveData;
            //provider.StartListening();
        }*/
        public void NewChecksum(string filename)
        {
            LocalHost.Checksum = Checksum.CalculateChecksum(filename);
            CompareChecksums();
            provider.Send(LocalHost.Checksum);
            //SendData();
            DataUpdate();
        }

        /*private void SendData(string data)
        {
            provider.Send(data);
        }*/

        /*public void ReceiveData(byte[] data)
        {
            //RemoteHost.Checksum = data;
            //CompareChecksums();
            DataUpdate();
        }*/

        public void CompareChecksums()
        {
            if (RemoteHost != null && RemoteHost.Checksum != null && LocalHost != null && LocalHost.Checksum != null)
            {
                IsChecksumsEqual = (LocalHost.Checksum == RemoteHost.Checksum);
            }
            else
            {
                IsChecksumsEqual = false;
            }
            
            //Console.WriteLine("MD5 of remote computer: "+BitConverter.ToString(RemoteHost.Checksum).Replace("-","").ToLower());
        }

        public bool TryLogin(string username, string password)
        {
            if (authentication.Login(username, password))
            {
                provider = new NetProvider(username);
                return true;
            }
            
            return false;
        }
    
        public HostData LocalHost { get; set; }
        public HostData RemoteHost { get; set; }
        public bool IsConnected { get; set; }
        public bool IsChecksumsEqual { get; set; }

        public delegate void UpdateData();
        public event UpdateData DataUpdate;

    }
}
