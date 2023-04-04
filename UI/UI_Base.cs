using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceBangBang
{
    public enum EventTypes
    {
        BeginDrag,
        Drag,
        EndDrag,
        Click
    }

    public class UI_Base : MonoBehaviour
    {
        UIEventHandler evt = null;

        protected virtual void Init()
        {
            evt = GameManager.Instance.GetOrAddComponent<UIEventHandler>(gameObject);
        }

        //UIEvent type에 따라 action에 설정해둔 행동이 이벤트형식으로 뿌려져서 실행됨
        public void AddUIEvent(Action<PointerEventData> action, EventTypes type)
        {
            switch (type)
            {
                case EventTypes.BeginDrag:
                    evt.OnBeginDragHandler -= action;
                    evt.OnBeginDragHandler += action;
                    break;
                case EventTypes.Drag:
                    evt.OnDragHandler -= action;
                    evt.OnDragHandler += action;
                    break;
                case EventTypes.EndDrag:
                    evt.OnEndDragHandler -= action;
                    evt.OnEndDragHandler += action;
                    break;

                case EventTypes.Click:
                    evt.OnPointerClickHandler -= action;
                    evt.OnPointerClickHandler += action;
                    break;
            }
        }

        //해당 이벤트 제거
        public void DeleteUIEvent(Action<PointerEventData> action, EventTypes type)
        {
            switch (type)
            {
                case EventTypes.BeginDrag:
                    evt.OnBeginDragHandler -= action;
                    break;
                case EventTypes.Drag:
                    evt.OnDragHandler -= action;
                    break;
                case EventTypes.EndDrag:
                    evt.OnEndDragHandler -= action;
                    break;

                case EventTypes.Click:
                    evt.OnPointerClickHandler -= action;
                    break;
            }
        }

        protected virtual void Clear(Player p)
        {
            evt = null;
        }
    }
}