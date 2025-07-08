using EmbedIO;
using Rainbow;
using Rainbow.Consts;
using Rainbow.Delegates;
using Rainbow.Enums;
using Rainbow.Example.Common;
using Rainbow.Model;
using Util = Rainbow.Example.Common.Util;


internal class RainbowAdminBot
{
    const int CLOUDPBX_ROW_SIZE = 5;                        // Cloud PBX list asked in same time
    const int USERS_ROW_SIZE = 200;                         // Devices list asked in same time
    const int DEVICES_ROW_SIZE = 200;                       // Devices list asked in same time
    const int DEVICE_REGISTRATION_SIMULTANEOUS_CALL = 10;   // Nb of simultaneous HTTP request made to know registration status of devices

    public UserConfig RainbowAccount { get; private set; }

    private readonly Rainbow.Application _rbApplication;
    private readonly AutoReconnection _rbAutoReconnection;
    private readonly Administration _rbAdministration;
    private readonly Contacts _rbContacts;
    private readonly HubTelephony _rbHubTelephony;

    private readonly String _callbackUrl;
    private CompanyEventWebHook? _companyEventWebHook;
    private CompanyEventSubscription? _companyEventSubscription;

    private readonly List<HubAdminCloudPBXInfo> _hubAdminCloudPBXInfoList;          // To store all Cloud Pbx
    private readonly Dictionary<String, Peer> _peersById;                           // To store all peers
    private readonly Dictionary<String, HubAdminDevice> _hubAdminDevicesById;       // To store all devices
    private readonly Dictionary<String, SIPDeviceStatus> _sipDeviceStatusByDeviceId;// To store all device status
    private readonly Object lockSIPDeviceStatus = new();

    internal delegate void SIPDeviceSatusDelegate(SIPDeviceStatus sipDeviceStatus);

    internal event SIPDeviceSatusDelegate? SIPDeviceStatusChanged;
    internal event ConnectionStateDelegate? ConnectionStateChanged;
    internal event SdkErrorDelegate? ConnectionFailed;

    internal RainbowAdminBot(ServerConfig serverConfig, UserConfig rainbowAccount, String callbackUrl)
    {
        RainbowAccount = rainbowAccount;

        if(!callbackUrl.EndsWith("/"))
            callbackUrl += "/";
        _callbackUrl = callbackUrl;

        _hubAdminCloudPBXInfoList = [];
        _peersById = [];
        _hubAdminDevicesById = [];
        _sipDeviceStatusByDeviceId = [];

        NLogConfigurator.AddLogger(rainbowAccount.Prefix);

        // Create Logger for the Web Server
        Swan.Logging.Logger.NoLogging();
        Swan.Logging.Logger.RegisterLogger<SwanLogger>();


        _rbApplication = new Rainbow.Application(
                iniFolderFullPathName: rainbowAccount.IniFolderPath,
                iniFileName: rainbowAccount.Prefix + "file.ini",
                loggerPrefix: rainbowAccount.Prefix);

        // Set restrictions
        _rbApplication.Restrictions.LogRestRequest = true;
        _rbApplication.Restrictions.LogRestRequestOnError = true;
        _rbApplication.Restrictions.LogEvent = true;
        _rbApplication.Restrictions.LogEventRaised = true;
        _rbApplication.Restrictions.EventMode = SdkEventMode.NONE; // We don't need event mode
        _rbApplication.Restrictions.UseHubTelephony = true;
        _rbApplication.Restrictions.UseHybridTelephony = false;

        // Get services
        _rbAutoReconnection = _rbApplication.GetAutoReconnection();
        _rbAdministration = _rbApplication.GetAdministration();
        _rbContacts = _rbApplication.GetContacts();
        _rbHubTelephony = _rbApplication.GetHubTelephony();

        // Configure AutoReconnection service
        _rbAutoReconnection.UsePreviousLoginPwd = true; // we want to auto reconnect using previous login/pwd even if security token has expired

        // Configure Application service
        _rbApplication.SetApplicationInfo(serverConfig.AppId, serverConfig.AppSecret);
        _rbApplication.SetHostInfo(serverConfig.HostName);

        // We want to be inform of some events
        _rbApplication.AuthenticationFailed += RbApplication_AuthenticationFailed; ;
        _rbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;
        _rbAutoReconnection.Cancelled += RbAutoReconnection_Cancelled;
        _rbHubTelephony.DeviceStateUpdated += RbHubTelephony_DeviceStateUpdated;

        // Create WebServer
        var webServer = CreateWebServer("http://localhost:9870", callbackUrl);
        var _ = webServer.RunAsync();

        // Start Login
        Login();
    }

