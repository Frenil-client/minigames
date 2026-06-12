using System;

public static class MiniGameSession
{
    public static int launchParameter;

    public static event Action onExitRequested;

    public static bool RequestExit()
    {
        if (onExitRequested == null) return false;
        onExitRequested.Invoke();
        return true;
    }
}
