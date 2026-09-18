using Microsoft.Extensions.Logging;
using Rainbow;
using Rainbow.Attributes;
using Rainbow.Consts;
using Rainbow.Delegates;
using Rainbow.Example.CommonWebHook;
using Rainbow.Enums;
using Rainbow.Example.Common;
using Rainbow.Model;
using System.Runtime.CompilerServices;

internal class RainbowAdminBot
{
    internal readonly ILogger log;

    public UserConfig RainbowAccount { get; private set; }

    private readonly Rainbow.Application _rbApplication;
    private readonly Administration _rbAdministration;
    private readonly AutoReconnection _rbAutoReconnection;
    private readonly Contacts _rbContacts;

    private readonly String _callbackUrl;
    private CompanyEventWebHook? _companyEventWebHook;
    private CompanyEventSubscription? _companyEventSubscription;

    public event PresenceDelegate? ContactPresenceUpdated;
    public event PresenceDelegate? ContactAggregatedPresenceUpdated;

    public event ConnectionStateDelegate? ConnectionStateChanged;
    public event SdkErrorDelegate? ConnectionFailed;

    internal RainbowAdminBot(ServerConfig serverConfig, UserConfig rainbowAccount, String callbackUrl)
    {
        RainbowAccount = rainbowAccount;

        if(!callbackUrl.EndsWith("/"))
            callbackUrl += "/";
        _callbackUrl = callbackUrl;

        log = LogFactory.CreateLogger<Application>(rainbowAccount.Prefix);
        LogInjectionManager.RegisterLogger(this, log);


        // Set restrictions
        Restrictions restrictions = new(true)
        {
            EventMode = SdkEventMode.NONE, // We don't need event mode

            LogRestRequest = true,
        };

        _rbApplication = new Rainbow.Application(
                iniFolderFullPathName: rainbowAccount.IniFolderPath,
                iniFileName: rainbowAccount.Prefix + "file.ini",
                loggerPrefix: rainbowAccount.Prefix,
                restrictions: restrictions);

        // Get services
        _rbAdministration = _rbApplication.GetAdministration();
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
        _rbContacts.ContactAggregatedPresenceUpdated += RbContacts_ContactAggregatedPresenceUpdated;

        // Create Logger for the Web Server
        Swan.Logging.Logger.NoLogging();
        Swan.Logging.Logger.RegisterLogger(new SwanLogger(rainbowAccount.Prefix));

        // Create WebServer
        var webServer = CallbackWebHookModule.CreateWebServer("http://localhost:9870", _rbApplication);
        var _ = webServer.RunAsync();

        // Start Login
        Login();
    }

#pragma warning disable CA1822
    // /!\ This method must NOT be static
    [LogInjection(PreventException = true)]
    private void RaiseEvent(Delegate? eventDelegate, Object[] args, [CallerArgumentExpression(nameof(eventDelegate))] string eventName = null)
        => Rainbow.Util.RaiseEvent(this, eventDelegate, eventName, args);

#pragma warning restore CA1822

    public void Login()
    {
        if (!_rbApplication.IsConnected())
        {
            Task.Run(async () =>
            {
                var sdkResult = await _rbApplication.LoginAsync(RainbowAccount.Login, RainbowAccount.Password);
                if (!sdkResult.Success)
                    RaiseEvent(ConnectionFailed, [sdkResult.Result]);
        });
        }
    }

    public async Task<SdkResult<CompanyEventSubscriptionStatus>> GetCompanyEventSubscriptionStatusAsync()
    {
        return await _rbAdministration.GetCompanyEventSubscriptionStatusAsync(_companyEventSubscription);
    }

