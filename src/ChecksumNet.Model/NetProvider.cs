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

                logger.Info(
                    "КТО: пользователь {0}. ЧТО: запуск регистрации хоста на порте {1}. РЕЗУЛЬТАТ: успешно. Имя хоста - {2}",
                    username, port, serviceUrl);
            }
            catch (AddressAlreadyInUseException)
            {
                logger.Error(
                    "ОШИБКА. КТО: пользователь{0}. ЧТО: запуск регистрации хоста на порте {1}. РЕЗУЛЬТАТ: неудача. Ошибка WCF: невозможно начать прослушивание на порте {1}, он используется другой программой",
                    username, port);
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

                logger.Info("КТО: пользователь {0}. ЧТО: получение данных от пользователя. РЕЗУЛЬТАТ: данные: {1}",
                    fromPeer.DisplayString, fromPeer.Checksum);
                OnDataReceived();
            }
            else
            {
                logger.Error("ОШИБКА. КТО: неизвестный пользователь. ЧТО: получение данных от пользователя. РЕЗУЛЬТАТ: пользователь неопознан. Данные: {0}",
                    receivedDataEventArgs.Data);
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
            logger.Info("КТО: пользователь {0}. ЧТО: попытка обновления хостов. РЕЗУЛЬТАТ: в процессе",
                    LocalPeer.DisplayString);
            // Создание распознавателя и добавление обработчиков событий
            var resolver = new PeerNameResolver();
            resolver.ResolveProgressChanged += resolver_ResolveProgressChanged;
            resolver.ResolveCompleted += resolver_ResolveCompleted;

            // Подготовка к добавлению новых пиров
            PeerList.Clear();

            // Преобразование незащищенных имен пиров асинхронным образом
            resolver.ResolveAsync(new PeerName("0.P2P Sample"), 1);
        }

        private void resolver_ResolveCompleted(object sender, ResolveCompletedEventArgs e)
        {
            logger.Info("КТО: пользователь {0}. ЧТО: завершение обновления хостов. РЕЗУЛЬТАТ: успешно",
                    LocalPeer.DisplayString);
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
                        logger.Info("КТО: пользователь {0}. ЧТО: регистрация удаленного хоста {1}. РЕЗУЛЬТАТ: успешно",
                            LocalPeer.DisplayString, newPeer.DisplayString);
                    }
                    catch (EndpointNotFoundException enfe)
                    {
                        logger.Error(
                            "ОШИБКА. КТО: пользователь {0}. ЧТО: регистрация удаленного хоста c IP: {1}:{2}. РЕЗУЛЬТАТ: неудача",
                            LocalPeer.DisplayString, ep.Address, ep.Port);
                    }
                }
            }
        }

        public void Send(string data)
        {
            logger.Info("КТО: пользователь {0}. ЧТО: попытка отправления данных {1} удаленным хостам. РЕЗУЛЬТАТ: в процессе",
                            LocalPeer.DisplayString, data);
            foreach (var peerEntry in PeerList)
            {
                // Получение пира и прокси, для отправки сообщения
                if (peerEntry != null && peerEntry.ServiceProxy != null)
                {
                    try
                    {
                        peerEntry.ServiceProxy.SendMessage(data, LocalPeer.PeerName);
                        logger.Info("КТО: пользователь {0}. ЧТО: отправление данных удаленному хосту {1}. РЕЗУЛЬТАТ: успешно.",
                            LocalPeer.DisplayString, peerEntry.DisplayString);
                    }
                    catch (CommunicationException ce)
                    {
                        logger.Error("ОШИБКА. КТО: пользователь {0}. ЧТО: отправление данных удаленному хосту {1}. РЕЗУЛЬТАТ: неудача. Невозможно соединиться с удаленным хостом.",
                            LocalPeer.DisplayString, peerEntry.DisplayString);
                    }
                }
            }

        }

        #endregion
    }
}