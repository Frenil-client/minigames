using UnityEngine;

namespace MiniGames.RandomDefence
{
    public class RD_TimeScaleController : MonoBehaviour
    {
        private static readonly float[] _speeds = { 1f, 2f, 3f };
        private int  _speedIndex     = 0;
        private bool _isPaused       = false;
        private bool _isDragOverride = false;

        private const float DragSlowScale = 0.1f;

        public float currentSpeed => _speeds[_speedIndex];
        public bool  isPaused     => _isPaused;

        public void CycleSpeed()
        {
            if (_isPaused) return;
            _speedIndex = (_speedIndex + 1) % _speeds.Length;
            Apply();
        }

        public void SetPause(bool pause)
        {
            _isPaused = pause;
            Apply();
        }

        public void SetDragOverride(bool active)
        {
            _isDragOverride = active;
            Apply();
        }

        public void Reset()
        {
            _speedIndex     = 0;
            _isPaused       = false;
            _isDragOverride = false;
            Time.timeScale  = 1f;
        }

        private void Apply()
        {
            if (_isPaused)
                Time.timeScale = 0f;
            else if (_isDragOverride)
                Time.timeScale = DragSlowScale;
            else
                Time.timeScale = _speeds[_speedIndex];
        }
    }
}
