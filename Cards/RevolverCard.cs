namespace SpaceBangBang
{
    public class RevolverCard : ICard
    {
        public RevolverCard(CardType t)
        {
            Type = t;
        }
        public CardType Type { get; set; }
        public bool UseCard(Player p)
        {
            //차후 수정
            //
            return true;
        }
    }
}