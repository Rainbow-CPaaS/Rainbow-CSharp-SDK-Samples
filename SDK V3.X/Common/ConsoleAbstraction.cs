using System;
using System.Globalization;
using System.IO;
using System.Text;

public static class ConsoleAbstraction
{
    private static readonly Object consoleLockObject = new();
    private static readonly TextReader _consoleIn = Console.In;

    private static bool _isInitialized = false;
    private static bool _colorsNotManaged = false;
    private static bool _keyAvailable = false;

    /// <summary>
    /// To check if the current console allow use of "Console.KeyAvailable".
    /// 
    /// Default VS Code Debug Console doesn't allow to use "Console.KeyAvailable" and "Console.ReadKey" without using ENTER/RETURN key, so we need to check it before using it.
    /// </summary>
    public static void Init(Boolean colorsManaged = true)
    {
        lock (consoleLockObject)
        {
            if (_isInitialized) return;
            _isInitialized = true;

            Console.OutputEncoding = Encoding.UTF8; // We want to display UTF8 on the console

            _colorsNotManaged = !colorsManaged;
            try
            {
                var _ = Console.KeyAvailable;
                _keyAvailable = true;
            }
            catch
            {
                _keyAvailable = false;

                ConsoleAbstraction.WriteRed($"/!\\ Your console don't allow to manage input key WITHOUT using ENTER/RETURN.{CR}");
            }
        }
    }

    public static String Title
    {
        get => Console.Title;
        set => Console.Title = value;
    }

    /// <summary>
    /// Abstract "Console.KeyAvailable" to be compatible even in VS Code Debug console
    /// </summary>
    public static Boolean KeyAvailable
    {
        get
        {
            if (_keyAvailable)
            {
                return Console.KeyAvailable;
            }
            else
            {
                return (_consoleIn.Peek() != -1);
            }
        }
    }

    private static ConsoleKeyInfo? CharToConsoleKeyInfo(char? c)
    {
        if (c is not null)
        {
            ConsoleKey consoleKey;
            if (Enum.TryParse(c.Value.ToString(CultureInfo.InvariantCulture), true, out consoleKey))
            {
                return new ConsoleKeyInfo(c.Value, consoleKey, false, false, false);
            }
            else if (c.Value == ' ')
                return new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false);
        }
        return (ConsoleKeyInfo?)null;
    }

    /// <summary>
    /// Abstract "Console.ReadKey" to be compatible even in VS Code Debug console
    /// </summary>
    public static ConsoleKeyInfo? ReadKey()
    {
        Init();
        if (_keyAvailable)
        {
            return Console.ReadKey(true);
        }
        else
        {
            var input = _consoleIn.ReadLine();
            return CharToConsoleKeyInfo((input?.Length > 0 ? input[0] : null));
        }
    }

    public static async Task<ConsoleKeyInfo?> ReadKeyAsync()
    {
        Init();

        char[] buffer = new char[1];
        await _consoleIn.ReadAsync(buffer);
        return CharToConsoleKeyInfo(buffer[0]);
    }



    /// <summary>
    /// Abstract "Console.ReadLine" to be compatible even in VS Code Debug console
    /// </summary>
    public static String? ReadLine()
    {
        Init();
        if (_keyAvailable)
        {
            return Console.ReadLine();
        }
        else
        {
            return _consoleIn.ReadLine();
        }
    }


