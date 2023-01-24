using NLog.Config;
using Rainbow.Model;
using Rainbow.WebRTC;
using Rainbow.Medias;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using static DirectShowLib.MediaSubType;
using Microsoft.Extensions.Logging.Abstractions;

namespace SDK.ConsoleApp.WebRTC
{
    internal class ConsoleAppWebRTC
    {

        enum VIDEO_REMOTE_DISPLAY
        {
            NONE,
            VIDEO,
            SHARING,
        }

        enum APP_STEP
        {
            NONE,
            CHECK_APPLICATION_INFO,
            CHECK_LIB_PATH,
            INIT_LIBRARIES,
            ASK_DEVICES,
            STAR_LOGIN_PROCESS,
            LOGIN_PROCESS_IN_PROGRESS,
            LOGIN_AND_INIT_DONE,
            SELECTION_MODE,
            SEARCH_USER_TYPING,
            SEARCH_USER_IN_PROGRESS,
            SEARCH_USER_SELECTION,
            SEARCH_USER_SELECTION_DONE,
            SEARCH_USER_SELECTED_KNOWN,
            WAIT_CONFERENCE_MESSAGE,
            WAIT_CONFERENCE,
            CALL_IN_PROGRESS,
            CALL_ENDED,
        }

        static APP_STEP appStep = APP_STEP.NONE;
        static VIDEO_REMOTE_DISPLAY videoRemoteDisplay = VIDEO_REMOTE_DISPLAY.NONE;
        static int remoteMedias = 0;
        static Boolean quitApp = false;

        // Object used for serach purpose        
        static int MAX_SEARCH_RESULT = 20; // Cannot be greater than 20
        static int currentSearchPage = 0;
        static String? currentSearchCriteria = "";

        // Define Rainbow SDK objects used in this example
        static Rainbow.Application? rbApplication;
        static Rainbow.Contacts? rbContacts;
        static Rainbow.Bubbles? rbBubbles;
        static Rainbow.Conferences? rbConferences;
        static Rainbow.WebRTC.WebRTCCommunications? rbWebRTCCommunications;

        // Define devices which will be used in WebRtc Conversation
        static Rainbow.Medias.Device? videoInputSelected = null;
        static Rainbow.Medias.Device? audioInputSelected = null;

        static Rainbow.Medias.MediaInput? videoInputStream = null;
        static Rainbow.Medias.MediaInput? audioInputStream = null;

        static Rainbow.Medias.Device? audioOutputSelected = null;

        // Define Contacts objects: current user and the peer used in the WebRtc Conversation
        static Rainbow.Model.Contact? currentContact = null;
        static Rainbow.Model.Contact? selectedContact = null;
        static Boolean askingMoreInfoAboutContact = false;

        // To store list of contacts found
        static List<Rainbow.Model.Contact>? contactsFound = null;

        // Define Call object
        static Rainbow.Model.Call? currentCall = null;
        static String? currentCallId = null;
        static String? currentConfId = null;
        static String selectionMode = "P2P";
        static Boolean isParticipating = false;

        static String? previousConferenceId;
        static DateTime? conferenceDateTime;

        // Object to display remote video in ASCII
        static Rainbow.AsciiFrame? asciiFrame = null;
        static String footerLine1 = "";
        static String footerLine2 = "";

    #region Main

