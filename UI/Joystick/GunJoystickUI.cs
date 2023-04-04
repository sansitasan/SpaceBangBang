using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceBangBang
{
    public class GunJoystickUI : JoystickUI
    {
        private bool _brange;
        private void Awake()
        {
            Init();
        }

        public override void DIP(Player p)
        {
            base.DIP(p);
            this.FixedUpdateAsObservable()
                .Where(_ => P == null)
                .Subscribe(_ =>
                        DeleteUIEvent(ClickEvent, EventTypes.Click));
        }

        protected override void Init()
        {
            base.Init();
            AddUIEvent(ClickEvent, EventTypes.Click);
        }

        protected override void DragEvent(PointerEventData eventData)
        {
            base.DragEvent(eventData);
            P.LookUpdate(lever.localPosition);
        }

        protected override void EndDragEvent(PointerEventData eventData)
        {
            P.UseCard(0);
            base.EndDragEvent(eventData);
        }

        private void ClickEvent(PointerEventData eventData)
        {
            if(lever.localPosition == Vector3.zero)
                P.UseCard(0);
        }

        protected override void Clear(Player p)
        {
            DeleteUIEvent(ClickEvent, EventTypes.Click);
            base.Clear(p);
        }

        public GameObject getObject()
        {
            return this.gameObject;
        }
    }
}