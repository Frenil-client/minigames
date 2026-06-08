using System;
using UnityEngine;

namespace Shared.UI
{
    /// <summary>
    /// 재사용 스크롤 슬롯의 추상 베이스.
    /// RecycleScrollView 가 풀링·바인딩을 관리한다.
    ///
    /// ■ 상속 방법
    ///   1) 직접 상속 (캐스팅 직접 처리):
    ///      public class MySlotView : ScrollSlotView { ... }
    ///
    ///   2) 타입 안전 중간 베이스 사용 (권장):
    ///      public class MySlotView : ScrollSlotView&lt;MyData&gt; { ... }
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class ScrollSlotView : MonoBehaviour
    {
        public Action<IScrollSlotData> onClicked     { get; set; }
        public RectTransform           rectTransform { get; private set; }

        protected virtual void Awake() => rectTransform = GetComponent<RectTransform>();

        public abstract void Bind(IScrollSlotData data);
        public virtual  void OnRecycled() { }
    }

    /// <summary>
    /// 타입 안전 슬롯 베이스.
    /// 구체 슬롯은 OnBind(TData) 만 구현하면 된다.
    /// </summary>
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
