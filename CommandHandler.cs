using System;

namespace PearlStealer
{
    internal static class CommandHandler
    {
        public static string Execute(string input)
        {
            string[] parts = input.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return "ERROR: Empty command";

            string command = parts[0].ToUpperInvariant();
            string[] options = parts[1..];

            return command switch
            {
                "PING" => HandlePing(options),
                "TIME" => HandleTime(options),
                "STATUS" => HandleStatus(options),
                "HELP" => HandleHelp(options),
                "STOP" => HandleStop(options),
                _ => "ERROR: Unknown command"
            };
        }

        private static string HandlePing(string[] options)
        {
            if (options.Length != 0)
                return "ERROR: PING accepts no options";

            return "PONG";
        }

        private static string HandleTime(string[] options)
        {
            if (options.Length == 0)
                return DateTimeOffset.Now.ToString("O");

            if (options.Length == 1 &&
                options[0].Equals(
                    "--utc",
                    StringComparison.OrdinalIgnoreCase))
            {
                return DateTimeOffset.UtcNow.ToString("O");
            }

            return "ERROR: Usage: TIME [--utc]";
        }

        private static string HandleStatus(string[] options)
        {
            if (options.Length != 0)
                return "ERROR: STATUS accepts no options";

            return "RUNNING";
        }

        private static string HandleHelp(string[] options)
        {
            if (options.Length != 0)
                return "ERROR: HELP accepts no options";

            return string.Join(
                Environment.NewLine,
                "Available commands:",
                "PING",
                "TIME [--utc]",
                "STATUS",
                "HELP",
                "STOP");
        }

        private static string HandleStop(string[] options)
        {
            if (options.Length != 0)
                return "ERROR: STOP accepts no options";

            return "BYE";
        }
    }
}