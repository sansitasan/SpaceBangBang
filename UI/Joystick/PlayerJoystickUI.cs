using UnityEngine.EventSystems;

namespace SpaceBangBang
{
    public class PlayerJoystickUI : JoystickUI
    {
        private void Awake()
        {
            Init();
        }

        protected override void Init()
        {
            base.Init();
        }

        protected override void DragEvent(PointerEventData eventData)
        {
            base.DragEvent(eventData);
            P.StateUpdate(PlayerStates.Move, lever.localPosition / r);
        }

        protected override void EndDragEvent(PointerEventData eventData)
        {
            base.EndDragEvent(eventData);
            P.StateUpdate(PlayerStates.Idle, lever.localPosition);
        }
    }
}