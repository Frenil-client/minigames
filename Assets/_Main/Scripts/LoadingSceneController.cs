using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Main
{
    public class LoadingSceneController : MonoBehaviour
    {
        public static LoadingSceneController instance { get; private set; }

        [SerializeField] private Slider          progressBar;
        [SerializeField] private TextMeshProUGUI percentText;

        [Header("로딩 텍스트 애니메이션")]
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private string          loadingLabel = "Loading";
        [SerializeField] private float           dotInterval  = 0.4f;
        [SerializeField] private int             maxDots      = 3;

        private Coroutine _dotRoutine;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            if (loadingText != null)
                _dotRoutine = StartCoroutine(DotRoutine());
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        public void SetProgress(float t)
        {
            t = Mathf.Clamp01(t);
            if (progressBar != null) progressBar.value = t;
            if (percentText != null) percentText.text  = $"{Mathf.RoundToInt(t * 100)}%";
        }

        private IEnumerator DotRoutine()
        {
            int dots = 0;
            while (true)
            {
                loadingText.text = loadingLabel + new string('.', dots);
                yield return new WaitForSecondsRealtime(dotInterval);
                dots = (dots + 1) % (maxDots + 1);
            }
        }
    }
}
