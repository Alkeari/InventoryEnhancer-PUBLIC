using System;
using System.IO;

namespace InventoryEnhancer.Helpers;

public static class InventoryEnhancerLogger
{
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "Mount and Blade II Bannerlord", "Configs", "ModLogs");

    private static string LogFilePath => Path.Combine(
        LogDirectory, $"InventoryEnhancer{DateTime.Now:yyyyMMdd}.log");

    public static void Log(string message)
    {
#if !DEBUG
        if (Settings.Settings.Instance != null && !Settings.Settings.Instance.EnableLogging) return;
#endif
        WriteEntry("[INFO]", message);
    }

    public static void LogError(string message, Exception? ex = null)
    {
        var entry = ex != null ? $"{message}{Environment.NewLine}{ex}" : message;
        WriteEntry("[ERROR]", entry);
    }

    private static void WriteEntry(string level, string message)
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level} {message}{Environment.NewLine}";
            File.AppendAllText(LogFilePath, logMessage);
        }
        catch
        {
            // ignored
        }
    }
}
