using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGames.Main
{
    public class SceneTransitionManager : MonoBehaviour
    {
        [SerializeField] private string loadingSceneName = "LoadingScene";
        [SerializeField] private float  completeHoldTime = 0.3f;

        public string activeGameScene { get; private set; }
        public bool   isTransitioning { get; private set; }

        public Action<string> onSceneLoaded;
        public Action         onSceneUnloaded;

        public void LoadGame(string scenePath, Action onComplete = null)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] 전환 중에는 중복 호출할 수 없습니다.");
                return;
            }
            StartCoroutine(LoadGameRoutine(scenePath, onComplete));
        }

        public void UnloadGame(Action onComplete = null)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] 전환 중에는 중복 호출할 수 없습니다.");
                return;
            }
            if (string.IsNullOrEmpty(activeGameScene))
            {
                Debug.LogWarning("[SceneTransitionManager] 언로드할 게임 씬이 없습니다.");
                onComplete?.Invoke();
                return;
            }
            StartCoroutine(UnloadGameRoutine(onComplete));
        }

        private IEnumerator LoadGameRoutine(string scenePath, Action onComplete)
        {
            isTransitioning = true;

            yield return StartCoroutine(ShowLoadingScene());

            bool hasPrev = !string.IsNullOrEmpty(activeGameScene);
            if (hasPrev)
            {
                var unload = SceneManager.UnloadSceneAsync(activeGameScene);
                while (!unload.isDone)
                {
                    SetProgress(unload.progress * 0.4f);
                    yield return null;
                }
                activeGameScene = null;
                onSceneUnloaded?.Invoke();
            }

            float loadOffset = hasPrev ? 0.4f : 0f;
            float loadRange  = 1f - loadOffset;

            var load = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
            load.allowSceneActivation = false;

            while (load.progress < 0.9f)
            {
                SetProgress(loadOffset + (load.progress / 0.9f) * loadRange);
                yield return null;
            }

            SetProgress(loadOffset + loadRange);
            load.allowSceneActivation = true;
            while (!load.isDone) yield return null;

            activeGameScene = scenePath;
            onComplete?.Invoke();
            onSceneLoaded?.Invoke(scenePath);

            yield return new WaitForSecondsRealtime(completeHoldTime);
            yield return StartCoroutine(HideLoadingScene());

            isTransitioning = false;
        }

        private IEnumerator UnloadGameRoutine(Action onComplete)
        {
            isTransitioning = true;

            var op = SceneManager.UnloadSceneAsync(activeGameScene);
            while (op != null && !op.isDone) yield return null;

            activeGameScene = null;
            onSceneUnloaded?.Invoke();
            onComplete?.Invoke();

            isTransitioning = false;
        }

        private IEnumerator ShowLoadingScene()
        {
            SetProgress(0f);
            var op = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
            if (op == null)
            {
                Debug.LogError($"[SceneTransitionManager] '{loadingSceneName}' 을 로드할 수 없습니다. Build Settings에 씬이 추가되어 있는지 확인하세요.");
                yield break;
            }
            while (!op.isDone) yield return null;
        }

        private IEnumerator HideLoadingScene()
        {
            var op = SceneManager.UnloadSceneAsync(loadingSceneName);
            while (op != null && !op.isDone) yield return null;
        }

        private void SetProgress(float t)
        {
            LoadingSceneController.instance?.SetProgress(t);
        }
    }
}
