using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceBangBang
{
    public class BattleSceneTouchPanel : MonoBehaviour, IPointerClickHandler
    {
        public Action Touch = null;

        public void OnPointerClick(PointerEventData eventData)
        {
            Touch?.Invoke();
        }
    }
}