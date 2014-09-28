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
        }

        private void ProviderOnOnNewPeers(PeerEntry peerEntry)
        {
            OnNewPeer(peerEntry);
        }

        private void ProviderOnOnDataReceived()
        {
            OnDataUpdate();
        }

        public void RefreshHosts()
        {
            provider.RefreshHosts();
            OnDataUpdate();
        }

        public void RegisterHost()
        {
            provider.RegisterHost(authentication.GetUsername());
            IsRegistered = true;
            OnDataUpdate();
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
            provider.Send(provider.LocalPeer.Checksum);
            OnDataUpdate();
        }

        public bool TryLogin(string username, string password)
        {
            if (authentication.Login(username, password))
            {
                RegisterHost();
                return true;
            }
            
            return false;
        }
    
        public bool IsRegistered { get; set; }

        public delegate void DataChanged();
        public event DataChanged OnDataUpdate;

        public delegate void NewPeer(PeerEntry peerEntry);
        public event NewPeer OnNewPeer;
        //public event UpdateData OnNewPeer;
    }
}
