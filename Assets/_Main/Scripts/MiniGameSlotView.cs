using Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Main
{
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
            if (thumbnail != null) thumbnail.sprite = data.thumbnail;
            if (nameText  != null) nameText.text    = data.gameName;
        }

        public override void OnRecycled()
        {
            if (thumbnail != null) thumbnail.sprite = null;
            if (nameText  != null) nameText.text    = string.Empty;
        }
    }
}
