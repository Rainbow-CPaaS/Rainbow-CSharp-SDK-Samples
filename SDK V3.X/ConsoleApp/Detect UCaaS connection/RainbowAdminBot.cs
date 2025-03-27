using LoginSSO;
using Rainbow;
using Rainbow.Console;
using Rainbow.Consts;
using Rainbow.Delegates;
using Rainbow.Enums;
using Rainbow.Model;
using Rainbow.SimpleJSON;


internal class RainbowAdminBot
{
    const int CLOUDPBX_ROW_SIZE = 5;                        // Cloud PBX list asked in same time
    const int DEVICES_ROW_SIZE = 200;                       // Devices list asked in same time
    const int DEVICE_REGISTRATION_SIMULTANEOUS_CALL = 10;   // Nb of simultaneous HTTP request made to know registration status of devices

    public UserConfig RainbowAccount { get; private set; }


    private Rainbow.Application _rbApplication;
    private AutoReconnection _rbAutoReconnection;
    private Administration _rbAdministration;
    private Contacts _rbContacts;

    private Dictionary<String, Peer>? _peersById = null;
    private Dictionary<String, SIPDeviceStatus>? _sipDeviceStatusByDeviceId = null;

    internal delegate void SIPDeviceSatusDelegate(SIPDeviceStatus sipDeviceStatus);

    internal event SIPDeviceSatusDelegate? SIPDeviceStatusChanged;
    internal event ConnectionStateDelegate? ConnectionStateChanged;
    internal event SdkErrorDelegate? ConnectionFailed;

    internal RainbowAdminBot(ServerConfig serverConfig, UserConfig rainbowAccount)
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
        _rbApplication.Restrictions.EventMode = SdkEventMode.NONE; // We don't need event mode

        // Get services
        _rbAutoReconnection = _rbApplication.GetAutoReconnection();
        _rbAdministration = _rbApplication.GetAdministration();
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

    private async void RbApplication_AuthenticationFailed(SdkError sdkError)
    {
        Rainbow.Util.RaiseEvent(() => ConnectionFailed, _rbApplication, sdkError);

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
        if(!sdkResult.Success)
        {
            // it was not possible to make REST request to my server ...
        }
    }

