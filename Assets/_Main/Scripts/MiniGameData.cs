using MiniGames.Common.UI;
using UnityEngine;

namespace MiniGames.Main
{
    /// <summary>
    /// 메인 씬 카드 슬롯에 표시할 미니게임 메타데이터.
    /// IScrollSlotData 를 구현하여 RecycleScrollView 에 직접 전달할 수 있다.
    /// </summary>
    [CreateAssetMenu(menuName = "MiniGames/MiniGameData", fileName = "MiniGameData_")]
    public class MiniGameData : ScriptableObject, IScrollSlotData
    {
        [Header("표시 정보")]
        public string gameName;
        public Sprite thumbnail;
        public Sprite logo;

        [Header("진입점")]
        [Tooltip("IMiniGame 을 구현한 루트 프리팹. null 이면 scenePath 를 사용.")]
        public GameObject rootPrefab;

        [Tooltip("rootPrefab 이 null 일 때 사용하는 Additive 씬 경로")]
        public string scenePath;
    }
}
