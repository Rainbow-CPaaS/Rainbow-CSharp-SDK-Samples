using Rainbow;
using Rainbow.Console;
using Rainbow.Consts;
using System.Diagnostics;
using Terminal.Gui;

public class LoginView: View
{
    readonly Rainbow.Application rbApplication;
    readonly Rainbow.AutoReconnection rbAutoReconnection;

    readonly TextField loginText;
    readonly TextField passwordText;
    readonly Button btnLogin;
    readonly LoggerView? loggerView;

    public LoginView(UserConfig rbAccount, Rainbow.Application rbApplication, Boolean withDetails = false)
    {
        this.rbApplication = rbApplication;
        rbAutoReconnection = rbApplication.GetAutoReconnection();

        // We want to receive events from SDK
        rbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed;       // Triggered when the authentication process will fail
        rbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;   // Triggered when the Connection State will change

        rbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;                       // Triggered when AutoReonnection service is cancelled
        rbAutoReconnection.TokenExpired += RbAutoReconnection_TokenExpired;                 // Triggered when the Security Token is expired

        Title = $"Login - {rbAccount.Prefix}";
        CanFocus = true;

        // Create Login and Password Label
        var loginLabel = new Label 
        { 
            Text = "Login:",
            TextAlignment = Alignment.End,
            Width = 9
        };
        var passwordLabel = new Label
        {
            Text = "Password:",
            TextAlignment = Alignment.End,
            X = Pos.Left(loginLabel),
            Y = Pos.Bottom(loginLabel) + 1,
            Width = 9
        };

        // Create Login and Password TextField
        loginText = new TextField
        {
            // Position text field adjacent to the label
            X = Pos.Right(passwordLabel) + 1,

            // Fill remaining horizontal space
            Width = Dim.Fill(1),

            Text = rbAccount.Login,
            TextAlignment = Alignment.End
        };
        passwordText = new TextField
        {
            Secret = true,

            // align with the text box above
            X = Pos.Left(loginText),
            Y = Pos.Top(passwordLabel),
            Width = Dim.Fill(1),

            Text = rbAccount.Password
        };

        // Create login button
        btnLogin = new Button
        {
            Text = "Login",
            Y = Pos.Bottom(passwordLabel) + 1,

            // center the login button horizontally
            X = Pos.Center(),
            IsDefault = true,
            Enabled = ButtonLoginEnabled(),
            ShadowStyle = ShadowStyle.None,
            ColorScheme = Tools.ColorSchemeBlackOnWhite
        };

        if (withDetails)
        {
            loggerView = new()
            {
                X = Pos.Left(loginLabel),
                Y = Pos.Bottom(btnLogin) + 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            loggerView.AddColorScheme("green", Tools.ColorSchemeLoggerGreen, LoggerView.DEBUG);
            loggerView.AddColorScheme("blue", Tools.ColorSchemeLoggerBlue, LoggerView.INFO);
            loggerView.AddColorScheme("red", Tools.ColorSchemeLoggerRed, LoggerView.WARN);
        }

        // When text is changed, enable/disable Login Button
        loginText.TextChanged += TextField_TextChanged;
        passwordText.TextChanged += TextField_TextChanged;

        // When login button is clicked - start login process
        btnLogin.MouseClick += BtnLogin_MouseClick_Login;

        // Add elements to this View
        Add(loginLabel, loginText, passwordLabel, passwordText, btnLogin);
        
        if (loggerView != null)
            Add(loggerView);

        if(rbAccount.AutoLogin)
            Initialized += LoginView_Initialized;
    }

    private Boolean ButtonLoginEnabled()
    {
        return !(String.IsNullOrEmpty(loginText.Text) || String.IsNullOrEmpty(passwordText.Text));
    }

    private void LogDebug(String txt) => Log(LoggerView.DEBUG, txt);
    private void LogInfo(String txt) => Log(LoggerView.INFO, txt);
    private void LogWarn(String txt) => Log(LoggerView.WARN, txt);

    private void Log(String level, String txt)
    {
        if (loggerView != null)
            Terminal.Gui.Application.Invoke(() => loggerView.AddText($"{level} - {txt}"));
    }

#region Events received from Terminal.Gui

    private void LoginView_Initialized(object? sender, EventArgs e)
    {
        if (ButtonLoginEnabled())
            BtnLogin_MouseClick_Login(null, null);
    }

    private void BtnLogin_MouseClick_Login(object? sender, MouseEventEventArgs e)
    {
        if(e != null)
            e.MouseEvent.Handled = true;

        // Start login - don't need to manage result here. We handle events to update UI
        var _ = rbApplication.LoginAsync(loginText.Text, passwordText.Text);
    }


    private void BtnLogin_MouseClick_Logout(object? sender, MouseEventEventArgs e)
    {
        if (e != null)
            e.MouseEvent.Handled = true;

        // Start logout - don't need to manage result here. We handle events to update UI
        var _ = rbApplication.LogoutAsync();
    }

    private void TextField_TextChanged(object? sender, EventArgs e)
    {
        btnLogin.Enabled = ButtonLoginEnabled();
    }

#endregion Events received from Terminal.Gui

#region Events received from Rainbow SDK

    void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
    {
        Terminal.Gui.Application.Invoke(() =>
        {
            switch (connectionState.Status)
            {
                case ConnectionStatus.Connected:
                    btnLogin.Text = "Logout";
                    btnLogin.Enabled = true;
                    btnLogin.MouseClick -= BtnLogin_MouseClick_Login;
                    btnLogin.MouseClick += BtnLogin_MouseClick_Logout;

                    LogInfo($"Connected: Reset NbAttempts: [{ rbAutoReconnection.CurrentNbAttempts}]");
                    break;

                case ConnectionStatus.Connecting:
                    loginText.Enabled = false;
                    passwordText.Enabled = false;
                    btnLogin.Enabled = false;

                    LogDebug($"Connecting: Current NbAttempts:[{rbAutoReconnection.CurrentNbAttempts}] - MaxNbAttempts[{rbAutoReconnection.MaxNbAttempts}]");
                    break;

                case ConnectionStatus.Disconnected:
                    btnLogin.Text = "Login";
                    btnLogin.Enabled = true;

                    loginText.Enabled = true;
                    passwordText.Enabled = true;

                    btnLogin.MouseClick -= BtnLogin_MouseClick_Logout;
                    btnLogin.MouseClick += BtnLogin_MouseClick_Login;
                    break;
            }

        });
    }

    void RbApplication_AuthenticationFailed(SdkError sdkError)
    {
        // TODO
    }

    void RbAutoReconnection_TokenExpired()
    {
        // TODO
    }

    void RbAutoReconnection_Cancelled(SdkError sdkError)
    {
        if(sdkError.Type == Rainbow.Enums.SdkErrorType.NoError)
            LogInfo($"AutoReconnection: cancelled because it has been asked (logout for example)");
        else
            LogWarn($"AutoReconnection: cancelled abnormally:{sdkError}");

    }
#endregion Events received from Rainbow SDK
}
