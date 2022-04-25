using System.Collections.Generic;
using Serilog.Sinks.SystemConsole.Themes;

namespace ZonyLrcTools.Cli.Infrastructure.Logging;

public static class CustomConsoleTheme
{
    public static AnsiConsoleTheme Code { get; } = new AnsiConsoleTheme(
        new Dictionary<ConsoleThemeStyle, string>
        {
            [ConsoleThemeStyle.Text] = "\x1b[38;5;0253m",
            [ConsoleThemeStyle.SecondaryText] = "\x1b[38;5;0246m",
            [ConsoleThemeStyle.TertiaryText] = "\x1b[38;5;0242m",
            [ConsoleThemeStyle.Invalid] = "\x1b[33;1m",
            [ConsoleThemeStyle.Null] = "\x1b[38;5;0038m",
            [ConsoleThemeStyle.Name] = "\x1b[38;5;0081m",
            [ConsoleThemeStyle.String] = "\x1b[38;5;0216m",
            [ConsoleThemeStyle.Number] = "\x1b[38;5;151m",
            [ConsoleThemeStyle.Boolean] = "\x1b[38;5;0038m",
            [ConsoleThemeStyle.Scalar] = "\x1b[38;5;0079m",
            [ConsoleThemeStyle.LevelVerbose] = "\x1b[37m",
            [ConsoleThemeStyle.LevelDebug] = "\x1b[37m",
            [ConsoleThemeStyle.LevelInformation] = "\x1b[32m\x1b[48;5;0238m",
            [ConsoleThemeStyle.LevelWarning] = "\x1b[38;5;0229m",
            [ConsoleThemeStyle.LevelError] = "\x1b[38;5;0197m\x1b[48;5;0238m",
            [ConsoleThemeStyle.LevelFatal] = "\x1b[38;5;0197m\x1b[48;5;0238m",
        });
}