    public void Login()
    {
        if (!_rbApplication.IsConnected())
        {
            Task.Run(async () =>
            {
                var sdkResult = await _rbApplication.LoginAsync(RainbowAccount.Login, RainbowAccount.Password);
                if (!sdkResult.Success)
                    Rainbow.Util.RaiseEvent(() => ConnectionFailed, _rbApplication, sdkResult.Result);
        });
        }
    }

    public async Task<SdkResult<CompanyEventSubscriptionStatus>> GetCompanyEventSubscriptionStatusAsync()
    {
        return await _rbAdministration.GetCompanyEventSubscriptionStatusAsync(_companyEventSubscription);
    }

    private WebServer CreateWebServer(string url, String callbackUrl)
    {
        Uri uri = new Uri(callbackUrl);
        String callbackAbsolutePath = uri.AbsolutePath;

        WebServer server = new WebServer(o => o
                .WithUrlPrefix(url)
                .WithMode(HttpListenerMode.EmbedIO)
                )

            // First, we will configure our web server by adding Modules.
            .WithLocalSessionManager()
            .WithModule(new CallbackWebModule(callbackAbsolutePath, _rbApplication));

        server.WithStaticFolder("/", "./webSiteContent", true, configure =>
        {

        });

        return server;
    }

    public async Task<SdkResult<String>> MakeRestRequestAsync(String requestUri, String requestMethod, String? body = null, Dictionary<string, object>? queryParameters = null, Dictionary<String, String>? specificHeaders = null, String? acceptContentType = null, int acceptHttpStatusCode = 200)
    {
        var httpClient = _rbApplication?.GetSecondaryHttpClient();

        if(httpClient is null)
            return new SdkResult<String>(SdkInternalError.FromInternalError(SdkInternalErrorEnum.OPERATION_FAILED, new[] { "HttpClient is null", "MakeRestRequestAsync" }));

        HttpRequestDescriptor httpRequestDescriptor = HttpRequestDescriptor.JsonRequest(requestUri, requestMethod, body);
        httpRequestDescriptor.AddQueryParameters(queryParameters);
        httpRequestDescriptor.AddAcceptContentType(acceptContentType);
        httpRequestDescriptor.AddAcceptHttpStatusCode(acceptHttpStatusCode);
        if (specificHeaders?.Count > 0)
            httpRequestDescriptor.AddHeaders(specificHeaders);

        return await httpClient.RequestAsStringAsync(httpRequestDescriptor);
    }

    private async Task<SdkResult<Boolean>> GetAllMandatoryInfoAsync()
    {
        Boolean mustTryAgain;
        SdkResult<Boolean> sdkResultBoolean;

        // We want to get all contacts in the compagny
        do
        {
            sdkResultBoolean = await GetAllContactsInCompanyAsync();
            mustTryAgain = !sdkResultBoolean.Success;
            if (!sdkResultBoolean.Success)
                Rainbow.Example.Common.Util.WriteRed($"Cannot get all contacts in company - Error:[{sdkResultBoolean.Result}]");
        }
        while (mustTryAgain);
        Util.WriteBlue($"Nb users in company:[{_peersById.Count}]");

        if (!sdkResultBoolean.Success)
            return sdkResultBoolean;

        // We want to get all CloudPBX in the compagny
        do
        {
            sdkResultBoolean = await GetAllCloudPbxInCompanyAsync();
            mustTryAgain = !sdkResultBoolean.Success;
            if (!sdkResultBoolean.Success)
                Rainbow.Example.Common.Util.WriteRed($"Cannot get all Cloud in company - Error:[{sdkResultBoolean.Result}]");
        }
        while (mustTryAgain);
        Util.WriteBlue($"Nb Cloud PBX  in company:[{_hubAdminCloudPBXInfoList.Count}]");

        if (!sdkResultBoolean.Success)
            return sdkResultBoolean;

        // We want to get all Hub Devices in the compagny
        do
        {
            sdkResultBoolean = await GetAllHubDevicesInCompanyAsync();
            mustTryAgain = !sdkResultBoolean.Success;
            if (!sdkResultBoolean.Success)
                Rainbow.Example.Common.Util.WriteRed($"Cannot get all Hub Devices in company - Error:[{sdkResultBoolean.Result}]");
        }
        while (mustTryAgain);
        Util.WriteBlue($"Nb Hub Devices in company:[{_hubAdminDevicesById.Count}]");

        return sdkResultBoolean;

    }

