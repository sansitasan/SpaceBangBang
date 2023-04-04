using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

namespace SpaceBangBang
{
    public class JoystickUI : UI_Base
    {
        //조이스틱 레버
        [SerializeField]
        protected RectTransform lever;

        //조이스틱의 rectTransform
        [SerializeField]
        protected RectTransform rectTransform;
        //조이스틱의 반지름
        protected float r;

        protected Player P { get; private set; } = null;

        public virtual void DIP(Player p)
        {
            P = p;
            P.Clear -= Clear;
            P.Clear += Clear;
            this.FixedUpdateAsObservable()
                .Where(_ => P == null)
                .Subscribe(_ =>
                {
                    DeleteUIEvent(DragEvent, EventTypes.Drag);
                    DeleteUIEvent(EndDragEvent, EventTypes.EndDrag);
                });
        }

        //플레이어와 조이스틱의 반지름을 가져오고 Action에 해당 함수들을 추가함
        protected override void Init()
        {
            base.Init();
            r = rectTransform.sizeDelta.x / 2;
            AddUIEvent(DragEvent, EventTypes.Drag);
            AddUIEvent(EndDragEvent, EventTypes.EndDrag);
        }

        //lever가 일정 범위 밖으로 벗어나지 못하게 고정
        protected virtual void DragEvent(PointerEventData eventData)
        {
            lever.position = eventData.position;
            if (Mathf.Pow(lever.localPosition.x, 2) + Mathf.Pow(lever.localPosition.y, 2) >= Mathf.Pow(r, 2))
                lever.localPosition = lever.localPosition.normalized * r;
        }

        //드래그가 끝나면 lever 원위치
        protected virtual void EndDragEvent(PointerEventData eventData)
        {
            lever.localPosition = Vector2.zero;
        }

        protected override void Clear(Player p)
        {
            lever.localPosition = Vector2.zero;
            P.Clear -= Clear;
            DeleteUIEvent(DragEvent, EventTypes.Drag);
            DeleteUIEvent(EndDragEvent, EventTypes.EndDrag);
            base.Clear(p);
            gameObject.SetActive(false);
        }
    }
}