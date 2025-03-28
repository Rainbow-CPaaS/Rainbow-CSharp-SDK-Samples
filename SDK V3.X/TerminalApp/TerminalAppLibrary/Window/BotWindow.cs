using Rainbow.Example.Common;
using System.Drawing;
using System.Globalization;
using Terminal.Gui;
using RuntimeEnvironment = Microsoft.DotNet.PlatformAbstractions.RuntimeEnvironment;

// Defines a top-level window with border and title
public class BotWindow : Window
{
    private View? leftView = null;
    private View? rightView = null;

    public static IBotViewFactory? BotViewFactory { get; set; }

    public static Point MousePosition { get; set; } // To get mouse position in all windows / views

    private readonly Shortcut? ShVersion;

    private readonly List<View> botViewsAvailable;

    public BotWindow()
    {
        if(BotViewFactory == null)
            throw new ArgumentNullException(nameof(BotViewFactory));

        try
        {
            if(!String.IsNullOrEmpty(Configuration.ExeSettings.CultureInfo))
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(Configuration.ExeSettings.CultureInfo);
        }
        catch { }

        //Title = $"Rainbow Terminal App";
        BorderStyle = LineStyle.None;
        
        botViewsAvailable = [];


        // We need at least one account
        if ((Configuration.Credentials.UsersConfig == null) || (Configuration.Credentials.UsersConfig.Count == 0))
            return;

        int nbAccounts = Configuration.Credentials.UsersConfig.Count;

        // We display up to two Accounts in same time if specified
        if (Configuration.UseSplittedView && nbAccounts > 1)
        {
            leftView = BotViewFactory.CreateBotView(Configuration.Credentials.UsersConfig[0]);
            SetOnLeft(leftView);
            botViewsAvailable.Add(leftView);
            Add(leftView);

            rightView = BotViewFactory.CreateBotView(Configuration.Credentials.UsersConfig[1]);
            SetOnRight(rightView);
            botViewsAvailable.Add(rightView);
            Add(rightView);
        }
        else
        {
            leftView = BotViewFactory.CreateBotView(Configuration.Credentials.UsersConfig[0]);
            botViewsAvailable.Add(leftView);
            Add(leftView);
        }

        // Create StatusBar
        StatusBar statusBar = new()
        {
            Visible = true,
            AlignmentModes = AlignmentModes.IgnoreFirstOrLast,
            CanFocus = false
        };


        var statusBarShortcutHide = new Shortcut
        {
            Key = Key.F10,
            Title = "Show/Hide Status Bar",
            CanFocus = false,
        };
        statusBarShortcutHide.Accepting += (sender, e) => { statusBar.Visible = !statusBar.Visible; };

        ShVersion = new()
        {
            Title = "Version Info",
            CanFocus = false
        };

        var quitBarShortcut = new Shortcut
        {
            CanFocus = false,
            Title = "Quit",
            Key = Application.QuitKey
        };

        if (Configuration.UseSplittedView && nbAccounts > 2)
        {
            var statusBarShortcutBotSelectionOnLeft = new Shortcut
            {
                Key = Key.F1,
                Title = "Select Bot on Left Panel",
                CanFocus = false,
            };
            statusBarShortcutBotSelectionOnLeft.Accepting += (sender, e) => { SelectBotOnPanel(true); };

            var statusBarShortcutBotSelectionOnRight = new Shortcut
            {
                Key = Key.F2,
                Title = "Select Bot on Right Panel",
                CanFocus = false,
            };
            statusBarShortcutBotSelectionOnRight.Accepting += (sender, e) => { SelectBotOnPanel(false); };

            statusBar.Add(quitBarShortcut,
                    statusBarShortcutBotSelectionOnLeft,
                    statusBarShortcutBotSelectionOnRight
                    );
        }
        else if ( (!Configuration.UseSplittedView) && nbAccounts > 1)
        {
            var statusBarShortcutBotSelection = new Shortcut
            {
                Key = Key.F1,
                Title = "Select Bot",
                CanFocus = false,
            };
            statusBarShortcutBotSelection.Accepting += (sender, e) => { SelectBotOnPanel(null); };

            statusBar.Add(quitBarShortcut,
                    statusBarShortcutBotSelection);
        }
        else
        {
            statusBar.Add(quitBarShortcut);
        }

        if(statusBarShortcutHide != null)
            statusBar.Add(statusBarShortcutHide);

        statusBar.Add(ShVersion); // always add it as the last one
        Add(statusBar);

        // Need to manage Loaded event
        Loaded += LoadedHandler;

        Terminal.Gui.Application.MouseEvent += ApplicationMouseEvent;
    }

    private void ApplicationMouseEvent(object? sender, MouseEvent a) { BotWindow.MousePosition = a.Position; }

    private static void SetOnLeft(View leftView)
    {
        leftView.X = 0;
        leftView.Width = Dim.Percent(50);
    }

