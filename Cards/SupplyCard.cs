namespace SpaceBangBang
{
    public class SupplyCard : ICard
    {
        public SupplyCard(CardType t)
        {
            Type = t;
        }
        public CardType Type { get; set; }
        public bool UseCard(Player p)
        {
            p.AddCard();
            p.AddCard();
            return true;
        }
    }
}