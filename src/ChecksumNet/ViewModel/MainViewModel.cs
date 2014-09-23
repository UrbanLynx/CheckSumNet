using System;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ChecksumNet.Model;

namespace ChecksumNet.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Members
        
        private static volatile MainViewModel _instance;
        private static object _syncRoot = new Object();

        private Manager manager;
        private string filename;

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
            manager = new Manager();
            manager.DataUpdate += DataChanged;
        }

        private void DataChanged()
        {
            OnPropertyChanged("RemoteIP");
            OnPropertyChanged("Filename");
            OnPropertyChanged("MyHash");
            OnPropertyChanged("RemoteHash");
        }


        public string OpenFile()
        {
            /*var dlg = new Microsoft.Win32.OpenFileDialog();
            var result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                _textSettings = _fileManager.LoadFrom(filename);
                return _textSettings.Text;
            }*/
            return null;
        }

       
        #endregion

        #region Properties

        public string RemoteIP
        {
            set { OnPropertyChanged("RemoteIP"); }
            get
            {
                if (manager.RemoteHost != null) return manager.RemoteHost.IP.ToString();
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
            set { OnPropertyChanged("MyHash"); }
            get
            {
                if (manager.Sender.Checksum != null)
                    return BitConverter.ToString(manager.Sender.Checksum).Replace("-", "").ToLower();
                return "Файл не выбран";
            }
        }

        public string RemoteHash
        {
            set { OnPropertyChanged("RemoteHash"); }
            get
            {
                if (manager.RemoteHost != null && manager.RemoteHost.Checksum != null)
                    return BitConverter.ToString(manager.RemoteHost.Checksum).Replace("-", "").ToLower();
                return "Удаленный ПК не выбрал файл";
            }
        }
        
        #endregion


        #region Commands

        #region Login

        void LoginExecute()
        {

            var logView = new View.LoginView
            {
                DataContext = new LoginVM()
            };
            logView.Show();
        }

        bool CanLoginExecute()
        {
            return true;
        }
        public ICommand LoginCommand
        {
            get { return new RelayCommand(param => this.LoginExecute(), param => this.CanLoginExecute()); }
        }

        

        #endregion

        #region Connect

        void ConnectExecute()
        {
            manager.SetConnection();
            manager.StartListening();
        }

        bool CanConnectExecute()
        {
            return true;
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
                manager.NewChecksum(Filename);
            }
        }

        bool CanBrowseExecute()
        {
            return true;
        }
        public ICommand BrowseCommand
        {
            get { return new RelayCommand(param => this.BrowseExecute(), param => this.CanBrowseExecute()); }
        }

        

        #endregion
        
        #endregion
    }
}
