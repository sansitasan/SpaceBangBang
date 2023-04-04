namespace SpaceBangBang
{
    public class SprayCard : ICard
    {
        public SprayCard(CardType t)
        {
            Type = t;
        }
        public CardType Type { get; set; }
        public bool UseCard(Player p)
        {
            p.gun.SparyShot();
            return true;
        }
    }
}