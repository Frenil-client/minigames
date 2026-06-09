using UnityEngine;

public static class GameScoreManager
{
    private const string Prefix = "BestScore_";

    public static int GetBest(string gameName)
        => PlayerPrefs.GetInt(Prefix + gameName, 0);

    public static bool TryUpdateBest(string gameName, int score)
    {
        if (score <= GetBest(gameName)) return false;
        PlayerPrefs.SetInt(Prefix + gameName, score);
        PlayerPrefs.Save();
        return true;
    }

    public static void DeleteBest(string gameName)
        => PlayerPrefs.DeleteKey(Prefix + gameName);
}