    private async void RbApplication_ConnectionStateChanged(Rainbow.Model.ConnectionState connectionState)
    {
        Rainbow.Util.RaiseEvent(() => ConnectionStateChanged, _rbApplication, connectionState);

        if (connectionState.Status == ConnectionStatus.Connected)
        {
            // We want to get all contacts in the compagny
            _peersById ??= new();
            _peersById.Clear();
            await GetAllContactsInCompanyAsync();

            // Now we start to check SIP devices of this company
            _sipDeviceStatusByDeviceId ??= new();
            _sipDeviceStatusByDeviceId.Clear();
            var _ = CheckSIPDevicesAsync();
        }

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

    private async Task AskHubDeviceRegistrationAsync(String cloudId, String deviceId, String deviceUserId, String deviceShortNumber)
    {
        //await _rbAdministration.GetHubDeviceAsync(cloudId, deviceId);

        var sdkResultRegistration = await _rbAdministration.GetHubDeviceRegistrationAsync(cloudId, deviceId);
        if (sdkResultRegistration.Success)
        {
            var registration = sdkResultRegistration.Data;
            Peer peer = null;

            // Do we know this peer ? If not ask server
            if (!_peersById.TryGetValue(deviceUserId, out peer))
            {
                var sdkResultContact = await _rbContacts.GetContactByIdAsync(deviceUserId);
                if (sdkResultContact.Success)
                    peer = sdkResultContact.Data.Peer;
            }

            SIPDeviceStatus sipDeviceStatus = new();
            sipDeviceStatus.Peer = peer;
            sipDeviceStatus.DeviceId = deviceId;
            sipDeviceStatus.DeviceShortNumber = deviceShortNumber;
            sipDeviceStatus.Registered = (registration != null);


            bool raiseEvent = false;
            if (_sipDeviceStatusByDeviceId.TryGetValue(deviceId, out var oldSipDeviceStatus))
            {
                if (oldSipDeviceStatus.Registered != sipDeviceStatus.Registered)
                {
                    raiseEvent = true;
                    _sipDeviceStatusByDeviceId[deviceId] = sipDeviceStatus;
                }
            }
            else
            {
                raiseEvent = true;
                _sipDeviceStatusByDeviceId[deviceId] = sipDeviceStatus;
            }

            if (raiseEvent)
                Rainbow.Util.RaiseEvent(() => SIPDeviceStatusChanged, _rbApplication, sipDeviceStatus);
        }

    }

    private async Task CheckSIPDevicesAsync()
    {
        var company = _rbContacts.GetCompany();
        var sdkResultCloudPBX =  await _rbAdministration.GetHubCloudPBXAsync(0, CLOUDPBX_ROW_SIZE, company.Id, null);
        if(sdkResultCloudPBX.Success)
        {
            var listCloudPbx = sdkResultCloudPBX.Data?.Data;
            if(listCloudPbx?.Count > 0)
            {
                foreach(var cloud in listCloudPbx)
                {
                    if(cloud.Status == "activated")
                    {
                        int offset = 0;
                        Boolean dontStop = true;

                        while (dontStop && _rbApplication.IsConnected())
                        {
                            // Get Hub Devices not banned
                            var sdkResultHubDevices = await _rbAdministration.GetHubDevicesAsync(cloud.Id, offset, DEVICES_ROW_SIZE, banned: false);
                            if (sdkResultHubDevices.Success)
                            {
                                var hubDevices = sdkResultHubDevices.Data?.Data;

                                if (hubDevices?.Count > 0)
                                {
                                    // Do we continue and how ?
                                    dontStop = !(hubDevices.Count < DEVICES_ROW_SIZE);
                                    offset += hubDevices.Count;


                                    // Make several request in sametime
                                    var total = hubDevices.Count;
                                    var current = 0;
                                    while (current < total)
                                    {
                                        var sublistDevices = hubDevices.Skip(current).Take(DEVICE_REGISTRATION_SIMULTANEOUS_CALL);
                                        var sublistDevicesTasks = new List<Task>();
                                        foreach (var device in sublistDevices)
                                        {
                                            sublistDevicesTasks.Add(AskHubDeviceRegistrationAsync(cloud.Id, device.Id, device.UserId, device.ShortNumber));
                                        }
                                        await Task.WhenAll(sublistDevicesTasks);

                                        current += DEVICE_REGISTRATION_SIMULTANEOUS_CALL;
                                    }


                                    //foreach (var device in hubDevices)
                                    //{
                                    //    await AskHubDeviceRegistrationAsync(cloud.Id, device.Id, device.UserId, device.ShortNumber);

                                    //    if (!_rbApplication.IsConnected())
                                    //        break;
                                    //}
                                }
                                else
                                    dontStop = false;
                            }
                            else
                            {
                                dontStop = false;
                            }
                        }
                    }
                }
            }
        }


        // We do the process again if are still connected
        if(_rbApplication.IsConnected())
        {
            //return;
            var _ = CheckSIPDevicesAsync();
        }
    }

    private async Task GetAllContactsInCompanyAsync()
    {
        var company = _rbContacts.GetCompany();

        int offset = 0;
        int rowSize = 200;
        
        
        Boolean dontStop;

        ContactsListData contactsListData;
        do
        {
            var sdkResult = await _rbAdministration.GetUsersAsync(false, offset, rowSize, company.Id, null, false, null);
            if (sdkResult.Success)
            {
                contactsListData = sdkResult.Data;
                
                // Store peer info
                foreach (var contact in contactsListData.Data)
                    _peersById[contact.Peer.Id] = contact.Peer;

                // Do we contniue and how ?
                dontStop = !(contactsListData.Data.Count < rowSize);
                offset += contactsListData.Data.Count;
            }
            else
            {
                dontStop = false;
            }
        }
        while (dontStop);


        var nbFounds = _peersById?.Count;

    }

}

