using System.Collections.Generic;
using MiniGames.Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Main
{
    /// <summary>
    /// MainScene 루트 컨트롤러.
    ///
    /// ─ Hierarchy 예시 ────────────────────────────────────────────────────
    /// Canvas (Screen Space - Overlay, 1920×1080)
    /// ├── TopBar          (height 140 px — 전체 높이의 약 13%)
    /// │   ├── BackButton  (Button)  ← Inspector 연결
    /// │   └── HomeButton  (Button)  ← Inspector 연결
    /// └── ScrollArea      (나머지 전체)
    ///     └── RecycleScrollView  (ScrollRect + RecycleScrollView)
    ///         ├── Viewport  (Mask)
    ///         │   └── Content
    ///         └── (Horizontal Scrollbar — 선택)
    /// ─────────────────────────────────────────────────────────────────────
    /// </summary>
    public class MainSceneController : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private RecycleScrollView scrollView;
        [SerializeField] private MiniGameLauncher  launcher;

        [Header("버튼")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button homeButton;

        [Header("미니게임 목록")]
        [SerializeField] private List<MiniGameData> miniGames;

        // ── Unity ─────────────────────────────────────────────────────────

        private void Start()
        {
            backButton.onClick.AddListener(OnBack);
            homeButton.onClick.AddListener(OnHome);

            scrollView.onSlotClicked = OnSlotClicked;
            scrollView.Initialize(BuildDataList());
        }

        // ── Callbacks ─────────────────────────────────────────────────────

        private void OnSlotClicked(IScrollSlotData data)
        {
            if (data is MiniGameData gameData)
                launcher.Launch(gameData);
        }

        private void OnBack()
        {
            launcher.Exit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnHome() => launcher.Exit();

        // ── Helpers ───────────────────────────────────────────────────────

        private List<IScrollSlotData> BuildDataList()
        {
            var list = new List<IScrollSlotData>(miniGames.Count);
            foreach (MiniGameData d in miniGames) list.Add(d);
            return list;
        }
    }
}
