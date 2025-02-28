
// Indicates configuration elements we need to use in this example
using EmbedIO;

Configuration.NeedsRainbowServerConfiguration = true;
Configuration.NeedsRainbowAccounts = true;

// Initialize the Configuration i.e. reads content of the file "config.json" defined in the "Resources" folder or "TerminalAppLibrary" project
if (!Configuration.Initialize())
    return;

// Check if a S2SCallbackURL has been defined
if (String.IsNullOrEmpty(Configuration.ExeSettings.S2SCallbackURL))
{
    Console.WriteLine("There is no S2SCallbackURL defined in config.json file ...");
    return;
}

// We want to log S2S specfic entries in a log file
NLogConfigurator.AddLogger("S2S");
var webServer = CreateWebServer("http://localhost:9870", Configuration.ExeSettings.S2SCallbackURL);
var _ = webServer.RunAsync();


// Specify the "BotViewFactory" to use in "BotWindow"
BotWindow.BotViewFactory = new BotViewFactory();

// Use "BotWindow" as main window for Terminal.Gui.Application
Terminal.Gui.Application.Run<BotWindow>().Dispose();

// Before the application exits, reset Terminal.Gui for clean shutdown
Terminal.Gui.Application.Shutdown();

webServer.DisposeSafely();


WebServer CreateWebServer(string url, String callbackUrl)
{
    Uri uri = new Uri(callbackUrl);
    String callbackAbsolutePath = uri.AbsolutePath;

    WebServer server = new WebServer(o => o
            .WithUrlPrefix(url)
            .WithMode(HttpListenerMode.EmbedIO)
            )

        // First, we will configure our web server by adding Modules.
        .WithLocalSessionManager()
        .WithModule(new CallbackWebModule(callbackAbsolutePath));

    server.WithStaticFolder("/", "./Resources", true, configure =>
    {

    });

    return server;
}