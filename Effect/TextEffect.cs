using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using SpaceBangBang;

public class TextEffect : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Sprite[] sprites; // 0: Dodge, 1: Steal

    private Sequence _seq;

    public void Init(EffectType cardType)
    {
        if (cardType == EffectType.DodgeText)
        {
            spriteRenderer.sprite = sprites[0];
        }
        else if (cardType == EffectType.StealText)
        {
            spriteRenderer.sprite = sprites[1];
        }
        _seq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                spriteRenderer.color = Color.white;
            })
            .Append(transform.DOMoveY(1f, 3f).SetEase(Ease.OutExpo).SetRelative())
            .Insert(0.5f, spriteRenderer.DOFade(0, 0.5f))
            .SetAutoKill(false)
            .Pause();
    }

    private void OnEnable()
    {
        _seq.Restart();
    }
}