    private async Task<SdkResult<Boolean>> GetAllContactsInCompanyAsync()
    {
        var company = _rbContacts.GetCompany();

        int offset = 0;
        int rowSize = USERS_ROW_SIZE;
        
        Boolean dontStop;
        SdkResult<Boolean> result = new(true);

        _peersById.Clear();
        do
        {
            var sdkResult = await _rbAdministration.GetUsersAsync(false, offset, rowSize, company.Id, null, false, null);
            if (sdkResult.Success)
            {
                var list = sdkResult.Data;
                
                // Store peer info
                foreach (var contact in list.Data)
                    _peersById[contact.Peer.Id] = contact.Peer;

                // Do we continue and how ?
                dontStop = !(list.Data.Count < rowSize);
                offset += list.Data.Count;
            }
            else
            {
                // Store error
                result = new SdkResult<Boolean>(sdkResult.Result);
                dontStop = false;
            }
        }
        while (dontStop);


        return result;
    }

    private async Task<SdkResult<Boolean>> GetAllCloudPbxInCompanyAsync()
    {
        var company = _rbContacts.GetCompany();

        int offset = 0;
        int rowSize = CLOUDPBX_ROW_SIZE;

        Boolean canContinue;
        SdkResult<Boolean> result = new(true);

        _hubAdminCloudPBXInfoList.Clear();
        do
        {
            var sdkResult = await _rbAdministration.GetHubCloudPBXAsync(offset, rowSize, company.Id, null);
            if(sdkResult.Success)
            {
                var list = sdkResult.Data;

                // Store peer info
                if (list?.Data?.Count > 0)
                {
                    foreach (var pbx in list.Data)
                        _hubAdminCloudPBXInfoList.Add(pbx);

                    // Do we continue and how ?
                    canContinue = !(list.Data.Count < rowSize);
                    offset += list.Data.Count;
                }
                else
                    canContinue = false;
            }
            else
            {
                // Store error
                result = new SdkResult<Boolean>(sdkResult.Result);
                canContinue = false;
            }
        }
        while (canContinue);


        return result;
    }

    private async Task<SdkResult<Boolean>> GetAllHubDevicesInCompanyAsync()
    {
        int rowSize = DEVICES_ROW_SIZE;

        Boolean canContinue;
        SdkResult<Boolean> result = new(true);

        _hubAdminDevicesById.Clear();
        // Loop on each cloud pbx
        foreach (var cloudPbx in _hubAdminCloudPBXInfoList)
        {
            var offset = 0;
            do
            {
                var sdkResult = await _rbAdministration.GetHubDevicesAsync(cloudPbx.Id, offset, rowSize, banned: false);
                if (sdkResult.Success)
                {
                    var list = sdkResult.Data;

                    // Store peer info
                    if (list?.Data?.Count > 0)
                    {
                        foreach (var device in list.Data)
                            _hubAdminDevicesById[device.Id] = device;

                        // Do we continue and how ?
                        canContinue = !(list.Data.Count < rowSize);
                        offset += list.Data.Count;
                    }
                    else
                        canContinue = false;
                }
                else
                {
                    // Store error
                    result = new SdkResult<Boolean>(sdkResult.Result);
                    canContinue = false;
                }
            }
            while (canContinue);

            // Don't continue to loop on clou PBX since we got an error
            if (!result.Success)
                break;
        }

        return result;
    }

