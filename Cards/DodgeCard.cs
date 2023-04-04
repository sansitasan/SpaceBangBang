namespace SpaceBangBang
{
    public class DodgeCard : ICard
    {
        public DodgeCard(CardType t)
        {
            Type = t;
        }
        public CardType Type { get; set; }
        public bool UseCard(Player p)
        {
            return true;
        }
    }
}