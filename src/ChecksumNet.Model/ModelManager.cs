using System;
using System.Collections.Generic;
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
            provider.OnDataReceived += ProviderOnOnDataReceived;
            provider.OnNewPeers += ProviderOnOnNewPeers;
            //provider.SetConnection();
        }

        private void ProviderOnOnNewPeers(PeerEntry peerEntry)
        {
            OnNewPeer(peerEntry);
        }

        private void ProviderOnOnDataReceived()
        {
            OnDataUpdate();
        }

        public void SetConnection()
        {

            provider.SetConnection();
            IsConnected = true;
            OnDataUpdate();
        }

        public void RefreshPeers()
        {
            provider.RefreshHosts();
            //OnPeerRefresh();
        }

        public PeerEntry GetLocalPeer()
        {
            return provider.LocalPeer;
        }

        public List<PeerEntry> GetPeers()
        {
            return provider.PeerList;
        } 

        /*public void StartListening()
        {
            provider.onDataReceived += ReceiveData;
            //provider.StartListening();
        }*/
        public void NewChecksum(string filename)
        {
            provider.LocalPeer.Checksum = Checksum.CalculateChecksum(filename);
            CompareChecksums();
            provider.Send(provider.LocalPeer.Checksum);
            OnDataUpdate();
        }

        public void CompareChecksums()
        {
            /*if (RemoteHost != null && RemoteHost.Checksum != null && LocalHost != null && LocalHost.Checksum != null)
            {
                IsChecksumsEqual = (LocalHost.Checksum == RemoteHost.Checksum);
            }
            else
            {
                IsChecksumsEqual = false;
            }*/
            
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
    
        public bool IsConnected { get; set; }
        public bool IsChecksumsEqual { get; set; }

        public delegate void DataChanged();
        public event DataChanged OnDataUpdate;

        public delegate void NewPeer(PeerEntry peerEntry);
        public event NewPeer OnNewPeer;
        //public event UpdateData OnNewPeer;
    }
}
