using System;
using System.Windows;
using System.Windows.Input;
using ChecksumNet.Model;

namespace ChecksumNet.ViewModel
{
    public class LoginVM : ViewModelBase
    {
        #region Members

        private string _username;
        private string _password;
        #endregion

        #region Constructors   

        public LoginVM() { }

        #endregion 

        #region Methods

        public bool CheckLogin() 
        {
            var isLogin = MainViewModel.Instance.Manager.TryLogin(Username, Password);
            if (!isLogin)
            {
                MessageBox.Show("Неправильное имя пользователя или пароль.");
                return false;
            }
            
            MainViewModel.Instance.IsLogedIn = true;
            //MessageBox.Show("Здравствуйте, " + Username + ", вы успешно вошли.");
            CloseWindow(); 
            return true;
        }

        private void CloseWindow()
        {
            OnRequestClose(this, new EventArgs());
        }
        #endregion

        #region Properties

        public event EventHandler OnRequestClose;

        public string Username
        {
            get { return _username; }
            set { _username = value; OnPropertyChanged("Username"); }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged("Password"); }           
        }
                 
        #endregion

        #region Commands

        #region Login

        void AuthenticateExecute()
        {
            CheckLogin();
        }

        bool CanAuthenticateExecute()
        {
            return true;
        }
        public ICommand Authenticate
        {
            get { return new RelayCommand(param => this.AuthenticateExecute(), param => this.CanAuthenticateExecute()); }
        }


        #endregion

        #endregion


    }
}
