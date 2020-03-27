using System;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Rainbow;
using Rainbow.Model;

using InstantMessaging.Helpers;
using InstantMessaging.Model;

using log4net;

namespace InstantMessaging.ViewModel
{
    public class LoginViewModel : ObservableObject
    {
        private static readonly ILog log = LogConfigurator.GetLogger(typeof(LoginViewModel));
        LoginModel model;

    #region PROPERTIES

        BitmapImage m_logo;
        public BitmapImage Logo
        {
            get { return m_logo; }
            set { SetProperty(ref m_logo, value); }
        }

        string m_login;
        public String Login
        {
            get { return m_login; }
            set { SetProperty(ref m_login, value); }
        }

        string m_password;
        public string Password
        {
            get { return m_password; }
            set { SetProperty(ref m_password, value); }
        }

        string m_connect;
        public String Connect
        {
            get { return m_connect; }
            set { SetProperty(ref m_connect, value); }
        }

        Boolean m_isNotBusy;
        public Boolean IsNotBusy
        {
            get { return m_isNotBusy; }
            set
            {
                SetProperty(ref m_isNotBusy, value);
                if (IsBusy == m_isNotBusy)
                    IsBusy = !m_isNotBusy;
            }
        }

        Boolean m_isBusy;
        public Boolean IsBusy
        {
            get { return m_isBusy; }
            set
            {
                SetProperty(ref m_isBusy, value);
                if (IsNotBusy == m_isBusy)
                    IsNotBusy = !m_isBusy;
            }
        }

        ICommand m_ButtonLoginCommand;
        public ICommand ButtonLoginCommand
        {
            get { return m_ButtonLoginCommand; }
            set { m_ButtonLoginCommand = value; }
        }

    #endregion PROPERTIES

    #region CONSTRUCTOR
        public LoginViewModel()
        {
            // Init / Define properties not managed by the Model Object
            Logo = Helper.GetBitmapImageFromResource("splash_logo.png");
            m_ButtonLoginCommand = new RelayCommand<object>(new Action<object>(LoginCommand));

            // Create Model object associated to the current ViewModel object
            model = new LoginModel();
            
            // Define event(s) we want to manage from model
            model.PropertyChanged += Model_PropertyChanged; // Use reflection to set ViewModel's properties from Model's properties

            // Init the Model - To set default values to ppropeties
            model.Init();
        }

    #endregion CONSTRUCTOR

    #region COMMANDS RAISED BY THE VIEW

        public void LoginCommand(object obj)
        {
            if (model != null)
                model.RBLogin(m_login, m_password);
            else
                log.ErrorFormat("[LoginCommand] Model object is null !");
        }

    #endregion COMMANDS RAISED BY THE VIEW

    #region EVENTS FIRED FROM MODEL
        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Use Reflection to get value from sender objec t(i.e. the Model) and set it to a property with the same name in the View Model
            PropertyInfo propertyInfoLocal;
            PropertyInfo propertyInfoSender;

            // Get property info from the current object
            propertyInfoLocal = this.GetType().GetProperty(e.PropertyName);
            if(propertyInfoLocal != null)
            {
                // Get property info from Model / Sender
                propertyInfoSender = model.GetType().GetProperty(e.PropertyName);
                if (propertyInfoSender != null)
                {
                    // Get value from the sender object to the local object
                    Object objectValue = propertyInfoSender.GetValue(model);
                    try
                    {
                        // Set value from the sender object to the local object
                        propertyInfoLocal.SetValue(this, Convert.ChangeType(objectValue, propertyInfoLocal.PropertyType), null);
                    }
                    catch (Exception exc)
                    {
                        log.WarnFormat("[Model_PropertyChanged] EXCEPTION:[{0}]", Util.SerializeException(exc));
                    }
                }
                else
                    log.WarnFormat("[Model_PropertyChanged] Cannot get property info from SENDER OBJECT - PropertyName:[{0}]", e.PropertyName);
            }
            else
                log.WarnFormat("[Model_PropertyChanged] Cannot get property info from LOCAL OBJECT - PropertyName:[{0}]", e.PropertyName);
        }
    #endregion

    }
}
