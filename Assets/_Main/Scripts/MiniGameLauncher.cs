using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGames.Main
{
    public class MiniGameLauncher : MonoBehaviour
    {
        [SerializeField] private SceneTransitionManager transitionManager;

        private GameObject   _currentInstance;
        private IMiniGame    _currentMiniGame;
        private MiniGameData _lastData;

        public bool isGameRunning =>
            _currentInstance != null ||
            (transitionManager != null && !string.IsNullOrEmpty(transitionManager.activeGameScene));

        public Action               onGameLoading;
        public Action<MiniGameData> onGameLaunched;
        public Action               onGameExited;

        public void Launch(MiniGameData data)
        {
            _lastData = data;

            if (data.rootPrefab != null)
            {
                ExitImmediate();
                onGameLoading?.Invoke();
                LaunchPrefab(data);
            }
            else if (data.sceneReference != null && data.sceneReference.isValid)
            {
                _currentMiniGame?.OnMiniGameExit();
                _currentMiniGame = null;
                Time.timeScale = 1f;

                onGameLoading?.Invoke();
                transitionManager.LoadGame(data.sceneReference.path, () => OnGameSceneLoaded(data));
            }
            else
            {
                Debug.LogWarning($"[Launcher] '{data.gameName}' 에 rootPrefab 또는 sceneReference 가 없습니다.");
            }
        }

        public void Relaunch()
        {
            if (_lastData != null) Launch(_lastData);
        }

        public void Exit()
        {
            if (_currentInstance != null)
            {
                ExitImmediate();
                return;
            }

            if (transitionManager != null && !string.IsNullOrEmpty(transitionManager.activeGameScene))
            {
                _currentMiniGame?.OnMiniGameExit();
                _currentMiniGame = null;
                Time.timeScale = 1f;

                transitionManager.UnloadGame(() =>
                {
                    onGameExited?.Invoke();
                });
                return;
            }

            if (_currentMiniGame != null)
            {
                Debug.LogWarning("[MiniGameLauncher] activeGameScene 추적 없이 Exit 호출. 폴백으로 처리합니다.");
                _currentMiniGame.OnMiniGameExit();
                _currentMiniGame = null;
                Time.timeScale = 1f;
                onGameExited?.Invoke();
            }
        }

        private void ExitImmediate()
        {
            if (_currentInstance == null) return;

            _currentMiniGame?.OnMiniGameExit();
            Destroy(_currentInstance);
            _currentInstance = null;
            _currentMiniGame = null;
            Time.timeScale = 1f;
            onGameExited?.Invoke();
        }

        private void LaunchPrefab(MiniGameData data)
        {
            _currentInstance = Instantiate(data.rootPrefab);
            _currentMiniGame = _currentInstance.GetComponent<IMiniGame>();
            _currentMiniGame?.OnMiniGameStart();
            onGameLaunched?.Invoke(data);
        }

        private void OnGameSceneLoaded(MiniGameData data)
        {
            Scene scene = SceneManager.GetSceneByPath(data.sceneReference.path);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.TryGetComponent<IMiniGame>(out IMiniGame game))
                {
                    _currentMiniGame = game;
                    game.OnMiniGameStart();
                    break;
                }
            }
            onGameLaunched?.Invoke(data);
        }
    }
}
