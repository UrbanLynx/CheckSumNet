using System.Windows.Input;
using ChecksumNet.Model;
using FunctionalFun.UI;

namespace ChecksumNet.ViewModel
{
    public class LoginVM : ViewModelBase
    {
        #region Members

        public Manager manager = new Manager();

        private string _login;
        private string _password;
        #endregion

        #region Constructors   

        public LoginVM() { }

        #endregion 

        #region Methods

        /*private void DataChanged()
        {
            OnPropertyChanged("RemoteIP");
            OnPropertyChanged("Filename");
            OnPropertyChanged("MyHash");
            OnPropertyChanged("RemoteHash");
        }
       */
        #endregion

        #region Properties

        public string Login
        {
            get { return _login; }
            set { _login = value; OnPropertyChanged("Login"); }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged("Password"); }           
        }

        public string AuthResult
        {
            set
            { OnPropertyChanged("AuthResult");}
            get
            {
                if (manager.isAuthentication == true) 
                    return "Вход произведен";
                return "Вход не выполнен";
            }

        }
                 
        #endregion

        #region Commands

        #region Login

        void AuthenticationExecute()
        {
            manager.Comare(Login, Password);
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
