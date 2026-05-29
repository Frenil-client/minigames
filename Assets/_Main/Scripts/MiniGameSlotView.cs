using MiniGames.Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Main
{
    /// <summary>
    /// 메인 씬 카드 슬롯 View.
    /// ScrollSlotView&lt;MiniGameData&gt; 를 상속하여 RecycleScrollView 에서 재사용된다.
    ///
    /// ─ Prefab 계층 예시 ──────────────────────────────────────────────────
    /// MiniGameSlotView  (RectTransform + MiniGameSlotView + Button)
    /// ├── Thumbnail     (Image — 카드 배경 아트워크, 카드 꽉 채움)
    /// ├── LogoImage     (Image — 게임 로고, 선택)
    /// └── NameText      (TextMeshProUGUI — 게임 이름)
    ///
    /// ─ 비율 가이드 (1920×1080 기준) ──────────────────────────────────────
    ///   · 카드 W × H ≈ 530 × 780  (약 2:3)
    ///   · RecycleScrollView.slotVertPad = 160  (상하 각 80px)
    /// ─────────────────────────────────────────────────────────────────────
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MiniGameSlotView : ScrollSlotView<MiniGameData>
    {
        [SerializeField] private Image    thumbnail;
        [SerializeField] private TMP_Text nameText;

        protected override void Awake()
        {
            base.Awake();
            GetComponent<Button>().onClick.AddListener(OnClicked);
        }

        protected override void OnBind(MiniGameData data)
        {
            if (data == null) return;
            if (thumbnail  != null) thumbnail.sprite  = data.thumbnail;
            if (nameText   != null) nameText.text     = data.gameName;
        }

        public override void OnRecycled()
        {
            if (thumbnail != null) thumbnail.sprite = null;
            if (nameText  != null) nameText.text    = string.Empty;
        }
    }
}
