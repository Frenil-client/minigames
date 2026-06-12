using System.Collections.Generic;
using Shared.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Main
{
    public class MainSceneController : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private MiniGameLauncher  launcher;
        [SerializeField] private UIStackManager    uiStack;
        [SerializeField] private RecycleScrollView scrollView;

        [Header("패널")]
        [SerializeField] private GameObject      gameSelectPanel;
        [SerializeField] private GameDetailPanel gameDetailPanel;

        [Header("공통 버튼")]
        [SerializeField] private Button backButton;

        [SerializeField] private GameObject mainCanvasRoot;

        [Header("미니게임 목록")]
        [SerializeField] private List<MiniGameData> miniGames;

        private void Start()
        {
            backButton.onClick.AddListener(OnBack);

            launcher.onGameLoading += OnGameLoading;
            launcher.onGameExited  += OnGameExited;

            scrollView.onSlotClicked = OnSlotClicked;
            scrollView.Initialize(BuildDataList());

            if (gameDetailPanel != null)
                gameDetailPanel.onStartClicked += OnGameStart;

            uiStack.SetRoot(gameSelectPanel);
        }

        private void OnGameLoading()
        {
            uiStack.ClearAll();
            if (mainCanvasRoot != null)
                mainCanvasRoot.SetActive(false);
        }

        private void OnGameExited()
        {
            uiStack.SetRoot(gameSelectPanel);
            if (mainCanvasRoot != null)
                mainCanvasRoot.SetActive(true);

            if (launcher.lastData != null && gameDetailPanel != null)
            {
                gameDetailPanel.Populate(launcher.lastData);
                uiStack.Push(gameDetailPanel.gameObject);
            }
        }

        private void OnSlotClicked(IScrollSlotData data)
        {
            if (data is not MiniGameData gameData) return;

            gameDetailPanel.Populate(gameData);
            uiStack.Push(gameDetailPanel.gameObject);
        }

        private void OnGameStart(MiniGameData data)
        {
            launcher.Launch(data);
        }

        private void OnBack()
        {
            if (uiStack.canPop)
            {
                uiStack.Pop();
                return;
            }

            if (launcher.isGameRunning)
            {
                launcher.Exit();
                return;
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private List<IScrollSlotData> BuildDataList()
        {
            var list = new List<IScrollSlotData>(miniGames.Count);
            foreach (MiniGameData d in miniGames) list.Add(d);
            return list;
        }
    }
}
