using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ChecksumNet.Model;
using ChecksumNet.View;

namespace ChecksumNet.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Members
        
        private static volatile MainViewModel _instance;
        private static object _syncRoot = new Object();

        private string filename;
        private bool isLogedIn = false;

        #endregion

        #region Constructors
        private MainViewModel() { }

        public static MainViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new MainViewModel();
                    }
                    MainViewModel.Instance.Creator();
                }

                return _instance;
            }
        }
        #endregion

        #region Methods

        private void Creator()
        {
            Manager = new ModelManager();
            Manager.OnDataUpdate += OnDataChanged;
            Manager.OnPeerRefresh += RefreshPeerList;
        }

        private void OnDataChanged()
        {
            OnPropertyChanged("LocalIP");
            OnPropertyChanged("RemoteIP");
            OnPropertyChanged("Filename");
            OnPropertyChanged("MyHash");
            OnPropertyChanged("RemoteHash");
            OnPropertyChanged("IsChecksumsEqual");
        }

        void RefreshPeerList()
        {
            PeerList = new List<PeerVM>();
            foreach (var peerEntry in Manager.GetPeers())
            {
                PeerList.Add(new PeerVM(peerEntry));
            }
        }

        
        #endregion

        #region Properties

        public ModelManager Manager { get; set; }

        public List<PeerVM> PeerList { get; set; }

        public string LocalIP
        {
            set { OnPropertyChanged("LocalIP"); }
            get
            {
                //if (Manager.LocalHost != null && Manager.LocalHost.IP != null) return Manager.LocalHost.IP.ToString();
                return "";
            }
        }

        public string RemoteIP
        {
            set { OnPropertyChanged("RemoteIP"); }
            get
            {
                //if (Manager.RemoteHost != null && Manager.RemoteHost.IP != null) return Manager.RemoteHost.IP.ToString();
                return "Нет соединения";
            }
        }
        public string Filename
        {
            get { return filename; }
            set { filename = value; OnPropertyChanged("Filename"); }
        }

        public string MyHash
        {
            get
            {
                /*var peer = Manager.GetLocalPeer();
                if (peer.Checksum != null)
                    return peer.Checksum;*/
                return "Файл не выбран";
            }
        }

        public string RemoteHash
        {
            get
            {
                /*var peer = Manager.GetPeers().FirstOrDefault();
                if (peer != null && peer.Checksum != null)
                    return peer.Checksum;*/
                return "Удаленный ПК не выбрал файл";
            }
            set { OnPropertyChanged("RemoteHash"); }
        }

        public bool IsLogedIn
        {
            get { return isLogedIn; }
            set { isLogedIn = value; OnPropertyChanged("RemoteHash"); }
        }

        public bool IsChecksumsEqual
        {
            get { return Manager.IsChecksumsEqual; }
            set { OnPropertyChanged("IsChecksumsEqual"); }
        }
        #endregion


        #region Commands

        #region Login

        void LoginExecute()
        {

            var vm = new LoginVM();
            var loginWindow = new LoginView
            {
                DataContext = vm
            };
            vm.OnRequestClose += (s, e) => loginWindow.Close();

            loginWindow.ShowDialog(); 
        }

        bool CanLoginExecute()
        {
            return !IsLogedIn;
        }
        public ICommand LoginCommand
        {
            get { return new RelayCommand(param => this.LoginExecute(), param => this.CanLoginExecute()); }
        }

        #endregion

        #region Connect

        void ConnectExecute()
        {
            Manager.SetConnection();
            
            //Manager.StartListening();
        }

        bool CanConnectExecute()
        {
            return true;
            //return IsLogedIn;
        }
        public ICommand ConnectCommand
        {
            get { return new RelayCommand(param => this.ConnectExecute(), param => this.CanConnectExecute()); }
        }



        #endregion

        #region Browse

        void BrowseExecute()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            var result = dlg.ShowDialog();

            if (result == true)
            {
                Filename = dlg.FileName;
                Manager.NewChecksum(Filename);
            }
        }

        bool CanBrowseExecute()
        {
            return true;
            //return IsLogedIn && Manager.IsConnected;
        }
        public ICommand BrowseCommand
        {
            get { return new RelayCommand(param => this.BrowseExecute(), param => this.CanBrowseExecute()); }
        }

        

        #endregion
        
        #endregion
    }
}
