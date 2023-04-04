using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpaceBangBang
{
    public class DropGun : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer[] spriteRenderers;

        private Sequence _seq;
        [SerializeField]
        private SortingGroup _sort;

        public void Init(WeaponTypes weaponType)
        {
            int len = GameManager.Data.WeaponDict[(int)weaponType].Sprites.Length;

            for (int i = 0; i < len; i++)
            {
                spriteRenderers[i].sprite = GameManager.Data.WeaponDict[(int)weaponType].Sprites[i];
            }

            for (int j = len; j < spriteRenderers.Length; j++)
            {
                spriteRenderers[j].sprite = null;
            }

            _seq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                _sort.sortingOrder = 7;
                for (int i = 0; i < spriteRenderers.Length; i++)
                    spriteRenderers[i].color = Color.white;
            })
            .Append(transform.DOLocalMoveY(-5, 0.7f).SetEase(Ease.OutQuad).SetRelative())
            .Join(transform.DOLocalRotate(new Vector3(0, 0, 220), 0.6f).SetRelative().SetEase(Ease.OutQuad))
            .AppendCallback(() => _sort.sortingOrder = 0)
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                for (int i = 0; i < spriteRenderers.Length; i++)
                    spriteRenderers[i].DOFade(0, 1);
            });
        }

        private void OnEnable()
        {
            _seq.Restart();
        }
    }
}