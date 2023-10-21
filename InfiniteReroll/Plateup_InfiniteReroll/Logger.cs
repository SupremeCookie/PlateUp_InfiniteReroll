#if DEBUG
#define EnableLogging
#endif

public static class Logger
{
    public static void Log(string message)
    {
#if EnableLogging
        UnityEngine.Debug.Log($"[DKatGames] -- {message}");
#endif
    }

}