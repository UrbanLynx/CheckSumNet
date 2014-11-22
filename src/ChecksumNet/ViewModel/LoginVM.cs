using System;
using System.Windows;
using System.Windows.Input;
using NLog;

namespace ChecksumNet.ViewModel
{
    public class LoginVM : ViewModelBase
    {
        #region Members

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private string _password;
        private string _username;

        #endregion

        #region Constructors   

        #endregion

        #region Methods

        public bool CheckLogin()
        {
            bool isLogin = MainViewModel.Instance.Manager.TryLogin(Username, Password);
            if (!isLogin)
            {
                MessageBox.Show("Неправильное имя пользователя или пароль.");
                logger.Info("ОШИБКА. КТО: пользователь {0}. ЧТО: попытка авторизации. РЕЗУЛЬТАТ: неудача", Username);
                return false;
            }

            MainViewModel.Instance.IsLogedIn = true;
            CloseWindow();
            return true;
        }

        private void CloseWindow()
        {
            OnRequestClose(this, new EventArgs());
        }

        #endregion

        #region Properties

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("Username");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

        public event EventHandler OnRequestClose;

        #endregion

        #region Commands

        #region Login

        public ICommand Authenticate
        {
            get { return new RelayCommand(param => AuthenticateExecute(), param => CanAuthenticateExecute()); }
        }

        private void AuthenticateExecute()
        {
            CheckLogin();
        }

        private bool CanAuthenticateExecute()
        {
            return true;
        }

        #endregion

        #endregion
    }
}