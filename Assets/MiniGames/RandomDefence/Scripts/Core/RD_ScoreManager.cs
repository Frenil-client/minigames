using System;
using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_ScoreManager : MonoBehaviour
    {
        private static readonly int[] _milestones = { 10_000, 30_000, 50_000, 100_000 };

        public int totalScore     { get; private set; }
        public int killCount      { get; private set; }
        public int milestone      { get; private set; }
        public int bestTowerGrade { get; private set; }

        public Action<int> onScoreChanged;
        public Action<int> onMilestoneReached;

        public void Initialize()
        {
            totalScore = killCount = milestone = bestTowerGrade = 0;
            onScoreChanged?.Invoke(0);
        }

        public void ReportTowerGrade(int grade)
        {
            if (grade > bestTowerGrade)
                bestTowerGrade = grade;
        }

        public void AddScore(int value)
        {
            totalScore += value;
            killCount++;
            onScoreChanged?.Invoke(totalScore);
            CheckMilestones();
        }

        public void AddBossBonus(int bonus)
        {
            totalScore += bonus;
            onScoreChanged?.Invoke(totalScore);
            CheckMilestones();
        }

        private void CheckMilestones()
        {
            for (int i = milestone; i < _milestones.Length; i++)
            {
                if (totalScore < _milestones[i]) break;
                milestone = i + 1;
                onMilestoneReached?.Invoke(milestone);
            }
        }
    }
}
