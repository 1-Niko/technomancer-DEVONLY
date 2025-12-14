// Logging.cs
/* Defines and initializes the Logger (For debugging purposes) */

using BepInEx.Logging;

namespace Slugpack;

public static class Log
{
    private static ManualLogSource _instance;

    public static void Init(ManualLogSource logger)
    {
        _instance = logger;
    }

    public static void Info(object ex) => _instance.LogInfo(ex);

    public static void Warning(object ex) => _instance.LogWarning(ex);

    public static void Error(object ex) => _instance.LogError(ex);
}