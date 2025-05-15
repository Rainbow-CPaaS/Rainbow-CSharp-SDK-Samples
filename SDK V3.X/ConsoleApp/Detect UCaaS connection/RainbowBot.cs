using Rainbow;
using Rainbow.Consts;
using Rainbow.Delegates;
using Rainbow.Enums;
using Rainbow.Example.Common;
using Rainbow.Model;
using Rainbow.SimpleJSON;


internal class RainbowBot
{
    public UserConfig RainbowAccount { get; private set; }

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

    internal RainbowBot(ServerConfig serverConfig, UserConfig rainbowAccount)
    {
        RainbowAccount = rainbowAccount;

        NLogConfigurator.AddLogger(rainbowAccount.Prefix);

        _rbApplication = new Rainbow.Application(
                iniFolderFullPathName: rainbowAccount.IniFolderPath,
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
        _rbApplication.SetApplicationInfo(serverConfig.AppId, serverConfig.AppSecret);
        _rbApplication.SetHostInfo(serverConfig.HostName);

        // We want to be inform of some events
        _rbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed; ;
        _rbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
        _rbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;
        _rbContacts.ContactPresenceUpdated += RbContacts_ContactPresenceUpdated;

        _resourcesUsed = new();
        _resourceUsedByAnotherProcess = false;

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
                    Rainbow.Util.RaiseEvent(() => ConnectionFailed, _rbApplication, sdkResult.Result);
            });
        }
    }

    public async Task<SdkResult<String>> MakeRestRequestAsync(String requestUri, String requestMethod, String? body = null, Dictionary<string, object>? queryParameters = null, Dictionary<String, String>? specificHeaders = null, String? acceptContentType = null, int acceptHttpStatusCode = 200)
    {
        var httpClient = _rbApplication?.GetSecondaryHttpClient();

        if (httpClient is null)
            return new SdkResult<String>(SdkInternalError.FromInternalError(SdkInternalErrorEnum.OPERATION_FAILED, new[] { "HttpClient is null", "MakeRestRequestAsync" }));

        HttpRequestDescriptor httpRequestDescriptor = HttpRequestDescriptor.JsonRequest(requestUri, requestMethod, body);
        httpRequestDescriptor.AddQueryParameters(queryParameters);
        httpRequestDescriptor.AddAcceptContentType(acceptContentType);
        httpRequestDescriptor.AddAcceptHttpStatusCode(acceptHttpStatusCode);
        if (specificHeaders?.Count > 0)
            httpRequestDescriptor.AddHeaders(specificHeaders);

        return await httpClient.RequestAsStringAsync(httpRequestDescriptor);
    }

    private async void RbApplication_AuthenticationFailed(SdkError sdkError)
    {
        Rainbow.Util.RaiseEvent(() => ConnectionFailed, _rbApplication, sdkError);

        /*
        // EXAMPLE TO POST DATA USING HTTP
        Dictionary<String, object> bodyDico = new()
        {
            { "login", RainbowAccount.Login },
            { "error", sdkError.ToString()}
        };

        var requestUri = "https://www.myserver.com/AuthenticationFailed";
        var requestMethod = "POST";
        var body = JSON.ToJson(bodyDico);

        var sdkResult = await MakeRestRequestAsync(requestUri, requestMethod, body);
        if (!sdkResult.Success)
        {
            // it was not possible to make REST request to my server ...
        }
        */
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
                        Rainbow.Util.RaiseEvent(() => AccountUsedOnAnotherDevice, _rbApplication, _resourceUsedByAnotherProcess);
                    }
                }
            }
        }
        
    }

    private async void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
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

        Rainbow.Util.RaiseEvent(() => ConnectionStateChanged, _rbApplication, connectionState);

        /*
        // EXAMPLE TO POST DATA USING HTTP
        Dictionary<String, object> bodyDico = new()
        {
            { "login", RainbowAccount.Login },
            { "connectionStatus", connectionState.Status}
        };

        var requestUri = "https://www.myserver.com/ConnectionStateChanged";
        var requestMethod = "POST";
        var body = JSON.ToJson(bodyDico);

        var sdkResult = await MakeRestRequestAsync(requestUri, requestMethod, body);
        if (!sdkResult.Success)
        {
            // it was not possible to make REST request to my server ...
        }
        */
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
            Rainbow.Util.RaiseEvent(() => ConnectionFailed, _rbApplication, sdkError);
    }
}