    private async Task<SdkResult<Boolean>> CreateWebhookAndSubscriptionsAsync()
    {
        var currentContact = _rbContacts.GetCurrentContact();
        var companyId = currentContact.CompanyId;

        SdkResult<Boolean>? sdkResultBool;
        SdkResult<String>? sdkResultString;
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
                        Util.WriteBlue($"CompanyEventSubscription deleted:[{sub}]");
                    else
                        Util.WriteRed($"CompanyEventSubscription NOT deleted:[{sdkResultBool.Result}]");
                }
            }
        }
        else
        {
            Util.WriteRed($"GetCompanyEventSubscriptionsAsync - Error:[{sdkResultSubscriptions.Result}]");
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
                        Util.WriteBlue($"CompanyEventWebHook deleted:[{sub}]");
                    else
                        Util.WriteRed($"CompanyEventWebHook NOT deleted:[{sdkResultBool.Result}]");
                }
            }
        }
        else
        {
            Util.WriteRed($"GetCompanyEventWebHooksAsync - Error:[{sdkResultHooks.Result}]");
            return new SdkResult<Boolean>(sdkResultHooks.Result);
        }

        // Create Company Event Webhook
        sdkResultHook = await _rbAdministration.CreateCompanyEventWebHookAsync(companyId, _callbackUrl);
        if (sdkResultHook.Success)
        {
            _companyEventWebHook = sdkResultHook.Data;
            Util.WriteBlue($"CompanyEventWebHook created:[{_companyEventWebHook}]");

            String eventType = "device_state";

            // Create Company Event Subscription
            sdkResultSubscription = await _rbAdministration.CreateCompanyEventSubscriptionAsync(companyId, _companyEventWebHook, eventType);
            if (sdkResultSubscription.Success)
            {
                _companyEventSubscription = sdkResultSubscription.Data;
                Util.WriteBlue($"CompanyEventSubscription created:[{_companyEventSubscription}]");

                return new SdkResult<Boolean>(true);
            }
            else
            {
                Util.WriteRed($"CreateCompanyEventSubscriptionAsync - Error:[{sdkResultSubscription.Result}]");
                return new SdkResult<Boolean>(sdkResultSubscription.Result);
            }
        }
        else
        {
            Util.WriteRed($"CreateCompanyEventWebHookAsync - Error:[{sdkResultHook.Result}]");
            return new SdkResult<Boolean>(sdkResultHook.Result);
        }
    }

    private async Task AskAllHubDevicesRegistrationAsync()
    {
        // Make several request in sametime
        var hubAdminDevices = _hubAdminDevicesById.Values;
        var total = hubAdminDevices.Count;
        var current = 0;
        while (current < total)
        {
            var sublistDevices = hubAdminDevices.Skip(current).Take(DEVICE_REGISTRATION_SIMULTANEOUS_CALL);
            var sublistDevicesTasks = new List<Task>();
            foreach (var device in sublistDevices)
                sublistDevicesTasks.Add(AskHubDeviceRegistrationAsync(device.SystemId, device.Id, device.UserId, device.ShortNumber));
            await Task.WhenAll(sublistDevicesTasks);

            current += DEVICE_REGISTRATION_SIMULTANEOUS_CALL;
        }
    }

    private async Task<SdkResult<Boolean>> AskHubDeviceRegistrationAsync(String cloudId, String deviceId, String userId, String deviceShortNumber)
    {
        // We ask server only if we don't have the info yet
        lock (lockSIPDeviceStatus)
        {
            if (_sipDeviceStatusByDeviceId.TryGetValue(deviceId,  out var oldSipDeviceStatus))
                return new SdkResult<Boolean>(true);
        }

        var sdkResultRegistration = await _rbAdministration.GetHubDeviceRegistrationAsync(cloudId, deviceId);
        if (sdkResultRegistration.Success)
        {
            var registration = sdkResultRegistration.Data;

            // Do we know this peer ? If not ask server
            if (!_peersById.TryGetValue(userId, out Peer? peer))
            {
                var sdkResultContact = await _rbContacts.GetContactByIdAsync(userId);
                if (sdkResultContact.Success)
                {
                    peer = sdkResultContact.Data.Peer;
                    _peersById[peer.Id] = peer;
                }
            }

            if(peer is null)
            {
                peer = Peer.FromContactId(userId);
                peer.PhoneNumber = deviceShortNumber;
            }

            SIPDeviceStatus sipDeviceStatus = new()
            {
                Peer = peer,
                DeviceId = deviceId,
                DeviceShortNumber = deviceShortNumber,
                Registered = (registration != null)
            };

            CheckAndStoreSIPDeviceStatus(sipDeviceStatus, false);

            return new SdkResult<Boolean>(true);
        }
        else
            return new SdkResult<Boolean>(sdkResultRegistration.Result);
    }

    private void CheckAndStoreSIPDeviceStatus(SIPDeviceStatus sipDeviceStatus, Boolean needUpdate)
    {
        bool raiseEvent = false;
        lock (lockSIPDeviceStatus)
        {
            if (_sipDeviceStatusByDeviceId.TryGetValue(sipDeviceStatus.DeviceId, out var oldSipDeviceStatus))
            {
                if (!needUpdate)
                    return;

                if (oldSipDeviceStatus.Registered != sipDeviceStatus.Registered)
                {
                    raiseEvent = true;
                    _sipDeviceStatusByDeviceId[sipDeviceStatus.DeviceId] = sipDeviceStatus;
                }
            }
            else
            {
                raiseEvent = true;
                _sipDeviceStatusByDeviceId[sipDeviceStatus.DeviceId] = sipDeviceStatus;
            }
        }
        if (raiseEvent)
            Rainbow.Util.RaiseEvent(() => SIPDeviceStatusChanged, _rbApplication, sipDeviceStatus);
    }

    private String GetInternalPhoneNumber(Contact contact)
    {
        if (contact is null)
            return "";

        if (contact.PhoneNumbers?.Count > 0)
        {
            foreach (var phoneNumber in contact.PhoneNumbers)
            {
                if (!String.IsNullOrEmpty(phoneNumber.PbxId))
                {
                    if (phoneNumber.IsFromSystem)
                    {
                        return phoneNumber.InternalNumber;
                    }
                }
            }
        }
        return "";
    }

