using Shared.UI;
using UnityEngine;

namespace MiniGames.Main
{
    [CreateAssetMenu(menuName = "MiniGames/MiniGameData", fileName = "MiniGameData_")]
    public class MiniGameData : ScriptableObject, IScrollSlotData
    {
        [Header("표시 정보")]
        public string  gameName;
        public Sprite  thumbnail;
        public Texture mainBackground;

        [Header("진입점")]
        public SceneReference sceneReference;

        [Header("난이도 선택 (0 = 선택 UI 없음)")]
        public int stageCount;
        [Tooltip("해금된 최대 스테이지를 저장한 PlayerPrefs 키. 비우면 전부 해금")]
        public string stageUnlockPrefsKey;
    }
}