        static void Main(string[] args)
        {
            Console.Clear();

            if (!InitLogsWithNLog())
            {
                Console.WriteLine("Cannot initialized application log files ...");
                return;
            }

            // Check if APP_ID,  APP_SECRET_KEY,  HOST_NAME, LOGIN_USER and LOGIN_PASSWORD are set
            appStep = APP_STEP.CHECK_APPLICATION_INFO;
            if (!CheckApplicationInfo())
                return;

            // Check if FFmpeg lib folder path exists
            appStep = APP_STEP.CHECK_LIB_PATH;
            if (!CheckFFmpegLibFolderPath())
                return;

            // Create Rainbow.Application
            rbApplication = new Rainbow.Application();
            rbApplication.SetApplicationInfo(ApplicationInfo.APP_ID, ApplicationInfo.APP_SECRET_KEY);
            rbApplication.SetHostInfo(ApplicationInfo.HOST_NAME);
            rbApplication.Restrictions.UseSameResourceId = true; // We want to use same resourceId each time
            rbApplication.Restrictions.UseWebRTC = true; // We will use WebRTC features

            // Create Rainbow.Contacts service
            rbContacts = rbApplication.GetContacts();

            // Create Rainbow.Bubles service
            rbBubbles = rbApplication.GetBubbles();

            // Create Rainbow.Conferences service
            rbConferences = rbApplication.GetConferences();

            // Create Rainbow.WebRTC.Communication service
            appStep = APP_STEP.INIT_LIBRARIES;
            Console.WriteLine($"\nFFmpeg libraries must be stored here:[{ApplicationInfo.FFMPEG_LIB_FOLDER_PATH}]");

            Console.WriteLine($"\nSDL2 library must be stored in the current folder:[{Rainbow.Util.GetSDKFolderPath()}]");
            Console.WriteLine($"\nTry to initalize external libraries: FFmpeg and SDL2 ...");

            try
            {
                // Do we use audio onyl ?
                //Rainbow.WebRTC.WebRTCCommunications.USE_ONLY_MICROPHONE_OR_HEADSET = ApplicationInfo.USE_ONLY_MICROPHONE_OR_HEADSET; //  /!\ We have to set this BEFORE TO CREATE rbCommunication object

                rbWebRTCCommunications = Rainbow.WebRTC.WebRTCCommunications.GetOrCreateInstance(rbApplication, ApplicationInfo.FFMPEG_LIB_FOLDER_PATH);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nInitialization failed ... Pleae ensure you have correclty defined libray path and copy libraries (FFmpeg and SDL2) into correct path");
                Console.WriteLine($"\nException:[{Rainbow.Util.SerializeException(ex)}]");
                return;
            }

            Console.WriteLine("\nInitialization done");

            // Create Rainbow.WebRTC.Devices service
            appStep = APP_STEP.ASK_DEVICES;

            // Select devices: Audio Input, Audio Output and Video Input
            if (!SelectAudioInputDevice())
            {
                Console.WriteLine("\nIt's mandatory in this sample to create a P2P call.");
                return;
            }

            //if (!SelectAudioOutputDevice())
            //{
            //    Console.WriteLine("\nIt's mandatory in this sample to create a P2P call.");
            //    return;
            //}

            SelectVideoInputDevice(); // Not mandatory to have a Video Input device: in this case only audio will be possible

            // Define Rainbow.Application events events callback we want to manage
            rbApplication.InitializationPerformed += RbApplication_InitializationPerformed;
            rbApplication.ConnectionStateChanged += RbApplication_ConnectionStateChanged;

            // Define Rainbow.WebRTC.Communication events callback we want to manage
            rbWebRTCCommunications.CallUpdated += RbWebRTCCommunications_CallUpdated;

            rbConferences.ConferenceRemoved += RbConferences_ConferenceRemoved;
            rbConferences.ConferenceUpdated += RbConferences_ConferenceUpdated;
            rbConferences.ConferenceParticipantsUpdated += RbConferences_ConferenceParticipantsUpdated;

            appStep = APP_STEP.STAR_LOGIN_PROCESS;

            ConsoleKeyInfo keyConsole;


            while (!quitApp)
            {
                switch (appStep)
                {
                    case APP_STEP.STAR_LOGIN_PROCESS:

                        // Try to connect to the server
                        appStep = APP_STEP.LOGIN_PROCESS_IN_PROGRESS;
                        Console.WriteLine($"\nUsing HostName:[{ApplicationInfo.HOST_NAME}] - User:[{ApplicationInfo.LOGIN_USER}] Trying to connect ...");

                        rbApplication.Login(ApplicationInfo.LOGIN_USER, ApplicationInfo.PASSWORD_USER, callback =>
                        {
                            if (callback.Result.Success)
                                Console.WriteLine("User / password are correct - waiting end of initialization");
                            else
                            {
                                Console.WriteLine("Login process failed");
                                if (callback.Result.Type == Rainbow.SdkError.SdkErrorType.IncorrectUse)
                                {
                                    Console.WriteLine("User / password are incorrect - you have to set correct credentials. Check also if APP_ID / APP_SECRET_KEY / HOST_NAME are correct.");
                                    Console.WriteLine($"Exception:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                }
                                else
                                {
                                    Console.WriteLine($"An error occurred\nException:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                }
                                quitApp = true;
                            }
                        });
                        break;

                    case APP_STEP.LOGIN_AND_INIT_DONE:
                        currentContact = rbContacts.GetCurrentContact();
                        Console.WriteLine($"You are now connected to Rainbow ({ApplicationInfo.HOST_NAME}) using:");
                        Console.WriteLine($"\tDisplay Name:[{Rainbow.Util.GetContactDisplayName(currentContact)}]");
                        Console.WriteLine($"\tCompany:[{currentContact.CompanyName}]");
                        Console.WriteLine($"\tEmail:[{ApplicationInfo.LOGIN_USER}]");
                        Console.WriteLine($"\tId:[{currentContact.Id}]");
                        Console.WriteLine($"\tJid:[{currentContact.Jid_im}]");

                        appStep = APP_STEP.SELECTION_MODE;



                        break;

                    case APP_STEP.SELECTION_MODE:
                        Console.WriteLine($"\nWhich mode to you want to use:");
                        Console.WriteLine($"\t[0] Search a user to call him in Peer to Peer communication");
                        Console.WriteLine($"\t[1] Wait until a Conference is started and join it automatically");
                        keyConsole = Console.ReadKey();
                        Console.WriteLine("");

                        if (keyConsole.KeyChar == '0')
                        {
                            appStep = APP_STEP.SEARCH_USER_TYPING;
                            selectionMode = "P2P";
                        }
                        else if (keyConsole.KeyChar == '1')
                        {
                            appStep = APP_STEP.WAIT_CONFERENCE_MESSAGE;
                            selectionMode = "CONFERENCE";
                        }
                        break;

                    case APP_STEP.WAIT_CONFERENCE_MESSAGE:
                        Console.WriteLine($"\nAsking for bubbles avaialble...");
                        appStep = APP_STEP.WAIT_CONFERENCE;

                        rbBubbles.GetAllBubbles(callback =>
                        {
                            if(callback.Result.Success)
                            {
                                var bubbles = callback.Data;
                                Console.WriteLine($"Existing bubbles:");
                                foreach (var bubble in bubbles)
                                {
                                    Console.WriteLine($"\t{bubble.Name} - [{bubble.Topic}]");
                                }
                                Console.WriteLine($"\nWaiting for a Conference in one of this bubbles ...");
                                appStep = APP_STEP.WAIT_CONFERENCE;
                            }
                            else
                            {
                                Console.WriteLine($"Error occurs. Trying again\n");
                                appStep = APP_STEP.WAIT_CONFERENCE_MESSAGE;
                            }
                        });
                        
                        break;

                    case APP_STEP.WAIT_CONFERENCE:
                        // Nothing to do here
                        break;

                    case APP_STEP.SEARCH_USER_TYPING:
                        Console.WriteLine($"\nSearch a user by Display Name (max result set to [{MAX_SEARCH_RESULT}]):");
                        currentSearchCriteria = Console.ReadLine();

                        if (currentSearchCriteria == null)
                            currentSearchCriteria = "";

                        currentSearchCriteria = currentSearchCriteria.Trim();
                        if (currentSearchCriteria.Length < 2)
                            Console.WriteLine($"\nUse at least two caracters to perform the search ...");
                        else
                        {
                            appStep = APP_STEP.SEARCH_USER_IN_PROGRESS;
                            rbContacts.SearchContactsByDisplayName(currentSearchCriteria, MAX_SEARCH_RESULT, callback =>
                            {
                                if (callback.Result.Success)
                                {
                                    Rainbow.Model.SearchContactsResult searchContactsResult = callback.Data;
                                    // We want to perform a WebRtcCall, so we check only Rainbow contacts
                                    if (searchContactsResult.ContactsList?.Count > 0)
                                    {
                                        contactsFound = searchContactsResult.ContactsList;
                                        currentSearchPage = 0;
                                        appStep = APP_STEP.SEARCH_USER_SELECTION;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"No contact found by Display Name using [{currentSearchCriteria}]");
                                        appStep = APP_STEP.SEARCH_USER_TYPING;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"An error occurred - try again\nException:[{Rainbow.Util.SerializeSdkError(callback.Result)}]");
                                    appStep = APP_STEP.SEARCH_USER_TYPING;
                                }
                            });
                        }
                        break;

                    case APP_STEP.SEARCH_USER_SELECTION:
                        if ((contactsFound == null) || (contactsFound.Count == 0))
                        {
                            appStep = APP_STEP.SEARCH_USER_TYPING;
                            break;
                        }

                        Console.WriteLine($"\nNb contacts found:[{contactsFound.Count}] using [{currentSearchCriteria}] - current page:[{currentSearchPage}] - Max results allowed:[{MAX_SEARCH_RESULT}]");
                        Console.WriteLine($"Select one contact using [0-9], use [n] or [p] to go to the next or previous page, use [s] to search again using.");

                        int index = currentSearchPage * 10;
                        int nbMax = Math.Min(index + 10, contactsFound.Count);

                        for (int i = index; i < nbMax; i++)
                            Console.WriteLine($"\t[{i - index}] - {Rainbow.Util.GetContactDisplayName(contactsFound[i])} - {contactsFound[i].Jid_im}");

                        keyConsole = Console.ReadKey();

                        if (keyConsole.KeyChar == 'n')
                        {
                            currentSearchPage++;
                            if (currentSearchPage * 10 > contactsFound.Count)
                                currentSearchPage = 0;
                        }
                        else if (keyConsole.KeyChar == 'p')
                        {
                            currentSearchPage--;
                            if (currentSearchPage < 0)
                                currentSearchPage = 0;
                        }
                        else if (keyConsole.KeyChar == 's')
                        {
                            appStep = APP_STEP.SEARCH_USER_TYPING;
                        }
                        else if (int.TryParse("" + keyConsole.KeyChar, out int keyValue) && keyValue < (nbMax - index) && keyValue >= 0)
                        {
                            selectedContact = contactsFound[index + keyValue];
                            askingMoreInfoAboutContact = false;
                            appStep = APP_STEP.SEARCH_USER_SELECTION_DONE;
                        }
                        break;

                    case APP_STEP.SEARCH_USER_SELECTION_DONE:
                        if (selectedContact == null)
                        {
                            appStep = APP_STEP.SEARCH_USER_TYPING;
                            break;
                        }

                        var knowContact = rbContacts.GetContactFromContactId(selectedContact.Id);
                        if ( (knowContact == null) && !askingMoreInfoAboutContact)
                        {
                            askingMoreInfoAboutContact = true;
                            Console.WriteLine($"\nAsking more details about this contact ...");
                            rbContacts.GetContactFromContactIdFromServer(selectedContact.Id, callback =>
                            {
                                if(callback.Result.Success)
                                    appStep = APP_STEP.SEARCH_USER_SELECTED_KNOWN;
                                else
                                {
                                    Console.WriteLine($"\nCannot get more details about this contact ...");
                                    appStep = APP_STEP.SEARCH_USER_TYPING;
                                }
                            });
                        }
                        else
                            appStep = APP_STEP.SEARCH_USER_SELECTED_KNOWN;

                        break;

                    case APP_STEP.SEARCH_USER_SELECTED_KNOWN:

                        Console.Clear();

                        if (selectedContact == null)
                        {
                            appStep = APP_STEP.SEARCH_USER_TYPING;
                            break;
                        }

                        int mediasUsed = 0;

                        List<Device> devicesUsed = GetDevicesToUse();

                        Console.WriteLine($"\nCurrent Contact : [{Rainbow.Util.GetContactDisplayName(currentContact)}]");
                        Console.WriteLine($"Contact selected: [{Rainbow.Util.GetContactDisplayName(selectedContact)}]");

                        if (videoInputSelected != null)
                            Console.WriteLine("\nDo You want to call this user in audio [a], in video [v] or search [s] another user ?");
                        else
                            Console.WriteLine("\nDo You want to call this user in audio [a] or search [s] another user ?");

                        keyConsole = Console.ReadKey();
                        if (keyConsole.KeyChar == 's')
                        {
                            appStep = APP_STEP.SEARCH_USER_TYPING;
                            break;
                        }
                        else if (keyConsole.KeyChar == 'a')
                        {
                            mediasUsed = Rainbow.Model.Call.Media.AUDIO;
                        }
                        else if ((keyConsole.KeyChar == 'v') && (videoInputSelected != null))
                        {
                            mediasUsed = Rainbow.Model.Call.Media.AUDIO + Rainbow.Model.Call.Media.VIDEO;
                        }

                        appStep = APP_STEP.CALL_IN_PROGRESS;

                        // Save presence for future rollback (once the call is over)
                        rbContacts.SavePresenceFromCurrentContactForRollback();

                        Dictionary<int, IMedia> mediaInputs = new Dictionary<int, IMedia>();
                        if(Rainbow.Util.MediasWithVideo(mediasUsed) && (videoInputStream != null))
                                mediaInputs.Add(Call.Media.VIDEO, videoInputStream);

                        if (Rainbow.Util.MediasWithAudio(mediasUsed) && (audioInputStream != null))
                            mediaInputs.Add(Call.Media.AUDIO, audioInputStream);

                        rbWebRTCCommunications.MakeCall(selectedContact.Id, mediaInputs, null, callback =>
                        {
                            if (callback.Result.Success)
                            {
                                currentCallId = callback.Data;
                                Console.WriteLine($"\nCall in progress - CallId:[{currentCallId}]");
                            }
                            else
                            {
                                Console.WriteLine($"\nCannot perform call: {Rainbow.Util.SerializeSdkError(callback.Result)}");
                                Console.WriteLine("\nPress a key to continue");
                                Console.ReadKey();
                                appStep = APP_STEP.SEARCH_USER_SELECTED_KNOWN;
                            }
                        });

                        break;

                    case APP_STEP.CALL_ENDED:
                        Console.WriteLine("\nCall has ended");
                        Console.WriteLine("\nPress a key to continue");
                        Console.ReadKey();

                        if (selectionMode == "P2P")
                            appStep = APP_STEP.SEARCH_USER_SELECTED_KNOWN;
                        else
                            appStep = APP_STEP.WAIT_CONFERENCE_MESSAGE;
                        break;
                }

                Thread.Sleep(10);
            }
        }

    #endregion Main

    #region Rainbow.Conferencesevents

        private static void RbConferences_ConferenceRemoved(object? sender, Rainbow.Events.IdEventArgs e)
        {
            currentConfId = null;
        }

        private static void RbConferences_ConferenceParticipantsUpdated(object? sender, Rainbow.Events.ConferenceParticipantsEventArgs e)
        {
            if (currentContact == null)
                return;

            //throw new NotImplementedException();
            if (e.Participants?.Count > 1)
            {
                isParticipating = e.Participants.ContainsKey(currentContact.Id);
                CheckIfAddVideoInConference();
            }
        }

        private static void RbConferences_ConferenceUpdated(object? sender, Rainbow.Events.ConferenceEventArgs e)
        {
            if (e.Conference.Active)
            {
                if(currentConfId != e.Conference.Id)
                {
                    // Store conference Id
                    currentConfId = e.Conference.Id;

                    // Join conference just after some delay
                    Rainbow.CancelableDelay.StartAfter(1000, () =>
                    {
                        if ((rbWebRTCCommunications != null) && !String.IsNullOrEmpty(currentConfId))
                        {
                            var devices = GetDevicesToUse();

                            Dictionary<int, IMedia> mediaInputs = new Dictionary<int, IMedia>();
                            if(audioInputStream != null)
                                mediaInputs.Add(Call.Media.AUDIO, audioInputStream);

                            rbWebRTCCommunications.JoinConference(currentConfId, mediaInputs, callbackJoinConference =>
                            {
                                currentCallId = currentConfId;
                            });
                        }
                    });
                }
            }
            else
            {
                currentConfId = null;
            }
        }       

    #endregion Rainbow.Conferencesevents

    #region Rainbow.WebRTC.Communication events

        private static void RbWebRTCCommunications_CallUpdated(object? sender, Rainbow.Events.CallEventArgs e)
        {
            if ((e != null) && (e.Call != null))
            {
                if (e.Call.Id == currentCallId)
                {
                    // Is the call no more in progress ?
                    if (!e.Call.IsInProgress())
                    {
                        currentCallId = null;
                        currentCall = null;

                        // The call is NO MORE in Progress => We Rollback presence if any
                        rbContacts?.RollbackPresenceSavedFromCurrentContact();

                        appStep = APP_STEP.CALL_ENDED;
                    }
                    else
                    {
                        currentCall = e.Call;

                        // Is the call no more in ringing state ?
                        if (!e.Call.IsRinging())
                        {
                            // The call is NOT IN RINGING STATE  => We update presence according media
                            rbContacts?.SetBusyPresenceAccordingMedias(e.Call.LocalMedias);

                            // If remote medias has changed, we have to update the display
                            if (remoteMedias != e.Call.RemoteMedias)
                            {
                                // A media has been added ?
                                if (remoteMedias < e.Call.RemoteMedias)
                                {
                                    if (Rainbow.Util.MediasWithSharing(e.Call.RemoteMedias) && !Rainbow.Util.MediasWithSharing(remoteMedias))
                                        videoRemoteDisplay = VIDEO_REMOTE_DISPLAY.SHARING;
                                    else if (Rainbow.Util.MediasWithVideo(e.Call.RemoteMedias) && !Rainbow.Util.MediasWithVideo(remoteMedias))
                                        videoRemoteDisplay = VIDEO_REMOTE_DISPLAY.VIDEO;

                                }
                                // A media has been removed
                                else
                                {
                                    if (Rainbow.Util.MediasWithSharing(e.Call.RemoteMedias))
                                        videoRemoteDisplay = VIDEO_REMOTE_DISPLAY.SHARING;
                                    else if (Rainbow.Util.MediasWithVideo(e.Call.RemoteMedias))
                                        videoRemoteDisplay = VIDEO_REMOTE_DISPLAY.VIDEO;
                                    else
                                        videoRemoteDisplay = VIDEO_REMOTE_DISPLAY.NONE;
                                }

                                // Store medias
                                remoteMedias = e.Call.RemoteMedias;
                            }

                            if (currentCall.IsConference)
                                CheckIfAddVideoInConference();
                        }
                        else
                        {
                            
                        }

                        footerLine1 = $"Call updated: Status:[{e.Call.CallStatus}] - LocalMedias:[{MediasToString(e.Call.LocalMedias)}] - RemoteMedias:[{MediasToString(e.Call.RemoteMedias)}]";
                        footerLine2 = "";

                        Console.WriteLine($"{footerLine1}\n{footerLine2}");
                    }
                }
                else
                {
                    //if (e.Call.Id == currentConfId)
                    //{
                    //    if (!e.Call.IsInProgress())
                    //    {
                    //        currentConfId = null;
                    //    }
                    //}
                }

            }
        }

    #endregion Rainbow.WebRTC.Communication events


    #region Rainbow.Application events

        private static void RbApplication_ConnectionStateChanged(object? sender, Rainbow.Events.ConnectionStateEventArgs e)
        {
            Console.WriteLine($"Connection State Changed:[{e.State}]");

            if (e.State == Rainbow.Model.ConnectionState.Disconnected)
            {
                Console.WriteLine($"We have been discconected - We quit the application");
                quitApp = true;
            }
        }

        private static void RbApplication_InitializationPerformed(object? sender, EventArgs e)
        {
            Console.WriteLine($"Initialization Performed");
            appStep = APP_STEP.LOGIN_AND_INIT_DONE;
        }

    #endregion Rainbow.Application events


    #region AUDIO / VIDEO DEVICES selection

        static Boolean SelectDevice(Boolean isVideo, Boolean isInput)
        {
            String? filePath = null;

            String logVideo = isVideo ? "Video" : "Audio";
            String logInput = isInput ? "Input" : "Ouput";

            List<Device>? devices = null;

            if (isVideo)
            {
                var list  = Devices.GetWebcamDevices();
                if (list != null)
                {
                    devices = new List<Device>();
                    foreach (var webcam in list)
                    {
                        devices.Add(webcam);
                    }
                }
            }
            else if (isInput)
                devices = Devices.GetAudioInputDevices();
            else
                devices = Devices.GetAudioOutputDevices();


            if ( (devices == null) || (devices.Count == 0) )
                Console.WriteLine($"You don't have an {logVideo} {logInput} device (Physical or emulated) ...");

            int keyValue = 0;
            while (true)
            {
                Console.WriteLine($"\nSelect {logVideo} {logInput} device:");
                Console.Write($"\n [{0}] - Don't use this kind of device ");

                int index = 1;
                if (devices != null)
                {
                    foreach (Device device in devices)
                    {
                        Console.Write($"\n [{index}] - {device.Name} ");
                        index++;
                    }
                }

                if (isInput)
                {
                    if (isVideo && (!String.IsNullOrEmpty(ApplicationInfo.VIDEO_FILE_PATH)))
                        filePath = ApplicationInfo.VIDEO_FILE_PATH;
                    else if ((!isVideo) && (!String.IsNullOrEmpty(ApplicationInfo.AUDIO_FILE_PATH)))
                        filePath = ApplicationInfo.AUDIO_FILE_PATH;
                    if (filePath != null)
                        Console.Write($"\n [f] - {logVideo}: [{filePath}]");
                }

                Console.WriteLine("\n");
                Console.Out.Flush();

                var keyConsole = Console.ReadKey();

                if ((keyConsole.KeyChar == 'f') && (filePath != null))
                {
                    String fileName = Path.GetFileName(filePath);
                    if (isVideo)
                    {
                        videoInputSelected = new InputStreamDevice(fileName, fileName, filePath, true, audioInputSelected?.Path == filePath, true);
                        videoInputStream = new MediaInput(videoInputSelected);
                        if (!videoInputStream.Init())
                        {
                            Console.WriteLine($"\n\n\t => {logVideo} {logInput} file/uri selected:[{filePath}] => Cannot init the stream ....");
                            return false;
                        }
                        Console.WriteLine($"\n\n\t => {logVideo} {logInput} file/uri selected:[{filePath}]");

                        if (audioInputSelected?.Path == filePath)
                        {
                            if (audioInputStream != null)
                                audioInputStream.Dispose();
                            audioInputStream = videoInputStream;
                        }
                    }
                    else
                    {
                        audioInputSelected = new InputStreamDevice(fileName, fileName, filePath, false, true, true);
                        audioInputStream = new MediaInput(audioInputSelected);
                        if (!audioInputStream.Init())
                        {
                            Console.WriteLine($"\n\n\t => {logVideo} {logInput} file/uri selected:[{filePath}] => Cannot init the stream ....");
                            return false;
                        }
                        Console.WriteLine($"\n\n\t => {logVideo} {logInput} file/uri selected:[{filePath}]");
                    }
                    return true;
                }
                else if (int.TryParse("" + keyConsole.KeyChar, out keyValue) && keyValue < index && keyValue >= 0)
                {
                    break;
                }
            }

            Device ? deviceSelected = null;

            if ( (keyValue != 0) && (devices != null) )
            {
                deviceSelected = devices[keyValue - 1];
                Console.WriteLine($"\n\n\t => {logVideo} {logInput} device selected:[{deviceSelected.Name}]");
            }
            else
                Console.WriteLine($"\n\n\t => {logVideo} {logInput} device selected:[NONE]");

            if (isVideo)
                videoInputSelected = deviceSelected;
            else if (isInput)
                audioInputSelected = deviceSelected;
            else
                audioOutputSelected = deviceSelected;

            return true;

        }

        static Boolean SelectAudioInputDevice()
        {
            return SelectDevice(false, true); ;
        }

        static Boolean SelectAudioOutputDevice()
        {
            return SelectDevice(false, false);
        }

        static Boolean SelectVideoInputDevice()
        {
            return SelectDevice(true, true);
        }

    #endregion AUDIO / VIDEO DEVICES selection

        static void CheckIfAddVideoInConference()
        {
            if ((currentCall == null) || (rbWebRTCCommunications == null))
                return;

            if ( (!currentCall.IsConference) || (currentCall.CallStatus != Call.Status.ACTIVE) )
                return;

            if (!Rainbow.Util.MediasWithVideo(currentCall.LocalMedias))
            {
                Rainbow.CancelableDelay.StartAfter(1000, () =>
                {
                    if (videoInputSelected != null)
                    {
                        var video = new MediaInput(videoInputSelected);
                        if ( (!Rainbow.Util.MediasWithVideo(currentCall.LocalMedias))  && (video.Init()) )
                            rbWebRTCCommunications.AddVideo(currentCall.Id, video);
                    
                    }
                });
            }

        }
        static List<Device> GetDevicesToUse()
        {
            List<Device> devicesUsed = new List<Device>();

            if (audioInputSelected != null)
            {
                Console.WriteLine($"\nAudio input device selected: [{audioInputSelected.Name}]");
                devicesUsed.Add(audioInputSelected);
            }

            if (audioOutputSelected != null)
            {
                Console.WriteLine($"\nAudio output device selected: [{audioOutputSelected.Name}]");
                devicesUsed.Add(audioOutputSelected);
            }

            if (videoInputSelected != null)
            {
                Console.WriteLine($"\nVideo input device selected: [{videoInputSelected.Name}]");
                devicesUsed.Add(videoInputSelected);
            }
            return devicesUsed;
        }

        // Check if APP_ID, APP_SECRET_KEY, HOST_NAME, LOGIN_USER and LOGIN_PASSWORD are set in file "ApplicationInfo.cs"
        static Boolean CheckApplicationInfo()
        {
            Boolean result = true;

            if (!((ApplicationInfo.APP_ID?.Length > 0) && (ApplicationInfo.APP_ID != ApplicationInfo.DEFAULT_VALUE)))
            {
                Console.WriteLine("\nYou need to specify a valid APP_ID");
                result = false;
            }

            if (!((ApplicationInfo.APP_SECRET_KEY?.Length > 0) && (ApplicationInfo.APP_SECRET_KEY != ApplicationInfo.DEFAULT_VALUE)))
            {
                Console.WriteLine("\nYou need to specify a valid APP_SECRET_KEY");
                result = false;
            }

            if (!((ApplicationInfo.HOST_NAME?.Length > 0) && (ApplicationInfo.HOST_NAME != ApplicationInfo.DEFAULT_VALUE)))
            {
                Console.WriteLine("\nYou need to specify a valid HOST_NAME");
                result = false;
            }

            if (!((ApplicationInfo.LOGIN_USER?.Length > 0) && (ApplicationInfo.LOGIN_USER != ApplicationInfo.DEFAULT_VALUE)))
            {
                Console.WriteLine("\nYou need to specify a valid LOGIN_USER");
                result = false;
            }

            if (!((ApplicationInfo.PASSWORD_USER?.Length > 0) && (ApplicationInfo.PASSWORD_USER != ApplicationInfo.DEFAULT_VALUE)))
            {
                Console.WriteLine("\nYou need to specify a valid PASSWORD_USER");
                result = false;
            }

            if (ApplicationInfo.AUDIO_FILE_PATH?.Length > 0)
            {
                if (!File.Exists(ApplicationInfo.AUDIO_FILE_PATH))
                {
                    if (!Uri.TryCreate(ApplicationInfo.AUDIO_FILE_PATH, UriKind.Absolute, out Uri? uriResult))
                    {
                        Console.WriteLine($"\nYou specified an AUDIO_FILE_PATH but the file has not been found - path:[{ApplicationInfo.AUDIO_FILE_PATH}]");
                        result = false;
                    }
                }
            }

            if ((ApplicationInfo.VIDEO_FILE_PATH?.Length > 0) && (!File.Exists(ApplicationInfo.VIDEO_FILE_PATH)))
            {
                if (!File.Exists(ApplicationInfo.VIDEO_FILE_PATH))
                {
                    if (!Uri.TryCreate(ApplicationInfo.VIDEO_FILE_PATH, UriKind.Absolute, out Uri? uriResult))
                    {
                        Console.WriteLine($"\nYou specified an VIDEO_FILE_PATH but the file has not been found - path:[{ApplicationInfo.VIDEO_FILE_PATH}]");
                        result = false;
                    }
                }
            }

            return result;
        }

        // Check if folder where FFmpeg libraries are stored exists
        static Boolean CheckFFmpegLibFolderPath()
        {
            Boolean result = Directory.Exists(ApplicationInfo.FFMPEG_LIB_FOLDER_PATH);
            if (!result)
                Console.WriteLine($"\nThe folder where FFmpeg libraries must be stored doesn't exist:[{ApplicationInfo.FFMPEG_LIB_FOLDER_PATH}]");
            return result;
        }

        static Boolean InitLogsWithNLog()
        {
            if (!File.Exists(ApplicationInfo.NLOG_CONFIG_FILE_PATH))
            {
                Console.WriteLine("\nCannot file defined by ApplicationInfo.NLOG_CONFIG_FILE_PATH");
                return false;
            }

            try
            {
                // Get content of the log file configuration
                String logConfigContent = File.ReadAllText(ApplicationInfo.NLOG_CONFIG_FILE_PATH, System.Text.Encoding.UTF8);

                // Create NLog configuration using XML file content
                XmlLoggingConfiguration config = XmlLoggingConfiguration.CreateFromXmlString(logConfigContent);
                if (config.InitializeSucceeded == true)
                {
                    // Set NLog configuration
                    NLog.LogManager.Configuration = config;

                    // Create Logger factory
                    var factory = new NLog.Extensions.Logging.NLogLoggerFactory();

                    // Set Logger factory to Rainbow SDK
                    Rainbow.LogFactory.Set(factory);

                    return true;
                }
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception:[{Rainbow.Util.SerializeException(ex)}]");
            }

            return false;
        }

        static String MediasToString(int medias)
        {
            if (medias == Call.Media.AUDIO)
                return "Audio";

            if (medias == Call.Media.AUDIO + Call.Media.VIDEO)
                return "Audio+Video";

            if (medias == Call.Media.AUDIO + Call.Media.SHARING)
                return "Audio+Sharing";

            if (medias == Call.Media.VIDEO)
                return "Video";

            if (medias == Call.Media.SHARING)
                return "Sharing";

            if (medias == Call.Media.VIDEO + Call.Media.SHARING)
                return "Video+Sharing";

            if (medias == Call.Media.AUDIO + Call.Media.VIDEO + Call.Media.SHARING)
                return "Audio+Video+Sharing";

            return "";
        }

    }
}

