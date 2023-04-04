using DG.Tweening;
using UnityEngine;

namespace SpaceBangBang
{
    public class BasePanel : MonoBehaviour
    {
        protected RectTransform t;
        protected Vector2 pos;
        private Sequence _activeTrueSeq;
        private Sequence _activeFalseSeq;
        [SerializeField]
        protected GameObject ChildGOParents;

        private void Awake()
        {
            Init();
        }

        protected virtual void Init()
        {
            t = gameObject.GetComponent<RectTransform>();
            pos = t.sizeDelta;
            t.sizeDelta = new Vector3(pos.x, 0);
            _activeTrueSeq = DOTween.Sequence()
            .Append(t.DOSizeDelta(pos, 0.5f).SetEase(Ease.InOutExpo))
            .AppendCallback(() =>
            {
                ChildGOParents.SetActive(true);
            })
            .Pause()
            .SetAutoKill(false);

            _activeFalseSeq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                ChildGOParents.SetActive(false);
            })
            .Append(t.DOSizeDelta(new Vector3(pos.x, 0), 0.5f).SetEase(Ease.InOutExpo))
            .AppendCallback(() =>
            {
                gameObject.SetActive(false);
            })
            .Pause()
            .SetAutoKill(false);

            ChildGOParents.SetActive(false);
            gameObject.SetActive(false);
        }

        public void BActive(bool bopen)
        {
            if (bopen)
            {
                gameObject.SetActive(true);
                _activeTrueSeq.Restart();
            }
            else
                _activeFalseSeq.Restart();
        }

        private void OnDestroy()
        {
            _activeFalseSeq.Kill();
            _activeTrueSeq.Kill();
        }
    }
}