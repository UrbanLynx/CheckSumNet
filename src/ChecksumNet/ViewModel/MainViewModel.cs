using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ChecksumNet.Model;
using ChecksumNet.View;
using Microsoft.Win32;

namespace ChecksumNet.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Members

        private static volatile MainViewModel _instance;
        private static readonly object _syncRoot = new Object();

        private string filename;
        private bool isLogedIn;
        private PeerVM localPeer;
        private ObservableCollection<PeerVM> peerList;

        #endregion

        #region Constructors

        private MainViewModel()
        {
        }

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
                    Instance.Creator();
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
            Manager.OnNewPeer += AddPeerToList;
            SetLocalPeer();
        }

        private void OnDataChanged()
        {
            OnPropertyChanged("Filename");
            OnPropertyChanged("LocalPeer");
            OnPropertyChanged("PeerList");
        }

        private void SetLocalPeer()
        {
            LocalPeer = new PeerVM(Manager.GetLocalPeer());
            Manager.OnDataUpdate += localPeer.DataUpdate;
        }

        private void AddPeerToList(PeerEntry peerEntry)
        {
            var newPeer = new PeerVM(peerEntry);
            Manager.OnDataUpdate += newPeer.DataUpdate;
            PeerList.Add(newPeer);
        }

        #endregion

        #region Properties

        public ModelManager Manager { get; set; }

        public ObservableCollection<PeerVM> PeerList
        {
            get { return peerList; }
            set
            {
                peerList = value;
                OnPropertyChanged("PeerList");
            }
        }

        public PeerVM LocalPeer
        {
            get { return localPeer; }
            set
            {
                localPeer = value;
                OnPropertyChanged("LocalPeer");
            }
        }

        public string Filename
        {
            get { return filename; }
            set
            {
                filename = value;
                OnPropertyChanged("Filename");
            }
        }

        public bool IsLogedIn
        {
            get { return isLogedIn; }
            set
            {
                isLogedIn = value;
                OnPropertyChanged("IsLogedIn");
            }
        }

        #endregion

        #region Commands

        #region Login

        public ICommand LoginCommand
        {
            get { return new RelayCommand(param => LoginExecute(), param => CanLoginExecute()); }
        }

        private void LoginExecute()
        {
            var vm = new LoginVM();
            var loginWindow = new LoginView
            {
                DataContext = vm
            };
            vm.OnRequestClose += (s, e) => loginWindow.Close();

            loginWindow.ShowDialog();
        }

        private bool CanLoginExecute()
        {
            return !IsLogedIn;
        }

        #endregion

        #region Refresh

        public ICommand RefreshCommand
        {
            get { return new RelayCommand(param => RefreshExecute(), param => CanRefreshExecute()); }
        }

        private void RefreshExecute()
        {
            PeerList = new ObservableCollection<PeerVM>();
            Manager.RefreshHosts();
        }

        private bool CanRefreshExecute()
        {
            return IsLogedIn;
        }

        #endregion

        #region Browse

        public ICommand BrowseCommand
        {
            get { return new RelayCommand(param => BrowseExecute(), param => CanBrowseExecute()); }
        }

        private void BrowseExecute()
        {
            var dlg = new OpenFileDialog();
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                Filename = dlg.FileName;
                Manager.NewChecksum(Filename);
            }
        }

        private bool CanBrowseExecute()
        {
            return IsLogedIn;
        }

        #endregion

        #region About

        public ICommand AboutCommand
        {
            get { return new RelayCommand(param => AboutExecute(), param => CanAboutExecute()); }
        }

        private void AboutExecute()
        {
            MessageBox.Show("Программа написана коллективом Мушиц С., Переверзев В., ИУ7-71, 2014г.");
        }

        private bool CanAboutExecute()
        {
            return true;
        }

        #endregion

        #endregion
    }
}