    private static void SetOnRight(View rightView)
    {
        rightView.X = Pos.Percent(50);
        rightView.Width = Dim.Percent(50);
    }

    private void SelectBotOnPanel(Boolean? onLeft)
    {
        Application.Invoke(() =>
        {
            if (BotViewFactory == null) return;

            // Create buttons
            String buttonClicked = "";
            List<Button> buttons = [];
            List<String> buttonsName = ["Ok", "Cancel"];
            foreach (var name in buttonsName)
            {
                Button button = new() { Text = name, IsDefault = (name == "Ok"), ShadowStyle = ShadowStyle.None };
                button.MouseClick += (s, e) =>
                {
                    e.MouseEvent.Handled = true;
                    buttonClicked = name;
                    Application.RequestStop();
                };
                buttons.Add(button);
            }

            String msg = (onLeft == null) ? "" : ((onLeft.Value) ? " on LEFT" : " on RIGHT");
            String dialogTitle = $"Select Bot to display{msg}";

            Dialog dialog = new()
            {
                Title = dialogTitle,
                ButtonAlignment = Alignment.Center,
                Buttons = [.. buttons],

                X = Pos.Center(),
                Y = Pos.Center(),
                Width = Dim.Percent(50),
                Height = Dim.Percent(50),
                ShadowStyle = ShadowStyle.None
            };

            // /!\ It's not possible to display same BotView multiple time
            Dictionary<int, String> botsList = [];
            Boolean selectedByDefault;
            int selectedIndex = 0;
            int index = 0;

            UserConfig? leftRainbowAccount = BotViewFactory.GetRainbowAccountFromBotView(leftView);
            UserConfig? rightRainbowAccount = BotViewFactory.GetRainbowAccountFromBotView(rightView);

            foreach (var account in Configuration.Credentials.UsersConfig)
            {
                if ( (onLeft == null) || onLeft.Value)
                {
                    selectedByDefault = (leftRainbowAccount?.Prefix == account.Prefix);
                    if (rightRainbowAccount?.Prefix == account.Prefix)
                        continue;
                }
                else
                {
                    selectedByDefault = (rightRainbowAccount?.Prefix == account.Prefix);
                    if (leftRainbowAccount?.Prefix == account.Prefix)
                        continue;
                }

                if (selectedByDefault)
                    selectedIndex = index;

                botsList.Add(index, account.Prefix);
                index++;
            }

            RadioGroup radioGroup = new()
            {
                Title = dialogTitle,
                X = Pos.Center(),
                Y = Pos.Center(),
                RadioLabels = [.. botsList.Values],
                SelectedItem = selectedIndex
            };
            dialog.Add(radioGroup);

            Application.Run(dialog);
            dialog.Dispose();

            var itemSelected = radioGroup.SelectedItem;
            var botNameSelected = botsList[itemSelected];

            // Do nothing if we have selected the one currently used
            if  ( (!buttonClicked.Equals("Ok", StringComparison.InvariantCultureIgnoreCase)) 
                    || (itemSelected == selectedIndex) )
                return;

            View? viewToUpdate = null;
            // Check if we have already the BotView according selection
            foreach (var botView in botViewsAvailable)
            {
                UserConfig? rbAccount = BotViewFactory.GetRainbowAccountFromBotView(botView);
                if (botNameSelected == rbAccount?.Prefix)
                {
                    viewToUpdate = botView;
                    break;
                }
            }
            if (viewToUpdate == null)
            {
                foreach(var rbAccount in Configuration.Credentials.UsersConfig)
                {
                    if (rbAccount.Prefix == botNameSelected)
                    {
                        viewToUpdate = BotViewFactory.CreateBotView(rbAccount);
                        botViewsAvailable.Add(viewToUpdate);
                        break;
                    }
                }
            }

            if (viewToUpdate == null)
            {
                // /!\ WE HAVE A PB HERE ! 
                return;
            }

            View? botViewToRemove = ( onLeft == null || onLeft.Value) ? leftView : rightView;
            if ((onLeft == null) || onLeft.Value)
            {
                if (onLeft != null) 
                    SetOnLeft(viewToUpdate);
                botViewToRemove = leftView;
                leftView = viewToUpdate;
            }
            else
            {
                SetOnRight(viewToUpdate);
                botViewToRemove = rightView;
                rightView = viewToUpdate;
            }
            if (botViewToRemove != null)
                Remove(botViewToRemove);

            Add(viewToUpdate);
        });
    }

    private void LoadedHandler(object? sender, EventArgs? args)
    {
        if (ShVersion is { })
        {
            ShVersion.Title = $"{RuntimeEnvironment.OperatingSystem} {RuntimeEnvironment.OperatingSystemVersion}, {Driver.GetVersionInfo()}";
        }
    }
}