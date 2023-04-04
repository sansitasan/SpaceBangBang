using Photon.Pun;

namespace SpaceBangBang
{
    public class SniperCard : ICard
    {
        public SniperCard(CardType t)
        {
            Type = t;
        }
        public CardType Type { get; set; }
        public bool UseCard(Player p)
        {
            p.EquipWeapon(WeaponTypes.Sniper, PhotonNetwork.LocalPlayer);
            return true;
        }
    }
}