using System;
using System.Collections.Generic;
using NLog;

namespace ChecksumNet.Model
{
    public class ModelManager
    {
        #region Members

        private NetProvider provider = new NetProvider(); // управление сетевым соединением и обменом данными
        private Authentication authentication = new Authentication(); // управление авторизацией
        private static Logger logger = LogManager.GetCurrentClassLogger(); // управление журналированием событий и ошибок

        #endregion

        #region Constructors

        public ModelManager()
        {
            logger.Info("КТО: данный компьютер. ЧТО: запуск приложения. РЕЗУЛЬТАТ: успешно.");
            provider.OnDataReceived += ProviderOnOnDataReceived;
            provider.OnNewPeers += ProviderOnOnNewPeers;
        }

        #endregion

        #region Properties

        // События получения новых данных или информации о равноправных участниках. Могут использоваться для интерфейсной части программы
        public delegate void DataChanged();
        public event DataChanged OnDataUpdate;

        public delegate void NewPeer(PeerEntry peerEntry);
        public event NewPeer OnNewPeer;

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

        // обновление данных о других равноправных участниках
        public void RefreshHosts()
        {
            provider.RefreshHosts();
            OnDataUpdate();
        }

        // регистрация данного приложения как равноправного участника
        public void RegisterHost()
        {
            provider.RegisterHost(authentication.GetUsername());
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

        // подсчет и отправка контрольной суммы файла
        public void NewChecksum(string filename)
        {
            provider.LocalPeer.Checksum = Checksum.CalculateChecksum(filename);
            logger.Info("КТО: пользователь {0}. ЧТО: попытка подсчета контрольной суммы. РЕЗУЛЬТАТ: успешно. MD5: {1}",
                authentication.GetUsername(), provider.LocalPeer.Checksum);
            provider.Send(provider.LocalPeer.Checksum);
            OnDataUpdate();
        }

        // Попытка аутентификации с логином и паролем
        public bool TryLogin(string username, string password)
        {
            if (authentication.Login(username, password))
            {
                logger.Info("КТО: пользователь {0}. ЧТО: попытка авторизации. РЕЗУЛЬТАТ: успешно", username);
                RegisterHost();
                return true;
            }

            return false;
        }

        #endregion
    }
}