#region Event from RB SDK

    private void RbApplication_AuthenticationFailed(SdkError sdkError)
    {
        Rainbow.Util.RaiseEvent(() => ConnectionFailed, _rbApplication, sdkError);
    }

    private void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
    {
        Rainbow.Util.RaiseEvent(() => ConnectionStateChanged, _rbApplication, connectionState);

        if (connectionState.Status == ConnectionStatus.Connected)
        {
            Task.Run(async () =>
            {
                var sdkResult = await GetAllMandatoryInfoAsync();
                if (sdkResult.Success)
                {
                    // Ask all Hub devices registration
                    var _1 = AskAllHubDevicesRegistrationAsync();

                    // In the same time create Webhook + subscription to get Hub devices registration update
                    var _2 = CreateWebhookAndSubscriptionsAsync();
                }
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
        if (sdkError?.IncorrectUseError.ErrorDetailsCode == (int)SdkInternalErrorEnum.LOGIN_PROCESS_MAX_ATTEMPTS_REACHED)
        {
            // The auto reconnection service try to connect on server but failed after a lot of retry ....
            // We do it again and again ... 
            Login();
        }
        else
            Rainbow.Util.RaiseEvent(() => ConnectionFailed, _rbApplication, sdkError);
    }

    private void RbHubTelephony_DeviceStateUpdated(HubDeviceState hubDeviceState)
    {
        SIPDeviceStatus sipDeviceStatus;
        lock (lockSIPDeviceStatus)
        {
            sipDeviceStatus = new()
            {
                Peer = hubDeviceState.Contact,
                DeviceId = hubDeviceState.DeviceId,
                DeviceShortNumber = GetInternalPhoneNumber(hubDeviceState.Contact),
                Registered = hubDeviceState.InService == true
            };
        }
        CheckAndStoreSIPDeviceStatus(sipDeviceStatus, true);
    }

#endregion Event from RB SDK

}

