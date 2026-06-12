using UnityEngine;

namespace MiniGames.TimerMatch
{
    public class TM_TestLauncher : MonoBehaviour
    {
        [Header("씬 단독 실행 시 시작할 스테이지")]
        [Min(1)]
        [SerializeField] private int testStage = 1;

        private void Awake()
        {
            if (MiniGameSession.launchParameter > 0) return;

            MiniGameSession.launchParameter = testStage;
            Debug.Log($"[TM_TestLauncher] 테스트 실행: Stage {testStage}");
        }
    }
}
