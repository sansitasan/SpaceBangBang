using Photon.Pun;

namespace SpaceBangBang
{
    public class LazerCard : ICard
    {
        public LazerCard(CardType t)
        {
            Type = t;
        }
        public CardType Type { get; set; }
        public bool UseCard(Player p)
        {
            p.EquipWeapon(WeaponTypes.Lazer, PhotonNetwork.LocalPlayer);
            return true;
        }
    }
}
