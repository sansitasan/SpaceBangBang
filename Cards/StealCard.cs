using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace SpaceBangBang
{
    public class StealCard : ICard
    {
        public StealCard(CardType t)
        {
            Type = t;
        }
        public CardType Type { get; set; }

        public bool UseCard(Player p)
        {
            //수정요함
            List<Player> otherPlayers = NetworkManager.Instance.alivePlayers.ToList().FindAll(x => x != p);
            if (otherPlayers == null || otherPlayers.Count == 0)
            {
                return true;
            }

            do
            {
                Player otherPlayer = otherPlayers[Random.Range(0, otherPlayers.Count)];
                ICard temp = otherPlayer._cardHandle.DisCard();

                if (temp != null)
                {
                    ObjectPoolManager.Instance.photonView.RPC("GetEffectRPC", RpcTarget.All, EffectType.StealText, otherPlayer.transform.position, Vector3.zero, 10f, PhotonNetwork.LocalPlayer);
                    p._cardHandle.photonView.RPC("GetCardRPC", RpcTarget.All, CardsManager.Instance.ICardToCardType(temp));
                    break;
                }
                else
                {
                    otherPlayers.Remove(otherPlayer);
                    if (otherPlayers.Count == 0)
                    {
                        p._cardHandle.Draw(1);
                        break;
                    }
                }
            }
            while (otherPlayers.Count > 0);

            return true;
        }
    }
}