using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<PeerVM> peerList;
        private PeerVM localPeer;

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
            Manager.OnNewPeer += AddPeerToList;
            SetLocalPeer();
        }

        private void OnDataChanged()
        {
            OnPropertyChanged("Filename");
            OnPropertyChanged("LocalPeer");
            OnPropertyChanged("PeerList");
        }

        void SetLocalPeer()
        {
            LocalPeer = new PeerVM(Manager.GetLocalPeer());
            Manager.OnDataUpdate += localPeer.DataUpdate;
        }

        void AddPeerToList(PeerEntry peerEntry)
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
            set { peerList = value; OnPropertyChanged("PeerList"); }
        }

        public PeerVM LocalPeer
        {
            get { return localPeer; }
            set { localPeer = value; OnPropertyChanged("LocalPeer"); }
        }

        public string Filename
        {
            get { return filename; }
            set { filename = value; OnPropertyChanged("Filename"); }
        }
        
        public bool IsLogedIn
        {
            get { return isLogedIn; }
            set { isLogedIn = value; OnPropertyChanged("IsLogedIn"); }
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

        #region Refresh

        void RefreshExecute()
        {
            PeerList = new ObservableCollection<PeerVM>();
            Manager.RefreshHosts();
            
            //Manager.StartListening();
        }

        bool CanRefreshExecute()
        {
            //return true;
            return IsLogedIn;
        }
        public ICommand RefreshCommand
        {
            get { return new RelayCommand(param => this.RefreshExecute(), param => this.CanRefreshExecute()); }
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
            //return true;
            return IsLogedIn;
        }
        public ICommand BrowseCommand
        {
            get { return new RelayCommand(param => this.BrowseExecute(), param => this.CanBrowseExecute()); }
        }

        

        #endregion

        #region About

        void AboutExecute()
        {
            MessageBox.Show("Программа написана коллективом Мушиц С., Переверзев В., ИУ7-71, 2014г.");
        }

        bool CanAboutExecute()
        {
            return true;
        }
        public ICommand AboutCommand
        {
            get { return new RelayCommand(param => this.AboutExecute(), param => this.CanAboutExecute()); }
        }

        #endregion

        #endregion
    }
}
