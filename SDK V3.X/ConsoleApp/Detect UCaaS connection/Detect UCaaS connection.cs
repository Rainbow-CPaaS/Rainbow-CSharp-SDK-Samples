
using Microsoft.Extensions.Logging;
using Rainbow;
using Rainbow.Consts;
using Rainbow.Model;
using System.Text;

Object consoleLockObject = new(); // To lock until the current console display is performed

Console.OutputEncoding = Encoding.UTF8; // We want to display UTF8 on the console
Boolean canUseEmoji = true;             // Set to false if Emoji cannot be displayed in your console

List<RainbowBot> rainbowBots = new ();


Configuration configuration = Configuration.Instance;
if (!configuration.Initialized)
{
    WriteRed(configuration.ErrorMessage);
    return;
}

var logFullPath = Path.GetFullPath(configuration.LogsFolderPath);
WriteBlue($"Logs files will be stored in folder:[{logFullPath}]{Rainbow.Util.CR}");

WriteBlue($"Use [ESC] at anytime to quit{Rainbow.Util.CR}");

foreach (var account in configuration.RbAccounts)
{
    var rainbowBot = new RainbowBot(account);
    rainbowBots.Add(rainbowBot);

    // Set events we want to follow 
    rainbowBot.ConnectionStateChanged += (connectionState) => RainbowBot_ConnectionStateChanged(rainbowBot, connectionState);
    rainbowBot.AccountUsedOnAnotherDevice += (accountUsedOnAnotherDevice) => RainbowBot_AccountUsedOnAnotherDevice(rainbowBot, accountUsedOnAnotherDevice);
    rainbowBot.ConnectionFailed += (sdkError) => RainbowBot_ConnectionFailed(rainbowBot, sdkError);
}

do
{
    
    CheckInputKey();
    await Task.Delay(200);
} while (true);


void CheckInputKey()
{
    while (Console.KeyAvailable)
    {
        var userInput = Console.ReadKey(true);

        switch (userInput.Key)
        {
            case ConsoleKey.Escape:
                WriteYellow($"Asked to end process using [ESC] key");
                System.Environment.Exit(0);
                return;
        }
    }
}


#region EVENTS TRIGGERED BY RainbowBot

void RainbowBot_ConnectionStateChanged(RainbowBot rainbowBot, ConnectionState connectionState)
{
    switch(connectionState.Status)
    {
        case ConnectionStatus.Connected:
            WriteDarkYellow($"Bot using [{rainbowBot.RainbowAccount.Login}] is connected{Rainbow.Util.CR}");
            break;

        case ConnectionStatus.Connecting:
            WriteBlue($"Bot using [{rainbowBot.RainbowAccount.Login}] is connecting ...{Rainbow.Util.CR}");
            break;

        case ConnectionStatus.Disconnected:
            WriteRed ($"Bot using [{rainbowBot.RainbowAccount.Login}] is disconnected{Rainbow.Util.CR}");
            break;
    }
}

void RainbowBot_AccountUsedOnAnotherDevice(RainbowBot rainbowBot, Boolean accountUsedOnAnotherDevice)
{
    if(accountUsedOnAnotherDevice)
        WriteDarkYellow($"[{rainbowBot.RainbowAccount.Login}] is connected using another device{Rainbow.Util.CR}");
    else
        WriteRed($"[{rainbowBot.RainbowAccount.Login}] is NOT connected using another device{Rainbow.Util.CR}");
}

void RainbowBot_ConnectionFailed(RainbowBot rainbowBot, SdkError sdkError)
{
    WriteRed($"Bot using [{rainbowBot.RainbowAccount.Login}] is not connected - Error:[{sdkError}]{Rainbow.Util.CR}");
}

#endregion EVENTS TRIGGERED BY RainbowBot


#region UTILITY METHODS

void WriteYellow(String message)
{
    WriteToConsole(message, ConsoleColor.Yellow);
}

void WriteDarkYellow(String message)
{
    WriteToConsole(message, ConsoleColor.DarkYellow);
}

void WriteWhite(String message)
{
    WriteToConsole(message, ConsoleColor.White);
}

void WriteRed(String message)
{
    WriteToConsole(message, ConsoleColor.Red);
}

void WriteGreen(String message)
{
    WriteToConsole(message, ConsoleColor.Green);
}

void WriteBlue(String message)
{
    WriteToConsole(message, ConsoleColor.Blue);
}

void WriteToConsole(String message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
{
    if (String.IsNullOrEmpty(message))
        return;

    lock (consoleLockObject)
    {
        if (foregroundColor != null)
            Console.ForegroundColor = foregroundColor.Value;

        if (backgroundColor != null)
            Console.BackgroundColor = backgroundColor.Value;

        Console.WriteLine(message);
        Console.ResetColor();
    }
}
#endregion UTILITY METHODS
