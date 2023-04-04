using Photon.Pun;

namespace SpaceBangBang
{
    public class MachinegunCard : ICard
    {
        // Start is called before the first frame update
        public MachinegunCard(CardType t)
        {
            Type = t;
        }
        public CardType Type { get; set; }
        public bool UseCard(Player p)
        {
            p.EquipWeapon(WeaponTypes.Machinegun, PhotonNetwork.LocalPlayer);
            return true;
        }
    }
}