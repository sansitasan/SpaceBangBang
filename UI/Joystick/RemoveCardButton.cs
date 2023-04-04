using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceBangBang
{
    public class RemoveCardButton : MonoBehaviour, IPointerClickHandler
    {
        private Player _p;

        public void Init(Player p)
        {
            _p = p;
            p.Clear -= Clear;
            p.Clear += Clear;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _p.RemoveCard(0);
        }

        private void Clear(Player p)
        {
            p.Clear -= Clear;
            gameObject.SetActive(false);
        }
    }
}