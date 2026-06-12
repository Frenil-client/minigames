using UnityEngine;

namespace MiniGames.TimerMatch
{
    public static class TM_StageUnlock
    {
        private const string UnlockedKey  = "TM_UnlockedStage";
        private const string HighScoreKey = "TM_HighScore_Stage_";

        public static int unlockedStage => Mathf.Max(1, PlayerPrefs.GetInt(UnlockedKey, 1));

        public static bool IsUnlocked(int stageIndex) => stageIndex <= unlockedStage;

        public static bool IsCleared(int stageIndex) => stageIndex < unlockedStage;

        public static void Unlock(int stageIndex)
        {
            if (stageIndex <= unlockedStage) return;
            PlayerPrefs.SetInt(UnlockedKey, stageIndex);
            PlayerPrefs.Save();
        }

        public static int GetHighScore(int stageIndex)
            => PlayerPrefs.GetInt(HighScoreKey + stageIndex, 0);

        public static bool TryUpdateHighScore(int stageIndex, int score)
        {
            if (score <= GetHighScore(stageIndex)) return false;
            PlayerPrefs.SetInt(HighScoreKey + stageIndex, score);
            PlayerPrefs.Save();
            return true;
        }
    }
}
