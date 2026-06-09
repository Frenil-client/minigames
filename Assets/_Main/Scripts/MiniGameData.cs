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
        public GameObject    rootPrefab;
        public SceneReference sceneReference;
    }
}