    private async Task<SdkResult<Boolean>> CreateWebhookAndSubscriptionsAsync()
    {
        var currentContact = _rbContacts.GetCurrentContact();
        var companyId = currentContact.CompanyId;

        SdkResult<Boolean>? sdkResultBool;
        SdkResult<CompanyEventWebHook>? sdkResultHook;
        SdkResult<CompanyEventSubscription>? sdkResultSubscription;

        // Delete any previous subscriptions
        var sdkResultSubscriptions = await _rbAdministration.GetCompanyEventSubscriptionsAsync(companyId);
        if (sdkResultSubscriptions.Success)
        {
            var list = sdkResultSubscriptions.Data;
            if (list?.Count > 0)
            {
                foreach (var sub in list)
                {
                    sdkResultBool = await _rbAdministration.DeleteCompanyEventSubscriptionAsync(sub);
                    if (sdkResultBool.Success)
                        ConsoleAbstraction.WriteBlue($"CompanyEventSubscription deleted:[{sub}]");
                    else
                        ConsoleAbstraction.WriteRed($"CompanyEventSubscription NOT deleted:[{sdkResultBool.Result}]");
                }
            }
        }
        else
        {
            ConsoleAbstraction.WriteRed($"GetCompanyEventSubscriptionsAsync - Error:[{sdkResultSubscriptions.Result}]");
            return new SdkResult<Boolean>(sdkResultSubscriptions.Result);
        }

        // Get web hooks
        var sdkResultHooks = await _rbAdministration.GetCompanyEventWebHooksAsync(companyId);
        if (sdkResultHooks.Success)
        {
            var list = sdkResultHooks.Data;
            if (list?.Count > 0)
            {
                // Delete previous hooks
                foreach (var sub in list)
                {
                    sdkResultBool = await _rbAdministration.DeleteCompanyEventWebHookAsync(sub);
                    if (sdkResultBool.Success)
                        ConsoleAbstraction.WriteBlue($"CompanyEventWebHook deleted:[{sub}]");
                    else
                        ConsoleAbstraction.WriteRed($"CompanyEventWebHook NOT deleted:[{sdkResultBool.Result}]");
                }
            }
        }
        else
        {
            ConsoleAbstraction.WriteRed($"GetCompanyEventWebHooksAsync - Error:[{sdkResultHooks.Result}]");
            return new SdkResult<Boolean>(sdkResultHooks.Result);
        }

        // Create Company Event Webhook
        sdkResultHook = await _rbAdministration.CreateCompanyEventWebHookAsync(companyId, _callbackUrl);
        if (sdkResultHook.Success)
        {
            _companyEventWebHook = sdkResultHook.Data;
            ConsoleAbstraction.WriteBlue($"CompanyEventWebHook created:[{_companyEventWebHook}]");

            String eventType = "presence";

            // Create Company Event Subscription
            sdkResultSubscription = await _rbAdministration.CreateCompanyEventSubscriptionAsync(companyId, _companyEventWebHook, eventType);
            if (sdkResultSubscription.Success)
            {
                _companyEventSubscription = sdkResultSubscription.Data;
                ConsoleAbstraction.WriteBlue($"CompanyEventSubscription created:[{_companyEventSubscription}]");

                return new SdkResult<Boolean>(true);
            }
            else
            {
                ConsoleAbstraction.WriteRed($"CreateCompanyEventSubscriptionAsync - Error:[{sdkResultSubscription.Result}]");
                return new SdkResult<Boolean>(sdkResultSubscription.Result);
            }
        }
        else
        {
            ConsoleAbstraction.WriteRed($"CreateCompanyEventWebHookAsync - Error:[{sdkResultHook.Result}]");
            return new SdkResult<Boolean>(sdkResultHook.Result);
        }
    }

#region Event from RB SDK

    private void RbApplication_AuthenticationFailed(SdkError sdkError)
    {
        RaiseEvent(ConnectionFailed, [sdkError]);
    }

    private void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
    {
        RaiseEvent(ConnectionStateChanged, [connectionState]);

        if (connectionState.Status == ConnectionStatus.Connected)
        {
            Task.Run(() =>
            {
                // create Webhook + subscription to get Presence Level
                var _ = CreateWebhookAndSubscriptionsAsync();
            });
        }

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
        if (sdkError.IncorrectUseError?.ErrorDetailsCode == (int)SdkInternalErrorEnum.LOGIN_PROCESS_MAX_ATTEMPTS_REACHED)
        {
            // The auto reconnection service try to connect on server but failed after a lot of retry ....
            // We do it again and again ... 
            Login();
        }
        else
            RaiseEvent(ConnectionFailed, [sdkError]);
    }

    private void RbContacts_ContactAggregatedPresenceUpdated(Presence presence)
    {
        //ConsoleAbstraction.WriteDarkYellow($"Aggregated Presence Updated:[{presence.ToString(DetailsLevel.Medium)}]");
        RaiseEvent(ContactAggregatedPresenceUpdated, [presence]);
    }

    private void RbContacts_ContactPresenceUpdated(Presence presence)
    {
        //ConsoleAbstraction.WriteDarkYellow($"Presence Updated:[{presence.ToString(DetailsLevel.Medium)}]");
        RaiseEvent(ContactPresenceUpdated, [presence]);
    }

#endregion Event from RB SDK

}

