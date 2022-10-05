using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace BotVideoOrchestratorAndBroadcaster
{
    internal class BotVideoBroadcasterInfo
    {
        private RainbowBotVideoBroadcaster botVideoBroadcaster;
        private BackgroundWorker? backgroundWorker = null;
        private BotManager? _botManager = null;

        private String _stopMessage = "";

        public string Login; // email address of the rainbow account used to log in

        public string Pwd;  // pwd of the rainbow account used to log in

        public string Id; // Id of the rainbow account used to log in

        public string Name; // Name (i;e. first + last name to use

        public string Uri; // Uri of the stream to use (distant or local)

        public bool Selected; // To know if this bot must join the conference

        public string SharingUri; // Uri of the stream to use in sharing stream (distant or local)

        public BotVideoBroadcasterInfo(string login, string pwd)
        {
            Login = login;
            Pwd = pwd;
            Id = "";
            Name = "";
            Uri = "";
            Selected = false;
            SharingUri = "";

            botVideoBroadcaster = new RainbowBotVideoBroadcaster();
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            RainbowBotVideoBroadcaster.State state;
            Boolean canContinue = true;
            String message;

            List<BotManager>? botManagers = null; ;
            if (_botManager != null)
                botManagers = new List<BotManager>() { _botManager };

            if (!botVideoBroadcaster.Configure(
                    RainbowApplicationInfo.appId, RainbowApplicationInfo.appSecret,
                    RainbowApplicationInfo.hostname,
                    Login, Pwd,
                    Name,
                    botManagers,
                    _stopMessage,
                    RainbowBotVideoBroadcaster.INVALID_BUBBLE_ID,
                    null,
                    null,
                    "./",
                    Name + ".ini"
                    ))
            {
                Util.WriteErrorToConsole($"[{Name} Cannot configure this bot - check 'config.json' file !");
                return;
            }

            if (!botVideoBroadcaster.StartLogin())
            {
                Util.WriteErrorToConsole($"[{Name} Cannot start login with this bot - check 'config.json' file !");
                return;
            }

            // We loop until we cannot continue
            while (canContinue)
            {
                Thread.Sleep(20);

                (state, canContinue, message) = CheckBotStatus();

                // Get ID of the RB contact used by this bot
                if(canContinue && String.IsNullOrEmpty(Id))
                {
                    var contact = botVideoBroadcaster?.GetCurrentContact();
                    if (contact != null)
                    {
                        Id = contact.Id;
                        Util.WriteDebugToConsole($"[{RainbowApplicationInfo.labelOrchestrator}] We get id[{Id}] of the bot[{Name}]");
                    }
                }
            }
        }

        public void Configure(string stopMessage, BotManager botManager)
        {
            _stopMessage = stopMessage;
            _botManager = botManager;

            if (backgroundWorker == null)
            {
                backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += BackgroundWorker_DoWork;
                backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
                backgroundWorker.WorkerSupportsCancellation = true;
            }

            if (!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }

        public (RainbowBotVideoBroadcaster.State state, Boolean canContinue, String message) CheckBotStatus()
        {
            Boolean canContinue = true;
            String message = "";

            // Get the curent state of the bot and the trigger used to reach it
            (RainbowBotVideoBroadcaster.State state, RainbowBotVideoBroadcaster.Trigger trigger) = botVideoBroadcaster.GetStateAndTrigger();

            // Two states are important here:
            //  - NotConnected: the bot is not connected (or no more connected), we need to add logic according uour needs
            //  - Created: the bot is not connected because credentials are not correct
            // All other states are managed in the bot logic itself

            // Check if bot is in NotConnected state
            if (state == RainbowBotVideoBroadcaster.State.NotConnected)
            {
                switch (trigger)
                {
                    case RainbowBotVideoBroadcaster.Trigger.Disconnect:
                        // The bot has received the "stop message" from the bot master.
                        message = $"The bot [{botVideoBroadcaster.BotName}] has received the \"STOP message\" from the bot master...";
                        canContinue = false;
                        break;

                    case RainbowBotVideoBroadcaster.Trigger.ServerNotReachable:
                        // The server has not been reached.
                        // Need to add logic to try again OR something else ?
                        message = $"The bot [{botVideoBroadcaster.BotName}] cannot reach the server ...";
                        canContinue = false;
                        break;

                    case RainbowBotVideoBroadcaster.Trigger.TooManyAttempts:
                        // Bot was logged at least once but since we can reach the server after several attempts
                        // Need to add logic to try again OR something else ?
                        message = $"The bot [{botVideoBroadcaster.BotName}] was connected but after several attempts it can't reach the server anymore...";
                        canContinue = false;
                        break;
                }
            }
            else if (state == RainbowBotVideoBroadcaster.State.Created)
            {
                switch (trigger)
                {
                    case RainbowBotVideoBroadcaster.Trigger.IncorrectCredentials:
                        message = $"The bot [{botVideoBroadcaster.BotName}] is not connected because the credentials are not correct ...";
                        canContinue = false;
                        break;
                }
            }

            return (state, canContinue, message);
        }
    }

    internal class VideoInfo
    {
        public string Title;

        public string Uri;

        public VideoInfo(string title, string uri)
        {
            Title = title;
            Uri = uri;
        }

        public VideoInfo(string title)
        {
            Title = Uri = title;
        }
    }


    public class BotManager
    {
        public String Email;

        public String? Id;

        public String? Jid;

        public BotManager(string email, string? id = null, string? jid = null)
        {
            Email = email;
            Id = id;
            Jid = jid;
        }
    }
}
