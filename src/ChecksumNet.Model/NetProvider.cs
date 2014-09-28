using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.PeerToPeer;
using System.Net.Sockets;
using System.ServiceModel;
using NLog;

namespace ChecksumNet.Model
{
    public class NetProvider
    {
        public delegate void ProcessData();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public List<PeerEntry> PeerList = new List<PeerEntry>();
        public PeerEntry LocalPeer = new PeerEntry();

        // для управления службой wcf и pnrp
        private ServiceHost host;
        private PeerNameRegistration peerNameRegistration;
        
        public NetProvider()
        {
            //RegisterHost(Environment.MachineName);
            //RegisterHost("qw");
        }

        /*public NetProvider(string username)
        {
            RegisterHost(username);
        }*/

        public event ProcessData OnDataReceived;

        public delegate void NewPeer(PeerEntry peerEntry);
        public event NewPeer OnNewPeers;

        public void RegisterHost(string username)
        {
            // Получение конфигурационной информации из app.config
            string port = ConfigurationManager.AppSettings["port"];
            //username = Environment.MachineName;
            //string machineName = Environment.MachineName;
            string serviceUrl = null;


            //  Получение URL-адреса службы с использованием адресаIPv4 
            //  и порта из конфигурационного файла
            foreach (IPAddress address in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    serviceUrl = string.Format("net.tcp://{0}:{1}/P2PService", address, port);
                    break;
                }
            }

            // Выполнение проверки, не является ли адрес null
            if (serviceUrl == null)
            {
                logger.Info("Не удается определить адрес конечной точки WCF.");
            }

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
            }
            catch (AddressAlreadyInUseException)
            {
                logger.Info("Ошибка WCF. Не удается начать прослушивание, порт занят.");
            }

            // Создание имени равноправного участника (пира)
            LocalPeer.PeerName = new PeerName("P2P Checksums", PeerNameType.Unsecured);

            // Подготовка процесса регистрации имени равноправного участника в локальном облаке
            peerNameRegistration = new PeerNameRegistration(LocalPeer.PeerName, int.Parse(port));
            peerNameRegistration.Cloud = Cloud.AllLinkLocal;

            // Запуск процесса регистрации
            peerNameRegistration.Start();
        }

        private void ServiceProxyOnReceivedData(object sender, ReceivedDataEventArgs receivedDataEventArgs)
        {
            var fromPeer =
                PeerList.FirstOrDefault(
                    peer => peer.PeerName.PeerHostName == receivedDataEventArgs.FromPeer.PeerHostName);
            if (fromPeer != null)
            {
                fromPeer.Checksum = receivedDataEventArgs.Data;
                OnDataReceived();
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
            // Создание распознавателя и добавление обработчиков событий
            var resolver = new PeerNameResolver();
            resolver.ResolveProgressChanged += resolver_ResolveProgressChanged;
            resolver.ResolveCompleted += resolver_ResolveCompleted;

            // Подготовка к добавлению новых пиров
            PeerList.Clear();
            // TODO: RefreshButton.IsEnabled = false;

            // Преобразование незащищенных имен пиров асинхронным образом
            resolver.ResolveAsync(new PeerName("P2P Checksums", PeerNameType.Unsecured), 1);
        }

        private void resolver_ResolveCompleted(object sender, ResolveCompletedEventArgs e)
        {
            
            // TODO: Сообщение об ошибке, если в облаке не найдены пиры
            /*if (PeerList.Count == 0)
            {
                PeerList.Add(
                    new PeerEntry
                    {
                        DisplayString = "Пиры не найдены.",
                        CanConnect = false
                    });
            }*/
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
                        string endpointUrl = string.Format("net.tcp://{0}:{1}/P2PService", ep.Address, ep.Port);
                        var binding = new NetTcpBinding();
                        binding.Security.Mode = SecurityMode.None;
                        IP2PService serviceProxy = ChannelFactory<IP2PService>.CreateChannel(
                            binding, new EndpointAddress(endpointUrl));
                        var newPeer = new PeerEntry
                        {
                            PeerName = peer.PeerName,
                            ServiceProxy = serviceProxy,
                            DisplayString = serviceProxy.GetName(),
                            //CanConnect = true
                        };
                        PeerList.Add(newPeer);
                        OnNewPeers(newPeer);
                    }
                    catch (EndpointNotFoundException)
                    {
                        PeerList.Add(
                            new PeerEntry
                            {
                                PeerName = peer.PeerName,
                                DisplayString = "Неизвестный пир",
                                //CanConnect = false
                            });
                    }
                }
            }
        }

        public void Send(string data)
        {
            foreach (var peerEntry in PeerList)
            {
                // Получение пира и прокси, для отправки сообщения
                if (peerEntry != null && peerEntry.ServiceProxy != null)
                {
                    try
                    {
                        peerEntry.ServiceProxy.SendMessage(data, LocalPeer.PeerName);
                    }
                    catch (CommunicationException)
                    {

                    }
                }
            }
            
        }

        
    }
}