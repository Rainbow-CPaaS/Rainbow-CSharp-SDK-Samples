using System;
using System.Collections.Generic;
using System.IO;
using Rainbow.Medias;

namespace BotVideoCompositor
{

    internal static class RainbowApplicationInfo
    {
        static internal readonly string DEFAULT_VALUE = "TO DEFINE";  // /!\ DON'T MODIFY THIS

        static internal String ffmpegLibFolderPath = DEFAULT_VALUE;
        static internal List<String>? videosUri = null;
        static internal String labels = DEFAULT_VALUE;


        // NLog configuration file used to log SDK debug information
        static internal readonly String NLOG_CONFIG_FILE_PATH = $".{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}NLogConfiguration.xml";

        static internal List<BotManager>? botManagers = null;
        
        static internal String commandStop = DEFAULT_VALUE;
        static internal String commandStart = DEFAULT_VALUE;

        static internal String appId = DEFAULT_VALUE;
        static internal String appSecret = DEFAULT_VALUE;
        static internal String hostname = DEFAULT_VALUE;

        static internal Account? account = null;

        static internal BroadcastConfiguration BroadcastConfiguration = new BroadcastConfiguration();
        static internal Dictionary<String, String>? outputs = null;
        static internal Dictionary<String, String>? vignettes = null;
        static internal List<int>? fps = null;

        static internal Dictionary<String, String>? mosaicDisplay = null;
        static internal Dictionary<String, String>? overlayDisplay = null;
    }

    internal class Account
    {
        public String Login;
        public String Password;

        public Account(String login, String password)
        {
            Login = login;
            Password = password;
        }
    }

    public class Size
    {
        public int Width;
        public int Height;

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Size(String? s)
        {
            if (s == null)
                throw new ArgumentNullException("s");

            int index = -1;

            if (s.Contains("x", StringComparison.InvariantCultureIgnoreCase))
                index = s.IndexOf("x", StringComparison.InvariantCultureIgnoreCase);
            else if (s.Contains(":", StringComparison.InvariantCultureIgnoreCase))
                index = s.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);

            if(index < 1)
                throw new ArgumentException("s");

            
            string w = s.Substring(0, index);
            string h = s.Substring(index + 1);

            Width = int.Parse(w);
            Height = int.Parse(h);
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(Boolean asFormat)
        {
            String str;
            if (asFormat)
                str = $"[{Width}:{Height}]";
            else
                str = $"{Width}x{Height}";

            return str;
        }
    }

    public class Item
    {
        public string title;
        public string value;

        public Item(string t, string v)
        {
            title = t;
            value = v;
        }

        public Item(int t, int v)
        {
            title = t.ToString();
            value = v.ToString();
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
