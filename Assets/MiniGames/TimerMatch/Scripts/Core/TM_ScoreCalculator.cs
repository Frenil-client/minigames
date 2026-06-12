using UnityEngine;

namespace MiniGames.TimerMatch
{
    public struct TM_RoundResult
    {
        public int   roundIndex;
        public float targetTime;
        public float resultTime;
        public float signedError;
        public float error;
        public int   score;
    }

    public static class TM_ScoreCalculator
    {
        private struct Step
        {
            public float maxError;
            public int   score;
        }

        private static readonly Step[] Steps =
        {
            new Step { maxError = 0.05f, score = 10000 },
            new Step { maxError = 0.15f, score = 8000 },
            new Step { maxError = 0.40f, score = 6000 },
            new Step { maxError = 1.00f, score = 4000 },
            new Step { maxError = 5.00f, score = 2000 },
        };

        public static TM_RoundResult Evaluate(int roundIndex, TM_RoundSpec round, float resultTime)
        {
            float signed = resultTime - round.targetTime;
            float error  = Mathf.Abs(signed);

            int score = 0;
            foreach (var step in Steps)
            {
                if (error > step.maxError) continue;
                score = step.score;
                break;
            }

            return new TM_RoundResult
            {
                roundIndex  = roundIndex,
                targetTime  = round.targetTime,
                resultTime  = resultTime,
                signedError = signed,
                error       = error,
                score       = score,
            };
        }
    }
}
