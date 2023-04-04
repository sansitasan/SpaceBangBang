using System.Collections;
using TMPro;
using UnityEngine;

namespace SpaceBangBang
{
    public class MessagePanel : BasePanel
    {
        [SerializeField]
        private TextMeshProUGUI _msg;
        private WaitUntil wu;
        private bool _bTouch;

        protected override void Init()
        {
            base.Init();
        }

        public void SetMessage(string msg)
        {
            if (!gameObject.activeSelf)
            {
                BActive(true);
                StartCoroutine(Message(msg));
            }
        }

        private IEnumerator Message(string msg)
        {
            _msg.text = msg;
            yield return GameManager.Instance.Wfs(150);
            BActive(false);
        }
    }
}