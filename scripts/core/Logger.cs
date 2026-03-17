using Godot;
using System;
using System.Diagnostics;

namespace Game.Core;

// Il manquait cette définition d'énumération
public enum LogLevel
{
    DEBUG,
    INFO,
    WARNING,
    ERROR
}

public static class Logger
{
    public static void Log(LogLevel level, params object[] message)
    {
        var dateTime = DateTime.Now;
        string timeStamp = $"[{dateTime:yyyy-MM-dd HH:mm:ss}]";
        
        // On récupère la méthode appelante
        var stackTrace = new StackTrace();
        var frame = stackTrace.GetFrame(2); // 2 pour remonter au-dessus de Log() et Info/Debug/etc.
        var callingMethod = frame?.GetMethod();
        string className = callingMethod?.DeclaringType?.Name ?? "Unknown";
        string methodName = callingMethod?.Name ?? "Unknown";

        string logMessage = $"{timeStamp} [{level}] [{className}] [{methodName}] ";
        string color = "white";

        switch (level)
        {
            case LogLevel.DEBUG:
                color = "ghost_white";
                break;
            case LogLevel.INFO:
                color = "cyan";
                break;
            case LogLevel.WARNING:
                color = "yellow";
                break;
            case LogLevel.ERROR:
                color = "red";
                break;
        }

        // Construction du message final
        string fullMessage = string.Join(" ", message);
        GD.PrintRich($"[color={color}]{logMessage}{fullMessage}[/color]");
    }

    public static void Debug(params object[] message) => Log(LogLevel.DEBUG, message);
    public static void Info(params object[] message) => Log(LogLevel.INFO, message);
    public static void Warning(params object[] message) => Log(LogLevel.WARNING, message);
    public static void Error(params object[] message) => Log(LogLevel.ERROR, message);
}