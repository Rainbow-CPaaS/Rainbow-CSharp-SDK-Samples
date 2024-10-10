// Indicates configuration elements we need to use in this example
using Terminal.Gui;

Configuration.NeedsRainbowServerConfiguration = true;
Configuration.NeedsRainbowAccounts = true;

// Initialize the Configuration i.e. reads content of the file "config.json" defined in the "Resources" folder or "TerminalAppLibrary" project
if (!Configuration.Initialize())
{
    Console.WriteLine(Configuration.GetErrorMessage());
    return;
}

// Specify the "BotViewFactory" to use in "BotWindow"
BotWindow.BotViewFactory = new BotViewFactory();


// Use "BotWindow" as main window for Terminal.Gui.Application
Terminal.Gui.Application.Run<BotWindow>().Dispose();

//// Before the application exits, reset Terminal.Gui for clean shutdown
Terminal.Gui.Application.Shutdown();

public class TestWindow : Window
{
    public TestWindow()
    {

        Title = "TestWindow";
        Width = Dim.Fill();
        Height = Dim.Fill();

        var testView = new TestView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        Add(testView);
    }
}

public class TestView : View
{
    public TestView()
    {

        Title = $"TestView";
        CanFocus = true;

        // Create Login and Password Label
        var loginLabel = new Label
        {
            Text = "Login:",
            TextAlignment = Alignment.End,
            Width = 9
        };
        var passwordLabel = new Label
        {
            Text = "Password:",
            TextAlignment = Alignment.End,
            X = Pos.Left(loginLabel),
            Y = Pos.Bottom(loginLabel) + 1,
            Width = 9
        };

        // Create Login and Password TextField
        var loginText = new TextField
        {
            // Position text field adjacent to the label
            X = Pos.Right(passwordLabel) + 1,

            // Fill remaining horizontal space
            Width = Dim.Fill(1),

            Text = "login@test.com",
            TextAlignment = Alignment.End
        };
        var passwordText = new TextField
        {
            Secret = true,

            // align with the text box above
            X = Pos.Left(loginText),
            Y = Pos.Top(passwordLabel),
            Width = Dim.Fill(1),

            Text = "my incredible pwd"
        };

        // Create login button
        var btnLogin = new Button
        {
            Text = "Login",
            Y = Pos.Bottom(passwordLabel) + 1,

            // center the login button horizontally
            X = Pos.Center(),
            IsDefault = true,
        };

        // Add elements to this View
        Add(loginLabel, loginText, passwordLabel, passwordText, btnLogin);
    }
}

