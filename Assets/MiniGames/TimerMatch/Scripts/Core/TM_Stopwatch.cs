using UnityEngine;

namespace MiniGames.TimerMatch
{
    public class TM_Stopwatch : MonoBehaviour
    {
        public float displayTime     { get; private set; }
        public float elapsedRealTime { get; private set; }
        public bool  isRunning       { get; private set; }

        private TM_RoundSpec _round;

        public bool isTimerHidden
        {
            get
            {
                if (_round == null || _round.hideStartRatio >= 1f) return false;
                return elapsedRealTime / EstimatedDuration() >= _round.hideStartRatio;
            }
        }

        public void Prime(TM_RoundSpec round)
        {
            _round          = round;
            elapsedRealTime = 0f;
            displayTime     = round.startDirection == StartDirection.FromZero
                ? 0f
                : TM_RoundSpec.MaxTime;
            isRunning       = false;
        }

        public void Run()
        {
            if (_round != null) isRunning = true;
        }

        public void Stop()
        {
            isRunning = false;
        }

        private void Update()
        {
            if (!isRunning || _round == null) return;

            elapsedRealTime += Time.deltaTime;

            float direction = _round.startDirection == StartDirection.FromZero ? 1f : -1f;
            displayTime += Time.deltaTime * _round.speedMultiplier * direction;
            displayTime  = Mathf.Clamp(displayTime, 0f, TM_RoundSpec.MaxTime);
        }

        private float EstimatedDuration()
        {
            float distance = _round.startDirection == StartDirection.FromZero
                ? _round.targetTime
                : TM_RoundSpec.MaxTime - _round.targetTime;
            float speed = Mathf.Max(0.01f, _round.speedMultiplier);
            return Mathf.Max(0.01f, distance / speed);
        }
    }
}
