using System;
using UnityEngine;

namespace Shared.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class ScrollSlotView : MonoBehaviour
    {
        public Action<IScrollSlotData> onClicked     { get; set; }
        public RectTransform           rectTransform { get; private set; }

        protected virtual void Awake() => rectTransform = GetComponent<RectTransform>();

        public abstract void Bind(IScrollSlotData data);
        public virtual  void OnRecycled() { }
    }

    public abstract class ScrollSlotView<TData> : ScrollSlotView
        where TData : class, IScrollSlotData
    {
        private TData _current;

        public sealed override void Bind(IScrollSlotData data)
        {
            _current = data as TData;
            OnBind(_current);
        }

        protected abstract void OnBind(TData data);

        protected void OnClicked() => onClicked?.Invoke(_current);
    }
}
