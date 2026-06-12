using TMPro;
using UnityEngine;

namespace MiniGames.TimerMatch
{
    public class TM_RoundResultPopup : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private GameObject      panelRoot;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("오차 색상")]
        [SerializeField] private Color plusColor  = new Color(0.35f, 1f, 0.35f);
        [SerializeField] private Color minusColor = new Color(1f, 0.35f, 0.35f);

        private TM_GameManager _gm;

        private void Start()
        {
            _gm = TM_GameManager.instance;
            if (_gm == null) return;

            _gm.onStateChanged  += Refresh;
            _gm.onRoundFinished += Show;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_gm == null) return;
            _gm.onStateChanged  -= Refresh;
            _gm.onRoundFinished -= Show;
        }

        private void Refresh()
        {
            if (_gm.state != TM_GameState.RoundResult && panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void Show(TM_RoundResult result)
        {
            SetText(roundText, $"{result.roundIndex + 1} ROUND");

            bool plus = result.signedError >= 0f;
            if (errorText != null)
            {
                errorText.text  = $"{(plus ? "+" : "-")} {result.error:00.00}";
                errorText.color = plus ? plusColor : minusColor;
            }

            SetText(scoreText, $"+{result.score}");

            if (panelRoot != null) panelRoot.SetActive(true);
        }

        private static void SetText(TextMeshProUGUI label, string text)
        {
            if (label != null) label.text = text;
        }
    }
}
