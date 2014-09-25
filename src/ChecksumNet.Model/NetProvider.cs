using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.PeerToPeer;
using System.Net.Sockets;
using System.ServiceModel;
using NLog;

namespace ChecksumNet.Model
{
    public class NetProvider
    {
        public delegate void ProcessData(byte[] data);

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private List<PeerEntry> PeerList = new List<PeerEntry>();

        private ServiceHost host;
        private P2PService localService;
        private PeerName peerName;
        private PeerNameRegistration peerNameRegistration;
        private string serviceUrl;
        
        public NetProvider()
        {
            RegisterHost(Dns.GetHostName());
        }

        public NetProvider(string username)
        {
            RegisterHost(username);
        }

        public bool IsServerActive { get; set; }
        public EndPoint RemoteEP { get; set; }
        public EndPoint LocalEP { get; set; }

        public event ProcessData onDataReceived;

        public void SetConnection()
        {
            RefreshHosts();
        }

        public void RegisterHost(string username)
        {
            // Получение конфигурационной информации из app.config
            string port = ConfigurationManager.AppSettings["port"];
            //string username = "lalal";
            string machineName = Environment.MachineName;
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
                // TODO: Отображение ошибки и завершение работы приложения
                //MessageBox.Show(this, "Не удается определить адрес конечной точки WCF.", "Networking Error",MessageBoxButton.OK, MessageBoxImage.Stop);
            }

            // Регистрация и запуск службы WCF
            localService = new P2PService(username);
            host = new ServiceHost(localService, new Uri(serviceUrl));
            var binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;
            host.AddServiceEndpoint(typeof (IP2PService), binding, serviceUrl);
            try
            {
                host.Open();
            }
            catch (AddressAlreadyInUseException)
            {
                // TODO: Отображение ошибки и завершение работы приложения
                //MessageBox.Show(this, "Не удается начать прослушивание, порт занят.", "WCF Error", MessageBoxButton.OK, MessageBoxImage.Stop);
            }

            // Создание имени равноправного участника (пира)
            peerName = new PeerName("P2P Sample", PeerNameType.Unsecured);

            // Подготовка процесса регистрации имени равноправного участника в локальном облаке
            peerNameRegistration = new PeerNameRegistration(peerName, int.Parse(port));
            peerNameRegistration.Cloud = Cloud.AllLinkLocal;

            // Запуск процесса регистрации
            peerNameRegistration.Start();
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
            resolver.ResolveAsync(new PeerName("0.P2P Sample"), 1);
        }

        private void resolver_ResolveCompleted(object sender, ResolveCompletedEventArgs e)
        {
            // Сообщение об ошибке, если в облаке не найдены пиры
            if (PeerList.Count == 0)
            {
                PeerList.Add(
                    new PeerEntry
                    {
                        DisplayString = "Пиры не найдены.",
                        CanConnect = false
                    });
            }
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
                        PeerList.Add(
                            new PeerEntry
                            {
                                PeerName = peer.PeerName,
                                ServiceProxy = serviceProxy,
                                DisplayString = serviceProxy.GetName(),
                                CanConnect = true
                            });
                    }
                    catch (EndpointNotFoundException)
                    {
                        PeerList.Add(
                            new PeerEntry
                            {
                                PeerName = peer.PeerName,
                                DisplayString = "Неизвестный пир",
                                CanConnect = false
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
                        peerEntry.ServiceProxy.SendMessage("Привет друг!", ConfigurationManager.AppSettings["username"]);
                    }
                    catch (CommunicationException)
                    {

                    }
                }
            }
            
        }
    }
}