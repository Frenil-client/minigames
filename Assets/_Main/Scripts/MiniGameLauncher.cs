using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGames.Main
{
    /// <summary>
    /// 미니게임 로드 / 언로드 담당.
    /// MainScene 에 상주하며 MainSceneController 로부터 호출된다.
    /// </summary>
    public class MiniGameLauncher : MonoBehaviour
    {
        private GameObject _currentInstance;
        private string     _currentScenePath;

        // ── Public ────────────────────────────────────────────────────────

        public void Launch(MiniGameData data)
        {
            Exit();

            if (data.rootPrefab != null)
                LaunchPrefab(data.rootPrefab);
            else if (!string.IsNullOrEmpty(data.scenePath))
                LaunchScene(data.scenePath);
            else
                Debug.LogWarning($"[Launcher] '{data.gameName}' 에 rootPrefab 또는 scenePath 가 없습니다.");
        }

        public void Exit()
        {
            if (_currentInstance != null)
            {
                _currentInstance.GetComponent<IMiniGame>()?.OnMiniGameExit();
                Destroy(_currentInstance);
                _currentInstance = null;
                Time.timeScale = 1f;
            }

            if (!string.IsNullOrEmpty(_currentScenePath))
            {
                SceneManager.UnloadSceneAsync(_currentScenePath);
                _currentScenePath = null;
                Time.timeScale = 1f;
            }
        }

        // ── Private ───────────────────────────────────────────────────────

        private void LaunchPrefab(GameObject prefab)
        {
            _currentInstance = Instantiate(prefab);
            _currentInstance.GetComponent<IMiniGame>()?.OnMiniGameStart();
        }

        private void LaunchScene(string scenePath)
        {
            _currentScenePath = scenePath;
            SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive).completed += _ =>
            {
                Scene scene = SceneManager.GetSceneByPath(scenePath);
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.TryGetComponent<IMiniGame>(out IMiniGame game))
                    {
                        game.OnMiniGameStart();
                        break;
                    }
                }
            };
        }
    }
}
