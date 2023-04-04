using DG.Tweening;

namespace SpaceBangBang
{
    public class ScopeCard : ICard
    {
        public ScopeCard(CardType t)
        {
            Type = t;
        }
        public CardType Type { get; set; }

        public bool UseCard(Player p)
        {
            if (p._camera.m_Lens.OrthographicSize <= 20)
                DOTween.Sequence()
                .Append(DOTween.To(() => p._camera.m_Lens.OrthographicSize, x => p._camera.m_Lens.OrthographicSize = x, 25, 1).SetEase(Ease.InOutQuart));
            return true;
        }
    }
}