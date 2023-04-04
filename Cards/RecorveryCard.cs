namespace SpaceBangBang
{
    public class RecorveryCard : ICard
    {
        public RecorveryCard(CardType t)
        {
            Type = t;
        }
        public CardType Type { get; set; }
        public bool UseCard(Player p)
        {
            return p.addHP(1);
        }
    }
}