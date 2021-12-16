using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

using Rainbow;

using Microsoft.Extensions.Logging;

namespace SDK.WpfApp.Model
{
    public class LoginInfoModel: ObservableObject
    {
        string m_login;
        string m_password;
        string m_buttonLoginOrLogoutText;
        string m_connectionState;

        Boolean m_initialisationCompleted;

        Boolean m_canUseLoginAndPwdTextBox;
        Boolean m_canUseLoginOrLogoutButton;

        ICommand m_buttonLoginOrLogoutCommand;

        /// <summary>
        /// Login
        /// </summary>
        public String Login
        {
            get { return m_login; }
            set
            {
                SetProperty(ref m_login, value);
                CheckCanUseLoginOrLogoutButton();
            }
        }

        /// <summary>
        /// Password
        /// </summary>
        public String Password
        {
            get { return m_password; }
            set
            {
                SetProperty(ref m_password, value);
                CheckCanUseLoginOrLogoutButton();
            }
        }

        /// <summary>
        /// Password
        /// </summary>
        public String ButtonLoginOrLogoutText
        {
            get { return m_buttonLoginOrLogoutText; }
            private set { SetProperty(ref m_buttonLoginOrLogoutText, value); }
        }

        /// <summary>
        /// Connection State
        /// </summary>
        public String ConnectionState
        {
            get { return m_connectionState; }
            set
            {
                SetProperty(ref m_connectionState, value);
                UpdateButtonLoginOrLogoutText();
                CheckCanUseLoginAndPwdTextBlock();
                CheckCanUseLoginOrLogoutButton();
            }
        }

        /// <summary>
        /// StringInitialisation Completed
        /// </summary>
        public Boolean InitialisationCompleted
        {
            get { return m_initialisationCompleted; }
            set
            {
                SetProperty(ref m_initialisationCompleted, value);
                CheckCanUseLoginAndPwdTextBlock();
                CheckCanUseLoginOrLogoutButton();
            }
        }
        

        /// <summary>
        /// To know if the login/logout button can be used
        /// </summary>
        public Boolean CanUseLoginLogoutButton
        {
            get { return m_canUseLoginOrLogoutButton; }
            private set { SetProperty(ref m_canUseLoginOrLogoutButton, value); }
        }


        /// <summary>
        /// To know if the login/password textbox can be used
        /// </summary>
        public Boolean CanUseLoginAndPwdTextBox
        {
            get { return m_canUseLoginAndPwdTextBox; }
            private set { SetProperty(ref m_canUseLoginAndPwdTextBox, value); }
        }

        /// <summary>
        /// To manage action on the Confirm button
        /// </summary>
        public ICommand ButtonLoginOrLogoutCommand
        {
            get { return m_buttonLoginOrLogoutCommand; }
            set { m_buttonLoginOrLogoutCommand = value; }
        }

        /// <summary>
        /// Default consructor
        /// </summary>
        public LoginInfoModel()
        {
            Login = "";
            Password = "";
            ConnectionState = Rainbow.Model.ConnectionState.Disconnected;
        }

        private void CheckCanUseLoginOrLogoutButton()
        {
            if((!String.IsNullOrEmpty(Login)) && (!String.IsNullOrEmpty(Password)))
            {
                CanUseLoginLogoutButton = (ConnectionState == Rainbow.Model.ConnectionState.Disconnected)
                                        || ((ConnectionState == Rainbow.Model.ConnectionState.Connected)
                                                && InitialisationCompleted);
            }
            else
                CanUseLoginLogoutButton = false;

            //log.LogDebug("[CheckCanUseLoginOrLogoutButton] CanUseLoginLogoutButton:[{0}]", CanUseLoginLogoutButton.ToString());
        }

        private void CheckCanUseLoginAndPwdTextBlock()
        {
            CanUseLoginAndPwdTextBox = (ConnectionState == Rainbow.Model.ConnectionState.Disconnected);

            //log.LogDebug("[CheckCanUseLoginAndPwdTextBlock] CanUseLoginAndPwdTextBox:[{0}]", CanUseLoginAndPwdTextBox.ToString());
        }

        private void UpdateButtonLoginOrLogoutText()
        {
            if (String.IsNullOrEmpty(ConnectionState)
                || (ConnectionState == Rainbow.Model.ConnectionState.Disconnected))
            {
                ButtonLoginOrLogoutText = "Connect";
                InitialisationCompleted = false;
            }
            else if (ConnectionState == Rainbow.Model.ConnectionState.Connecting)
                ButtonLoginOrLogoutText = "Connecting";
            else
                ButtonLoginOrLogoutText = "Disconnect";

            //log.LogDebug("[UpdateButtonLoginOrLogoutText] ButtonLoginOrLogoutText:[{0}] - InitialisationCompleted:[{1}]", ButtonLoginOrLogoutText, InitialisationCompleted);
        }
    }
}
