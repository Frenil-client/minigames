using System;
using UnityEngine;

namespace MiniGames.Common.UI
{
    /// <summary>
    /// 재사용 스크롤 슬롯의 추상 베이스.
    /// RecycleScrollView 가 풀링·바인딩을 관리한다.
    ///
    /// ■ 상속 방법
    ///   1) 구체 슬롯이 직접 상속할 경우 (데이터 캐스팅 직접 처리):
    ///      public class MySlotView : ScrollSlotView { ... }
    ///
    ///   2) 타입 안전 중간 베이스를 사용할 경우 (권장):
    ///      public class MySlotView : ScrollSlotView&lt;MyData&gt; { ... }
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class ScrollSlotView : MonoBehaviour
    {
        /// <summary>슬롯 클릭 시 RecycleScrollView 를 통해 호출되는 콜백.</summary>
        public Action<IScrollSlotData> onClicked { get; set; }

        public RectTransform rectTransform { get; private set; }

        protected virtual void Awake() => rectTransform = GetComponent<RectTransform>();

        /// <summary>데이터를 받아 슬롯 표시를 갱신한다.</summary>
        public abstract void Bind(IScrollSlotData data);

        /// <summary>슬롯이 풀에 반환될 때 호출된다. 상태 초기화에 사용.</summary>
        public virtual void OnRecycled() { }
    }

    /// <summary>
    /// 타입 안전 슬롯 베이스.
    /// 구체 슬롯은 이 클래스를 상속하고 OnBind(TData) 만 구현하면 된다.
    /// Unity Inspector 에서 구체 클래스를 직접 어태치할 수 있다.
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

        /// <summary>버튼 클릭 시 OnClicked 를 발행할 때 사용.</summary>
        protected void OnClicked() => onClicked?.Invoke(_current);
    }
}
