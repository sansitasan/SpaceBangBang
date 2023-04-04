using Photon.Pun;

namespace SpaceBangBang
{
    public class ShotgunCard : ICard
    {
        public ShotgunCard(CardType t)
        {
            Type = t;
        }
        public CardType Type { get; set; }
        public bool UseCard(Player p)
        {
            p.EquipWeapon(WeaponTypes.Shotgun, PhotonNetwork.LocalPlayer);
            return true;
        }
    }
}