using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainbow.Example.Common
{
    /// <summary>
    /// Static class with some utility methods
    /// </summary>
    public static class Util
    {
        private static readonly Object consoleLockObject = new();

#region CONSOLE UTILITY METHODS

        // cf. https://stackoverflow.com/questions/54122982/how-to-color-words-in-different-colours-in-a-console-writeline-in-a-console-appl
        public static string DEFAULT        = System.Console.IsOutputRedirected ? "" : "\x1b[39m";
        public static string BLACK          = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;0m";
        public static string DARK_RED       = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;1m";
        public static string DARK_GREEN     = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;2m";
        public static string DARK_YELLOW    = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;3m";
        public static string DARK_BLUE      = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;4m";
        public static string DARK_MAGENTA   = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;5m";
        public static string DARK_CYAN      = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;6m";
        public static string GRAY           = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;7m";
        public static string DARK_GRAY      = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;8m";
        public static string RED            = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;9m";
        public static string GREEN          = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;10m";
        public static string YELLOW         = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;11m";
        public static string BLUE           = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;12m";
        public static string MAGENTA        = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;13m";
        public static string CYAN           = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;14m";
        public static string WHITE          = System.Console.IsOutputRedirected ? "" : "\x1b[38;5;15m";

        public static string BC_DEFAULT     = System.Console.IsOutputRedirected ? "" : "\x1b[49m";
        public static string BC_BLACK       = System.Console.IsOutputRedirected ? "" : "\x1b[48;5;0m";
        public static string BC_DARK_RED    = System.Console.IsOutputRedirected ? "" : "\x1b[48;5;1m";
        public static string BC_DARK_GREEN  = System.Console.IsOutputRedirected ? "" : "\x1b[48;5;2m";
        public static string BC_DARK_YELLOW = System.Console.IsOutputRedirected ? "" : "\x1b[48;5;3m";
        public static string BC_DARK_BLUE   = System.Console.IsOutputRedirected ? "" : "\x1b[48;5;4m";
        public static string BC_DARK_MAGENTA= System.Console.IsOutputRedirected ? "" : "\x1b[48;5;5m";
        public static string BC_DARK_CYAN   = System.Console.IsOutputRedirected ? "" : "\x1b[48;5;6m";
        public static string BC_GRAY        = System.Console.IsOutputRedirected ? "" : "\x1b[48;5;7m";
        public static string BC_DARK_GRAY   = System.Console.IsOutputRedirected ? "" : "\x1b[48;5;8m";
        public static string BC_RED         = System.Console.IsOutputRedirected ? "" : "\x1b[48;5;9m";
        public static string BC_GREEN       = System.Console.IsOutputRedirected ? "" : "\x1b[48;5;10m";
        public static string BC_YELLOW      = System.Console.IsOutputRedirected ? "" : "\x1b[48;5;11m";
        public static string BC_BLUE        = System.Console.IsOutputRedirected ? "" : "\x1b[48;5;12m";
        public static string BC_MAGENTA     = System.Console.IsOutputRedirected ? "" : "\x1b[48;5;13m";
        public static string BC_CYAN        = System.Console.IsOutputRedirected ? "" : "\x1b[48;5;14m";
        public static string BC_WHITE       = System.Console.IsOutputRedirected ? "" : "\x1b[48;5;15m";

        // Not working well - BOLD
        public static string BOLD           = System.Console.IsOutputRedirected ? "" : "\x1b[1m";
        public static string NO_BOLD         = System.Console.IsOutputRedirected ? "" : "\x1b[22m";

        public static string FAINT = System.Console.IsOutputRedirected ? "" : "\x1b[2m";
        public static string NO_FAINT = System.Console.IsOutputRedirected ? "" : "\x1b[22m";

        public static string ITALIC = System.Console.IsOutputRedirected ? "" : "\x1b[3m";
        public static string NO_ITALIC = System.Console.IsOutputRedirected ? "" : "\x1b[23m";

        public static string UNDERLINE      = System.Console.IsOutputRedirected ? "" : "\x1b[4m";
        public static string NO_UNDERLINE    = System.Console.IsOutputRedirected ? "" : "\x1b[24m";

        public static string BLINK = System.Console.IsOutputRedirected ? "" : "\x1b[5m";
        public static string NO_BLINK = System.Console.IsOutputRedirected ? "" : "\x1b[25m";
        // Not working well - BLINK_RAPID
        //public static string BLINK_RAPID    = System.Console.IsOutputRedirected ? "" : "\x1b[6m";

        public static string REVERSE        = System.Console.IsOutputRedirected ? "" : "\x1b[7m";
        public static string NO_REVERSE      = System.Console.IsOutputRedirected ? "" : "\x1b[27m";

        public static string STRIKE = System.Console.IsOutputRedirected ? "" : "\x1b[9m";
        public static string NO_STRIKE = System.Console.IsOutputRedirected ? "" : "\x1b[29m";


        public static void WriteDateTime()
        {
            WriteDarkYellow($"[{DateTime.Now:HH:mm:ss.fff}]");
        }

        // Yellow => To display information with user's interactions
        public static void WriteYellow(String message)
        {
            WriteToConsole(YELLOW + message);
        }

        // DarkYellow => To confirm user choice
        public static void WriteDarkYellow(String message)
        {
            WriteToConsole(DARK_YELLOW + message);
        }

        public static void WriteWhite(String message)
        {
            WriteToConsole(WHITE + message);
        }

        // Red => Error occurs or major event occurs
        public static void WriteRed(String message)
        {
            WriteToConsole(RED  + message);
        }

        // WriteGreen => to indicate a process is in progress
        public static void WriteGreen(String message)
        {
            WriteToConsole(GREEN + message);
        }

        // Blue => To display event result
        public static void WriteBlue(String message)
        {
            WriteToConsole(BLUE + message);
        }

        public static void WriteToConsole(String message)
        {
            lock (consoleLockObject)
            {
                // Unsure to return to default mode
                message += NO_BOLD + NO_FAINT + NO_ITALIC + NO_UNDERLINE + NO_BLINK + NO_REVERSE + NO_STRIKE + DEFAULT + BC_DEFAULT;
                System.Console.WriteLine(message);
            }
        }

        public static void WriteDemoOutput()
        {
            WriteToConsole($"NORMAL: {Util.BLACK}BLACK {Util.DARK_RED}DARK_RED {Util.DARK_GREEN}DARK_GREEN {Util.DARK_YELLOW}DARK_YELLOW {Util.DARK_BLUE}DARK_BLUE {Util.DARK_MAGENTA}DARK_MAGENTA {Util.DARK_CYAN}DARK_CYAN {Util.GRAY}GRAY");
            WriteToConsole($"NORMAL: {Util.DARK_GRAY}DARK_GRAY {Util.RED}RED {Util.GREEN}GREEN {Util.YELLOW}YELLOW {Util.BLUE}BLUE {Util.MAGENTA}MAGENTA {Util.CYAN}CYAN {Util.WHITE}WHITE");

            WriteToConsole($"BOLD: {Util.BOLD}{Util.BLACK}BLACK {Util.DARK_RED}DARK_RED {Util.DARK_GREEN}DARK_GREEN {Util.DARK_YELLOW}DARK_YELLOW {Util.DARK_BLUE}DARK_BLUE {Util.DARK_MAGENTA}DARK_MAGENTA {Util.DARK_CYAN}DARK_CYAN {Util.GRAY}GRAY");
            WriteToConsole($"BOLD: {Util.BOLD}{Util.DARK_GRAY}DARK_GRAY {Util.RED}RED {Util.GREEN}GREEN {Util.YELLOW}YELLOW {Util.BLUE}BLUE {Util.MAGENTA}MAGENTA {Util.CYAN}CYAN {Util.WHITE}WHITE");

            WriteToConsole($"FAINT: {Util.FAINT}{Util.BLACK}BLACK {Util.DARK_RED}DARK_RED {Util.DARK_GREEN}DARK_GREEN {Util.DARK_YELLOW}DARK_YELLOW {Util.DARK_BLUE}DARK_BLUE {Util.DARK_MAGENTA}DARK_MAGENTA {Util.DARK_CYAN}DARK_CYAN {Util.GRAY}GRAY");
            WriteToConsole($"FAINT: {Util.FAINT}{Util.DARK_GRAY}DARK_GRAY {Util.RED}RED {Util.GREEN}GREEN {Util.YELLOW}YELLOW {Util.BLUE}BLUE {Util.MAGENTA}MAGENTA {Util.CYAN}CYAN {Util.WHITE}WHITE");

            WriteToConsole($"ITALIC: {Util.ITALIC}{Util.BLACK}BLACK {Util.DARK_RED}DARK_RED {Util.DARK_GREEN}DARK_GREEN {Util.DARK_YELLOW}DARK_YELLOW {Util.DARK_BLUE}DARK_BLUE {Util.DARK_MAGENTA}DARK_MAGENTA {Util.DARK_CYAN}DARK_CYAN {Util.GRAY}GRAY");
            WriteToConsole($"ITALIC: {Util.ITALIC}{Util.DARK_GRAY}DARK_GRAY {Util.RED}RED {Util.GREEN}GREEN {Util.YELLOW}YELLOW {Util.BLUE}BLUE {Util.MAGENTA}MAGENTA {Util.CYAN}CYAN {Util.WHITE}WHITE");

            WriteToConsole($"REVERSE: {Util.REVERSE}{Util.BLACK}BLACK {Util.DARK_RED}DARK_RED {Util.DARK_GREEN}DARK_GREEN {Util.DARK_YELLOW}DARK_YELLOW {Util.DARK_BLUE}DARK_BLUE {Util.DARK_MAGENTA}DARK_MAGENTA {Util.DARK_CYAN}DARK_CYAN {Util.GRAY}GRAY");
            WriteToConsole($"REVERSE: {Util.REVERSE}{Util.DARK_GRAY}DARK_GRAY {Util.RED}RED {Util.GREEN}GREEN {Util.YELLOW}YELLOW {Util.BLUE}BLUE {Util.MAGENTA}MAGENTA {Util.CYAN}CYAN {Util.WHITE}WHITE");

            WriteToConsole($"BLINK: {Util.BLINK}{Util.BLACK}BLACK {Util.DARK_RED}DARK_RED {Util.DARK_GREEN}DARK_GREEN {Util.DARK_YELLOW}DARK_YELLOW {Util.DARK_BLUE}DARK_BLUE {Util.DARK_MAGENTA}DARK_MAGENTA {Util.DARK_CYAN}DARK_CYAN {Util.GRAY}GRAY");
            WriteToConsole($"BLINK: {Util.BLINK}{Util.DARK_GRAY}DARK_GRAY {Util.RED}RED {Util.GREEN}GREEN {Util.YELLOW}YELLOW {Util.BLUE}BLUE {Util.MAGENTA}MAGENTA {Util.CYAN}CYAN {Util.WHITE}WHITE");

            WriteToConsole($"UNDERLINE: {Util.UNDERLINE}{Util.BLACK}BLACK {Util.DARK_RED}DARK_RED {Util.DARK_GREEN}DARK_GREEN {Util.DARK_YELLOW}DARK_YELLOW {Util.DARK_BLUE}DARK_BLUE {Util.DARK_MAGENTA}DARK_MAGENTA {Util.DARK_CYAN}DARK_CYAN {Util.GRAY}GRAY");
            WriteToConsole($"UNDERLINE: {Util.UNDERLINE}{Util.DARK_GRAY}DARK_GRAY {Util.RED}RED {Util.GREEN}GREEN {Util.YELLOW}YELLOW {Util.BLUE}BLUE {Util.MAGENTA}MAGENTA {Util.CYAN}CYAN {Util.WHITE}WHITE");

            WriteToConsole($"STRIKE: {Util.STRIKE}{Util.BLACK}BLACK {Util.DARK_RED}DARK_RED {Util.DARK_GREEN}DARK_GREEN {Util.DARK_YELLOW}DARK_YELLOW {Util.DARK_BLUE}DARK_BLUE {Util.DARK_MAGENTA}DARK_MAGENTA {Util.DARK_CYAN}DARK_CYAN {Util.GRAY}GRAY");
            WriteToConsole($"STRIKE: {Util.STRIKE}{Util.DARK_GRAY}DARK_GRAY {Util.RED}RED {Util.GREEN}GREEN {Util.YELLOW}YELLOW {Util.BLUE}BLUE {Util.MAGENTA}MAGENTA {Util.CYAN}CYAN {Util.WHITE}WHITE");

            WriteToConsole($"FAINT+ITALIC+UNDERLINE: {Util.FAINT}{Util.ITALIC}{Util.UNDERLINE}{Util.BLACK}BLACK {Util.DARK_RED}DARK_RED {Util.DARK_GREEN}DARK_GREEN {Util.DARK_YELLOW}DARK_YELLOW {Util.DARK_BLUE}DARK_BLUE {Util.DARK_MAGENTA}DARK_MAGENTA {Util.DARK_CYAN}DARK_CYAN {Util.GRAY}GRAY");
            WriteToConsole($"FAINT+ITALIC+UNDERLINE: {Util.FAINT}{Util.ITALIC}{Util.UNDERLINE}{Util.DARK_GRAY}DARK_GRAY {Util.RED}RED {Util.GREEN}GREEN {Util.YELLOW}YELLOW {Util.BLUE}BLUE {Util.MAGENTA}MAGENTA {Util.CYAN}CYAN {Util.WHITE}WHITE");

            WriteToConsole($"BC_BLUE: {Util.BC_BLUE}BLACK {Util.DARK_RED}DARK_RED {Util.DARK_GREEN}DARK_GREEN {Util.DARK_YELLOW}DARK_YELLOW {Util.DARK_BLUE}DARK_BLUE {Util.DARK_MAGENTA}DARK_MAGENTA {Util.DARK_CYAN}DARK_CYAN {Util.GRAY}GRAY");
            WriteToConsole($"BC_BLUE: {Util.BC_BLUE}DARK_GRAY {Util.RED}RED {Util.GREEN}GREEN {Util.YELLOW}YELLOW {Util.BLUE}BLUE {Util.MAGENTA}MAGENTA {Util.CYAN}CYAN {Util.WHITE}WHITE");
        }

        #endregion CONSOLE UTILITY METHODS

    }
}
