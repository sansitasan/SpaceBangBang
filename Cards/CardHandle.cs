using Photon.Pun;
using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SpaceBangBang
{
    [Serializable]
    public class CardHandle : MonoBehaviourPun
    {
        [SerializeField] private int _handSize;
        [SerializeField] private int _curHandSize;
        [SerializeField] private List<ICard> _handCardList;
        public CardType cardType;
        public CardHandlUI _cardHandlUI;
        public Player _p;
        private IDisposable draw;

#if UNITY_EDITOR
        private void Update()
        {
            if (photonView.IsMine)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    photonView.RPC("DrawCard", RpcTarget.All, new int[1] { 0 }, 1, true);
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                    photonView.RPC("DrawCard", RpcTarget.All, new int[1] { 21 }, 1, true);
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                    photonView.RPC("DrawCard", RpcTarget.All, new int[1] { 36 }, 1, true);
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                    photonView.RPC("DrawCard", RpcTarget.All, new int[1] { 41 }, 1, true);
                else if (Input.GetKeyDown(KeyCode.Alpha5))
                    photonView.RPC("DrawCard", RpcTarget.All, new int[1] { 43 }, 1, true);
                else if (Input.GetKeyDown(KeyCode.Alpha6))
                    photonView.RPC("DrawCard", RpcTarget.All, new int[1] { 46 }, 1, true);
                else if (Input.GetKeyDown(KeyCode.Alpha7))
                    photonView.RPC("DrawCard", RpcTarget.All, new int[1] { 47 }, 1, true);
                else if (Input.GetKeyDown(KeyCode.Alpha8))
                    photonView.RPC("DrawCard", RpcTarget.All, new int[1] { 50 }, 1, true);
                else if (Input.GetKeyDown(KeyCode.Alpha9))
                    photonView.RPC("DrawCard", RpcTarget.All, new int[1] { 55 }, 1, true);
                else if (Input.GetKeyDown(KeyCode.Q))
                    photonView.RPC("DrawCard", RpcTarget.All, new int[1] { 56 }, 1, true);
                else if (Input.GetKeyDown(KeyCode.W))
                    photonView.RPC("DrawCard", RpcTarget.All, new int[1] { 57 }, 1, true);
                else if (Input.GetKeyDown(KeyCode.E))
                    photonView.RPC("DrawCard", RpcTarget.All, new int[1] { 59 }, 1, true);
                else if (Input.GetKeyDown(KeyCode.O))
                    photonView.RPC("RemoveCardRPC", RpcTarget.All, 0);
                else if (Input.GetKeyDown(KeyCode.S))
                    Draw();
            }
        }
#endif

        public void Init(Player p, int handSize, CardHandlUI cui)
        {
            _handCardList = new List<ICard>();
            _handSize = handSize;
            _p = p;
            p.Clear -= Clear;
            p.Clear += Clear;

            if (photonView.IsMine)
            {
                _cardHandlUI = cui;

                Draw(handSize);
                draw = Observable
                    .Timer(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10f / p._stat.GetCardNum))
                    .Subscribe(_ => Draw(1, false));
            }
        }

        [PunRPC]
        public void DrawCard(int[] cardnum, int cnt = 1, bool bCard = true)
        {
            for (int i = 0; i < cnt; ++i)
            {
                _curHandSize++;
                if (_curHandSize > _handSize && !bCard && photonView.IsMine)
                    photonView.RPC("RemoveCardRPC", RpcTarget.All, 0);
                _p.cAudio.PlaySound(_p.clips[(int)ClipType.CardDraw], Sound.Effect);
                _handCardList.Add(CardsManager.Instance.excute(cardnum[i]));
                _cardHandlUI?.AddCardToHand(_handCardList[_handCardList.Count - 1]);
            }
        }

        public void Draw(int cnt = 1, bool bCard = true)
        {
            int[] card = new int[cnt];
            for (int i = 0; i < cnt; ++i)
                card[i] = Random.Range(0, 60);
            photonView.RPC("DrawCard", RpcTarget.All, card, cnt, bCard);
        }

        [PunRPC]
        public bool GetCardRPC(CardType cardType)
        {
            ICard card = CardsManager.Instance.excute(cardType);

            _handCardList.Add(card);
            _cardHandlUI?.AddCardToHand(card);
            _curHandSize++;
            return true;
        }

        public CardType GetFstCard(int cnt = 0)
        {
            if (_handCardList.Count > cnt)
                return _handCardList[cnt].Type;
            return CardType.None;
        }

        public List<ICard> GetHandCardList()
        {
            return _handCardList;
        }

        public ICard DisCard()
        {
            if (_handCardList == null || _curHandSize == 0)
            {
                return null;
            }

            ICard temp = CardsManager.Instance.excute(_handCardList[0].Type);
            photonView.RPC("RemoveCardRPC", RpcTarget.All, 0);

            return temp;
        }

        public void RemovePassiveCard(CardType cardType)
        {
            for (int i = 0; i < _curHandSize; i++)
                if (_handCardList[i].Type == cardType)
                {
                    RemoveCardRPC(i);
                    return;
                }
        }

        public CardType FindPassiveCard()
        {
            if (_handCardList == null || _curHandSize == 0)
            {
                return CardType.None;
            }

            for (int i = 0; i < _curHandSize; i++)
            {
                if (_handCardList[i].Type == CardType.Dodge)
                {
                    RemoveCardRPC(i);
                    return CardType.Dodge;
                }
            }

            return CardType.None;
        }

        [PunRPC]
        public void RemoveCardRPC(int index)
        {
            if (_curHandSize == 0)
                return;
            _cardHandlUI?.RemoveCardFromHand(index);
            _handCardList.RemoveAt(index);
            _curHandSize--;
        }

        public CardType UseCard(int n)
        {
            if (_handCardList == null || _curHandSize == 0)
                return CardType.None;

            CardType temp = _handCardList[n].Type;
            if ((CardsManager.Instance.excute(_handCardList[n].Type)).UseCard(_p))
            {
                photonView.RPC("RemoveCardRPC", RpcTarget.All, n);
            }
            return temp;
        }

        private void Clear(Player p)
        {
            p.Clear -= Clear;
            if (photonView.IsMine)  
                draw.Dispose();
        }
    }
}