#region CONSOLE OUTPUT UTILITY METHODS

    public static string CR = System.Environment.NewLine;

    // cf. https://stackoverflow.com/questions/54122982/how-to-color-words-in-different-colours-in-a-console-writeline-in-a-console-appl
    public static string DEFAULT = _colorsNotManaged ? "" : "\x1b[39m";
    public static string BLACK = _colorsNotManaged ? "" : "\x1b[38;5;0m";
    public static string DARK_RED = _colorsNotManaged ? "" : "\x1b[38;5;1m";
    public static string DARK_GREEN = _colorsNotManaged ? "" : "\x1b[38;5;2m";
    public static string DARK_YELLOW = _colorsNotManaged ? "" : "\x1b[38;5;3m";
    public static string DARK_BLUE = _colorsNotManaged ? "" : "\x1b[38;5;4m";
    public static string DARK_MAGENTA = _colorsNotManaged ? "" : "\x1b[38;5;5m";
    public static string DARK_CYAN = _colorsNotManaged ? "" : "\x1b[38;5;6m";
    public static string GRAY = _colorsNotManaged ? "" : "\x1b[38;5;7m";
    public static string DARK_GRAY = _colorsNotManaged ? "" : "\x1b[38;5;8m";
    public static string RED = _colorsNotManaged ? "" : "\x1b[38;5;9m";
    public static string GREEN = _colorsNotManaged ? "" : "\x1b[38;5;10m";
    public static string YELLOW = _colorsNotManaged ? "" : "\x1b[38;5;11m";
    public static string BLUE = _colorsNotManaged ? "" : "\x1b[38;5;12m";
    public static string MAGENTA = _colorsNotManaged ? "" : "\x1b[38;5;13m";
    public static string CYAN = _colorsNotManaged ? "" : "\x1b[38;5;14m";
    public static string WHITE = _colorsNotManaged ? "" : "\x1b[38;5;15m";

    public static string BC_DEFAULT = _colorsNotManaged ? "" : "\x1b[49m";
    public static string BC_BLACK = _colorsNotManaged ? "" : "\x1b[48;5;0m";
    public static string BC_DARK_RED = _colorsNotManaged ? "" : "\x1b[48;5;1m";
    public static string BC_DARK_GREEN = _colorsNotManaged ? "" : "\x1b[48;5;2m";
    public static string BC_DARK_YELLOW = _colorsNotManaged ? "" : "\x1b[48;5;3m";
    public static string BC_DARK_BLUE = _colorsNotManaged ? "" : "\x1b[48;5;4m";
    public static string BC_DARK_MAGENTA = _colorsNotManaged ? "" : "\x1b[48;5;5m";
    public static string BC_DARK_CYAN = _colorsNotManaged ? "" : "\x1b[48;5;6m";
    public static string BC_GRAY = _colorsNotManaged ? "" : "\x1b[48;5;7m";
    public static string BC_DARK_GRAY = _colorsNotManaged ? "" : "\x1b[48;5;8m";
    public static string BC_RED = _colorsNotManaged ? "" : "\x1b[48;5;9m";
    public static string BC_GREEN = _colorsNotManaged ? "" : "\x1b[48;5;10m";
    public static string BC_YELLOW = _colorsNotManaged ? "" : "\x1b[48;5;11m";
    public static string BC_BLUE = _colorsNotManaged ? "" : "\x1b[48;5;12m";
    public static string BC_MAGENTA = _colorsNotManaged ? "" : "\x1b[48;5;13m";
    public static string BC_CYAN = _colorsNotManaged ? "" : "\x1b[48;5;14m";
    public static string BC_WHITE = _colorsNotManaged ? "" : "\x1b[48;5;15m";

    // Not working well - BOLD
    public static string BOLD = _colorsNotManaged ? "" : "\x1b[1m";
    public static string NO_BOLD = _colorsNotManaged ? "" : "\x1b[22m";

    public static string FAINT = _colorsNotManaged ? "" : "\x1b[2m";
    public static string NO_FAINT = _colorsNotManaged ? "" : "\x1b[22m";

    public static string ITALIC = _colorsNotManaged ? "" : "\x1b[3m";
    public static string NO_ITALIC = _colorsNotManaged ? "" : "\x1b[23m";

    public static string UNDERLINE = _colorsNotManaged ? "" : "\x1b[4m";
    public static string NO_UNDERLINE = _colorsNotManaged ? "" : "\x1b[24m";

    public static string BLINK = _colorsNotManaged ? "" : "\x1b[5m";
    public static string NO_BLINK = _colorsNotManaged ? "" : "\x1b[25m";
    // Not working well - BLINK_RAPID
    //public static string BLINK_RAPID    = _colorsNotManaged ? "" : "\x1b[6m";

    public static string REVERSE = _colorsNotManaged ? "" : "\x1b[7m";
    public static string NO_REVERSE = _colorsNotManaged ? "" : "\x1b[27m";

    public static string STRIKE = _colorsNotManaged ? "" : "\x1b[9m";
    public static string NO_STRIKE = _colorsNotManaged ? "" : "\x1b[29m";

    public static void WriteDateTime()
    {
        WriteDarkYellow($"[{DateTime.Now:HH:mm:ss.fff}]");
    }

    // Yellow => To display information with user's interactions
    public static void WriteYellow(String message)
    {
        WriteLine(YELLOW + message);
    }

    // DarkYellow => To confirm user choice
    public static void WriteDarkYellow(String message)
    {
        WriteLine(DARK_YELLOW + message);
    }

    public static void WriteWhite(String message)
    {
        WriteLine(WHITE + message);
    }

    // Red => Error occurs or major event occurs
    public static void WriteRed(String message)
    {
        WriteLine(RED + message);
    }

    // WriteGreen => to indicate a process is in progress
    public static void WriteGreen(String message)
    {
        WriteLine(GREEN + message);
    }

    // Blue => To display event result
    public static void WriteBlue(String message)
    {
        WriteLine(BLUE + message);
    }

    public static void WriteGray(String message)
    {
        WriteLine(GRAY + message);
    }

    public static void WriteLine(object? obj)
    {
        WriteLine(obj?.ToString() ?? "");
    }

    public static void WriteLine(String message, params object?[]? args)
    {
        Init();
        lock (consoleLockObject)
        {
            if(!String.IsNullOrEmpty(message))
                // Unsure to return to default mode
                message += NO_BOLD + NO_FAINT + NO_ITALIC + NO_UNDERLINE + NO_BLINK + NO_REVERSE + NO_STRIKE + DEFAULT + BC_DEFAULT;

            if(args is not null && args.Length > 0)
                System.Console.WriteLine(message, args);
            else
                System.Console.WriteLine(message);
    }
    }

    public static void WriteDemoOutput()
    {
        Init();
        WriteLine($"NORMAL: {BLACK}BLACK {DARK_RED}DARK_RED {DARK_GREEN}DARK_GREEN {DARK_YELLOW}DARK_YELLOW {DARK_BLUE}DARK_BLUE {DARK_MAGENTA}DARK_MAGENTA {DARK_CYAN}DARK_CYAN {GRAY}GRAY");
        WriteLine($"NORMAL: {DARK_GRAY}DARK_GRAY {RED}RED {GREEN}GREEN {YELLOW}YELLOW {BLUE}BLUE {MAGENTA}MAGENTA {CYAN}CYAN {WHITE}WHITE");

        WriteLine($"BOLD: {BOLD}{BLACK}BLACK {DARK_RED}DARK_RED {DARK_GREEN}DARK_GREEN {DARK_YELLOW}DARK_YELLOW {DARK_BLUE}DARK_BLUE {DARK_MAGENTA}DARK_MAGENTA {DARK_CYAN}DARK_CYAN {GRAY}GRAY");
        WriteLine($"BOLD: {BOLD}{DARK_GRAY}DARK_GRAY {RED}RED {GREEN}GREEN {YELLOW}YELLOW {BLUE}BLUE {MAGENTA}MAGENTA {CYAN}CYAN {WHITE}WHITE");

        WriteLine($"FAINT: {FAINT}{BLACK}BLACK {DARK_RED}DARK_RED {DARK_GREEN}DARK_GREEN {DARK_YELLOW}DARK_YELLOW {DARK_BLUE}DARK_BLUE {DARK_MAGENTA}DARK_MAGENTA {DARK_CYAN}DARK_CYAN {GRAY}GRAY");
        WriteLine($"FAINT: {FAINT}{DARK_GRAY}DARK_GRAY {RED}RED {GREEN}GREEN {YELLOW}YELLOW {BLUE}BLUE {MAGENTA}MAGENTA {CYAN}CYAN {WHITE}WHITE");

        WriteLine($"ITALIC: {ITALIC}{BLACK}BLACK {DARK_RED}DARK_RED {DARK_GREEN}DARK_GREEN {DARK_YELLOW}DARK_YELLOW {DARK_BLUE}DARK_BLUE {DARK_MAGENTA}DARK_MAGENTA {DARK_CYAN}DARK_CYAN {GRAY}GRAY");
        WriteLine($"ITALIC: {ITALIC}{DARK_GRAY}DARK_GRAY {RED}RED {GREEN}GREEN {YELLOW}YELLOW {BLUE}BLUE {MAGENTA}MAGENTA {CYAN}CYAN {WHITE}WHITE");

        WriteLine($"REVERSE: {REVERSE}{BLACK}BLACK {DARK_RED}DARK_RED {DARK_GREEN}DARK_GREEN {DARK_YELLOW}DARK_YELLOW {DARK_BLUE}DARK_BLUE {DARK_MAGENTA}DARK_MAGENTA {DARK_CYAN}DARK_CYAN {GRAY}GRAY");
        WriteLine($"REVERSE: {REVERSE}{DARK_GRAY}DARK_GRAY {RED}RED {GREEN}GREEN {YELLOW}YELLOW {BLUE}BLUE {MAGENTA}MAGENTA {CYAN}CYAN {WHITE}WHITE");

        WriteLine($"BLINK: {BLINK}{BLACK}BLACK {DARK_RED}DARK_RED {DARK_GREEN}DARK_GREEN {DARK_YELLOW}DARK_YELLOW {DARK_BLUE}DARK_BLUE {DARK_MAGENTA}DARK_MAGENTA {DARK_CYAN}DARK_CYAN {GRAY}GRAY");
        WriteLine($"BLINK: {BLINK}{DARK_GRAY}DARK_GRAY {RED}RED {GREEN}GREEN {YELLOW}YELLOW {BLUE}BLUE {MAGENTA}MAGENTA {CYAN}CYAN {WHITE}WHITE");

        WriteLine($"UNDERLINE: {UNDERLINE}{BLACK}BLACK {DARK_RED}DARK_RED {DARK_GREEN}DARK_GREEN {DARK_YELLOW}DARK_YELLOW {DARK_BLUE}DARK_BLUE {DARK_MAGENTA}DARK_MAGENTA {DARK_CYAN}DARK_CYAN {GRAY}GRAY");
        WriteLine($"UNDERLINE: {UNDERLINE}{DARK_GRAY}DARK_GRAY {RED}RED {GREEN}GREEN {YELLOW}YELLOW {BLUE}BLUE {MAGENTA}MAGENTA {CYAN}CYAN {WHITE}WHITE");

        WriteLine($"STRIKE: {STRIKE}{BLACK}BLACK {DARK_RED}DARK_RED {DARK_GREEN}DARK_GREEN {DARK_YELLOW}DARK_YELLOW {DARK_BLUE}DARK_BLUE {DARK_MAGENTA}DARK_MAGENTA {DARK_CYAN}DARK_CYAN {GRAY}GRAY");
        WriteLine($"STRIKE: {STRIKE}{DARK_GRAY}DARK_GRAY {RED}RED {GREEN}GREEN {YELLOW}YELLOW {BLUE}BLUE {MAGENTA}MAGENTA {CYAN}CYAN {WHITE}WHITE");

        WriteLine($"FAINT+ITALIC+UNDERLINE: {FAINT}{ITALIC}{UNDERLINE}{BLACK}BLACK {DARK_RED}DARK_RED {DARK_GREEN}DARK_GREEN {DARK_YELLOW}DARK_YELLOW {DARK_BLUE}DARK_BLUE {DARK_MAGENTA}DARK_MAGENTA {DARK_CYAN}DARK_CYAN {GRAY}GRAY");
        WriteLine($"FAINT+ITALIC+UNDERLINE: {FAINT}{ITALIC}{UNDERLINE}{DARK_GRAY}DARK_GRAY {RED}RED {GREEN}GREEN {YELLOW}YELLOW {BLUE}BLUE {MAGENTA}MAGENTA {CYAN}CYAN {WHITE}WHITE");

        WriteLine($"BC_BLUE: {BC_BLUE}BLACK {DARK_RED}DARK_RED {DARK_GREEN}DARK_GREEN {DARK_YELLOW}DARK_YELLOW {DARK_BLUE}DARK_BLUE {DARK_MAGENTA}DARK_MAGENTA {DARK_CYAN}DARK_CYAN {GRAY}GRAY");
        WriteLine($"BC_BLUE: {BC_BLUE}DARK_GRAY {RED}RED {GREEN}GREEN {YELLOW}YELLOW {BLUE}BLUE {MAGENTA}MAGENTA {CYAN}CYAN {WHITE}WHITE");
    }

#endregion CONSOLE OUTPUT UTILITY METHODS

}
