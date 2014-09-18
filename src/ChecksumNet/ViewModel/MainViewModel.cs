using System;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ChecksumNet.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Members
        
        private static volatile MainViewModel _instance;
        private static object _syncRoot = new Object();

        

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

        public string LabelText
        {
            get { return ""; }
        }
        #endregion

        #region Commands

        #region OpenCom

        void OpenComExecute()
        {
            /*var dlg = new Microsoft.Win32.OpenFileDialog();
            var result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                _textSettings = _fileManager.LoadFrom(filename);

            }*/
        }

        bool CanOpenComExecute()
        {
            return true;
        }
        public ICommand OpenCom
        {
            get { return new RelayCommand(param => this.OpenComExecute(), param => this.CanOpenComExecute()); }
        }

        

        #endregion
        
        #endregion
    }
}
