using System;
using System.Collections.Generic;
using NLog;

namespace ChecksumNet.Model
{
    public class ModelManager
    {
        #region Members

        private NetProvider provider = new NetProvider();
        private Authentication authentication = new Authentication();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        public ModelManager()
        {
            logger.Info("Application started.");
            provider.OnDataReceived += ProviderOnOnDataReceived;
            provider.OnNewPeers += ProviderOnOnNewPeers;
        }

        #endregion

        #region Properties

        public bool IsRegistered { get; set; }

        public delegate void DataChanged();

        public event DataChanged OnDataUpdate;

        public delegate void NewPeer(PeerEntry peerEntry);

        public event NewPeer OnNewPeer;
        //public event UpdateData OnNewPeer;

        #endregion

        #region Methods

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
            logger.Info("Start calculating checksum ...");
            provider.LocalPeer.Checksum = Checksum.CalculateChecksum(filename);
            logger.Info("Checksum is successfully calculated. MD5: {0}", provider.LocalPeer.Checksum);
            provider.Send(provider.LocalPeer.Checksum);
            OnDataUpdate();
        }

        public bool TryLogin(string username, string password)
        {
            logger.Info("Attempt to login with username: '{0}'", username);
            if (authentication.Login(username, password))
            {
                logger.Info("User '{0}' is successfully loged in", username);
                RegisterHost();
                return true;
            }

            return false;
        }

        #endregion
    }
}
