using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.PeerToPeer;
using System.Net.PeerToPeer.Collaboration;
using System.Net.Sockets;
using System.ServiceModel;
using NLog;

namespace ChecksumNet.Model
{
    public class NetProvider
    {
        #region Members

        private static Logger logger = LogManager.GetCurrentClassLogger();

        // для управления службой wcf и pnrp
        private ServiceHost host;
        private PeerNameRegistration peerNameRegistration;

        #endregion

        #region Properties

        public List<PeerEntry> PeerList = new List<PeerEntry>();
        public PeerEntry LocalPeer = new PeerEntry();

        public delegate void ProcessData();

        public event ProcessData OnDataReceived;

        public delegate void NewPeer(PeerEntry peerEntry);

        public event NewPeer OnNewPeers;

        #endregion

        #region Methods

        public void RegisterHost(string username)
        {
            // Получение конфигурационной информации из app.config
            string port = ConfigurationManager.AppSettings["port"];
            //username = Environment.MachineName;
            //string machineName = Environment.MachineName;
            //string serviceUrl = null;
            logger.Info("Start registering host with username {0} on port {1}", username, port);
            string serviceUrl = string.Format("net.tcp://0.0.0.0:{0}/P2PService", port);

            // Регистрация и запуск службы WCF
            LocalPeer.ServiceProxy = new P2PService(username);
            LocalPeer.ServiceProxy.ReceivedData += ServiceProxyOnReceivedData;
            LocalPeer.DisplayString = LocalPeer.ServiceProxy.GetName();
            host = new ServiceHost(LocalPeer.ServiceProxy, new Uri(serviceUrl));
            var binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;
            host.AddServiceEndpoint(typeof (IP2PService), binding, serviceUrl);
            try
            {
                host.Open();
                // Создание имени равноправного участника (пира)
                LocalPeer.PeerName = new PeerName("P2P Sample", PeerNameType.Unsecured);

                // Подготовка процесса регистрации имени равноправного участника в локальном облаке
                peerNameRegistration = new PeerNameRegistration(LocalPeer.PeerName, int.Parse(port));
                peerNameRegistration.Cloud = Cloud.AllLinkLocal;

                // Запуск процесса регистрации
                peerNameRegistration.Start();

                logger.Info("Registration of host is successfully completed. Host name: {0}. Service URL: {1}.",
                    username, serviceUrl);
            }
            catch (AddressAlreadyInUseException)
            {
                logger.Error("WCF Error: can't begin listening, port '{0}' is used by another program.", port);
            }


        }

        private void ServiceProxyOnReceivedData(object sender, ReceivedDataEventArgs receivedDataEventArgs)
        {
            var fromPeer =
                PeerList.FirstOrDefault(
                    peer => peer.PeerName.PeerHostName == receivedDataEventArgs.FromPeer.PeerHostName);
            if (fromPeer != null)
            {
                fromPeer.Checksum = receivedDataEventArgs.Data;
                logger.Info("New data from peer '{0}'. Data: '{1}'",
                    fromPeer.DisplayString, fromPeer.Checksum);
                OnDataReceived();
            }
            else
            {
                logger.Error("New data from unknown peer '{0}'. Data: '{1}'",
                    receivedDataEventArgs.FromPeer.PeerHostName, receivedDataEventArgs.Data);

            }
        }

        public void StopHost()
        {
            // Остановка регистрации
            peerNameRegistration.Stop();

            // Остановка WCF-сервиса
            host.Close();
        }

        public void RefreshHosts()
        {
            logger.Info("Start refreshing of hosts...");
            // Создание распознавателя и добавление обработчиков событий
            var resolver = new PeerNameResolver();
            resolver.ResolveProgressChanged += resolver_ResolveProgressChanged;
            resolver.ResolveCompleted += resolver_ResolveCompleted;

            // Подготовка к добавлению новых пиров
            PeerList.Clear();
            // TODO: RefreshButton.IsEnabled = false;

            // Преобразование незащищенных имен пиров асинхронным образом
            resolver.ResolveAsync(new PeerName("0.P2P Sample"), 1);
        }

        private void resolver_ResolveCompleted(object sender, ResolveCompletedEventArgs e)
        {
            logger.Info("Refreshing of hosts is completed.");
            // Повторно включаем кнопку "обновить"
            // TODO: RefreshButton.IsEnabled = true;
        }

        private void resolver_ResolveProgressChanged(object sender, ResolveProgressChangedEventArgs e)
        {
            PeerNameRecord peer = e.PeerNameRecord;

            foreach (IPEndPoint ep in peer.EndPointCollection)
            {
                if (ep.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    try
                    {
                        logger.Info("Found new remote peer with IP: {0}:{1}. Start registering new peer ...", ep.Address,
                            ep.Port);
                        string endpointUrl = string.Format("net.tcp://{0}:{1}/P2PService", ep.Address, ep.Port);
                        var binding = new NetTcpBinding();
                        binding.Security.Mode = SecurityMode.None;
                        IP2PService serviceProxy = ChannelFactory<IP2PService>.CreateChannel(
                            binding, new EndpointAddress(endpointUrl));
                        var newPeer = new PeerEntry
                        {
                            PeerName = peer.PeerName,
                            ServiceProxy = serviceProxy,
                            DisplayString = serviceProxy.GetName()
                        };
                        PeerList.Add(newPeer);
                        OnNewPeers(newPeer);
                        logger.Info("New remote peer is successfully registered. Remote peer name: {0}",
                            newPeer.DisplayString);
                    }
                    catch (EndpointNotFoundException enfe)
                    {
                        logger.Error("Can't register remote peer with IP: {0}:{1}. Endpoint is not found.",
                            ep.Address, ep.Port);
                        /*PeerList.Add(
                            new PeerEntry
                            {
                                PeerName = peer.PeerName,
                                DisplayString = "Неизвестный пир",
                                //CanConnect = false
                            });*/
                    }
                }
            }
        }

        public void Send(string data)
        {
            logger.Info("Start sending data to remote hosts. Data: {0}", data);
            foreach (var peerEntry in PeerList)
            {
                // Получение пира и прокси, для отправки сообщения
                if (peerEntry != null && peerEntry.ServiceProxy != null)
                {
                    try
                    {
                        peerEntry.ServiceProxy.SendMessage(data, LocalPeer.PeerName);
                        logger.Info("Data sent to remote host '{0}'", peerEntry.DisplayString);
                    }
                    catch (CommunicationException ce)
                    {
                        logger.Error("This PC can't connect to remote peer '{0}'", peerEntry.DisplayString);
                    }
                }
            }

        }

        #endregion
    }
}