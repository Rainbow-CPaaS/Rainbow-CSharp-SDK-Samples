using Rainbow;
using Rainbow.Consts;
using Rainbow.Delegates;
using Rainbow.Enums;
using Rainbow.Model;


internal class RainbowBot
{
    public RainbowAccount RainbowAccount { get; private set; }

    private Rainbow.Application _rbApplication;
    private AutoReconnection _rbAutoReconnection;
    private Contacts _rbContacts;

    private Contact _currentContact;
    private String _currentResource;

    private List<String> _resourcesUsed;
    private Boolean _resourceUsedByAnotherProcess;

    internal event BooleanDelegate AccountUsedOnAnotherDevice;
    internal event ConnectionStateDelegate ConnectionStateChanged;
    internal event SdkErrorDelegate ConnectionFailed;

    internal RainbowBot(RainbowAccount rainbowAccount)
    {
        RainbowAccount = rainbowAccount;

        _rbApplication = new Rainbow.Application(
                iniFolderFullPathName: Configuration.Instance.LogsFolderPath,
                iniFileName: rainbowAccount.Prefix + "file.ini",
                loggerPrefix: rainbowAccount.Prefix);

        // Set restrictions
        _rbApplication.Restrictions.LogRestRequest = true;
        _rbApplication.Restrictions.LogRestRequestOnError = true;
        _rbApplication.Restrictions.LogEvent = true;
        _rbApplication.Restrictions.LogEventRaised = true;

        // Get services
        _rbAutoReconnection = _rbApplication.GetAutoReconnection();
        _rbContacts = _rbApplication.GetContacts();

        // Configure AutoReconnection service
        _rbAutoReconnection.UsePreviousLoginPwd = true; // we want to auto reconnect using previous login/pwd even if security token has expired

        // Configure Application service
        _rbApplication.SetApplicationInfo(Configuration.Instance.AppId, Configuration.Instance.AppSecret);
        _rbApplication.SetHostInfo(Configuration.Instance.HostName);

        // We want to be inform of some events
        _rbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed; ;
        _rbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
        _rbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;
        _rbContacts.ContactPresenceUpdated += RbContacts_ContactPresenceUpdated;

        _resourcesUsed = new();
        _resourceUsedByAnotherProcess = false;

        if (rainbowAccount.AutoLogin)
            Login();
    }

    public void Login()
    {
        if (!_rbApplication.IsConnected())
        {
            _rbApplication.LoginAsync(RainbowAccount.Login, RainbowAccount.Password).ContinueWith((obj) =>
            {
                var sdkResult = obj.Result;
                if (!sdkResult.Success)
                    Util.RaiseEvent(() => ConnectionFailed, _rbApplication, sdkResult.Result);
            });
        }
    }

    private void RbApplication_AuthenticationFailed(SdkError sdkError)
    {
        Util.RaiseEvent(() => ConnectionFailed, _rbApplication, sdkError);
    }

    private void RbContacts_ContactPresenceUpdated(Rainbow.Model.Presence presence)
    {
        // We want to manage only presence of current contact
        if(presence.Contact?.Peer.Jid == _currentContact.Peer.Jid)
        {
            if( (presence.Resource != _currentResource) && (!Contacts.SPECIFIC_PRESENCE_RESOURCES.Contains(presence.Resource) ))
            {
                lock (_resourcesUsed)
                {
                    if (presence.PresenceLevel == PresenceLevel.Unavailable)
                    {
                        // The user is no more using this resource
                        _resourcesUsed.Remove(presence.Resource);
                    }
                    else
                    {
                        // The user is using this resource
                        _resourcesUsed.Add(presence.Resource);
                    }

                    // Trigger event AccountUsedOnAnotherDevice is necessary
                    if (  (_resourceUsedByAnotherProcess && (_resourcesUsed.Count == 0))
                        || ( (!_resourceUsedByAnotherProcess) && (_resourcesUsed.Count > 0)) ) 
                    {
                        _resourceUsedByAnotherProcess = !_resourceUsedByAnotherProcess;
                        Util.RaiseEvent(() => AccountUsedOnAnotherDevice, _rbApplication, _resourceUsedByAnotherProcess);
                    }
                }
            }
        }
        
    }

    private void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
    {
        switch(connectionState.Status) 
        {
            case ConnectionStatus.Connected:
                _currentContact = _rbContacts.GetCurrentContact();
                _currentResource = _rbApplication.GetResource();
                break;

            case ConnectionStatus.Disconnected:
                _resourcesUsed.Clear();
                _resourceUsedByAnotherProcess = false;
                break;
        }

        Util.RaiseEvent(() => ConnectionStateChanged, _rbApplication, connectionState);
    }

    private void RbAutoReconnection_Cancelled(Rainbow.SdkError sdkError)
    {
        if(sdkError?.IncorrectUseError.ErrorDetailsCode == (int)SdkInternalErrorEnum.LOGIN_PROCESS_MAX_ATTEMPTS_REACHED)
        {
            // The auto reconnection service try to connect on server but failed after a lot of retry ....
            // We do it again and again ... 
            Login();
        }
        else
            Util.RaiseEvent(() => ConnectionFailed, _rbApplication, sdkError);
    }
}

