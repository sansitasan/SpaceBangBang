namespace SpaceBangBang
{
    public interface ICard
    {
        public CardType Type { get; set; }
        //지정된 카드를 사용합니다.
        bool UseCard(Player p);
    }
}