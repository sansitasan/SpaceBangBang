using Photon.Pun;

namespace SpaceBangBang
{
    public class GuardCard : ICard
    {
        public GuardCard(CardType t)
        {
            Type = t;
        }
        public CardType Type { get; set; }
        public bool UseCard(Player p)
        {
            ObjectPoolManager.Instance.photonView.RPC("GetGuardObjectRPC", RpcTarget.All, p.SetGuardObjectPos(), 25f, PhotonNetwork.LocalPlayer);
            return true;
        }
    }
}