using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChecksumNet.View;
using ChecksumNet.Model;

namespace ChecksumNet.ViewModel
{
    public class LoginVM : ViewModelBase
    {
        #region Members

        public Authentication authentication = new Authentication();;

        #endregion

        #region Constructors   

        public LoginVM() { }

        #endregion 

        #region Methods

       /* private void Creator()
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
            / *var dlg = new Microsoft.Win32.OpenFileDialog();
            var result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                _textSettings = _fileManager.LoadFrom(filename);
                return _textSettings.Text;
            }* /
            return null;
        }

       */
        #endregion

        #region Properties

        public string Login
        {
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                OnPropertyChanged("Login");
            }
            get { return null; } 

        }

        public string Password
        {
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                OnPropertyChanged("Password");
            }
            get { return null; } 
        }  
           
        
        #endregion

        #region Commands

        #region Login

        void AuthenticationExecute()
        {
            

        }

        bool CanAuthenticationExecute()
        {
            return true;
        }
        public ICommand AuthenticationCommand
        {
            get { return new RelayCommand(param => this.AuthenticationExecute(), param => this.CanAuthenticationExecute()); }
        }


        #endregion

     
        
        #endregion


    }
}
