using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace SpaceBangBang
{
    public enum CardType
    {
        Bang,
        Dodge,
        Guard,
        Steal,
        Recorvery,
        Supply,
        Spray,
        Lazer,
        Revolver,
        Shotgun,
        Sniper,
        Machinegun,
        Scope,
        None
    }
    public class CardsManager
    {
        private static CardsManager instance;

        Dictionary<CardType, Sprite> cardSprites = new Dictionary<CardType, Sprite>();
        public static CardsManager Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new CardsManager();
                    ImgSet();
                }
                return instance;
            }
        }

        public static void ImgSet()
        {
            SpriteAtlas atlas = Resources.Load<SpriteAtlas>("Sprite/Cards");
            instance.cardSprites.Add(CardType.Bang, atlas.GetSprite("BangCard"));
            instance.cardSprites.Add(CardType.Dodge, atlas.GetSprite("DodgeCard"));
            instance.cardSprites.Add(CardType.Guard, atlas.GetSprite("GuardCard"));
            instance.cardSprites.Add(CardType.Steal, atlas.GetSprite("StealCard"));
            instance.cardSprites.Add(CardType.Recorvery, atlas.GetSprite("RecoveryCard"));
            instance.cardSprites.Add(CardType.Supply, atlas.GetSprite("SupplyCard"));
            instance.cardSprites.Add(CardType.Spray, atlas.GetSprite("SprayCard"));// 수정
            instance.cardSprites.Add(CardType.Lazer, atlas.GetSprite("LaserGunCard"));
            //instance.cardSprites.Add(CardType.Revolver, Resources.Load<Sprite>("Sprite/RevolverCard")); // 수정
            instance.cardSprites.Add(CardType.Shotgun, atlas.GetSprite("ShotgunCard"));
            instance.cardSprites.Add(CardType.Sniper, atlas.GetSprite("SniperCard"));
            instance.cardSprites.Add(CardType.Machinegun, atlas.GetSprite("MachineGunCard"));
            instance.cardSprites.Add(CardType.Scope, atlas.GetSprite("ScopeCard"));
        }

        public ICard excute(CardType t)
        {
            return ReturnCard(t);
        }

        public ICard excute(int t)
        {
            if (0 <= t && t <= 20)
                return ReturnCard(CardType.Bang);
            else if (20 < t && t <= 32)
                return ReturnCard(CardType.Dodge);
            else if (32 < t && t <= 36)
                return ReturnCard(CardType.Supply);
            else if (36 < t && t <= 41)
                return ReturnCard(CardType.Spray);
            else if (41 < t && t <= 43)
                return ReturnCard(CardType.Sniper);
            else if (43 < t && t <= 46)
                return ReturnCard(CardType.Shotgun);
            else if (46 < t && t <= 47)
                return ReturnCard(CardType.Lazer);
            else if (47 < t && t <= 50)
                return ReturnCard(CardType.Steal);
            else if (50 < t && t <= 55)
                return ReturnCard(CardType.Recorvery);
            else if (55 < t && t <= 56)
                return ReturnCard(CardType.Machinegun);
            else if (56 < t && t <= 57)
                return ReturnCard(CardType.Scope);
            else if (57 < t && t <= 59)
                return ReturnCard(CardType.Guard);
            else
                return null;
        }

        public ICard ReturnCard(CardType t)
        {
            switch (t)
            {
                case CardType.Bang:
                    return new BangCard(t);
                case CardType.Dodge:
                    return new DodgeCard(t);
                case CardType.Guard:
                    return new GuardCard(t);
                case CardType.Steal:
                    return new StealCard(t);
                case CardType.Recorvery:
                    return new RecorveryCard(t);
                case CardType.Supply:
                    return new SupplyCard(t);
                case CardType.Spray:
                    return new SprayCard(t);
                case CardType.Lazer:
                    return new LazerCard(t);
                case CardType.Revolver:
                    return new RevolverCard(t);
                case CardType.Shotgun:
                    return new ShotgunCard(t);
                case CardType.Sniper:
                    return new SniperCard(t);
                case CardType.Scope:
                    return new ScopeCard(t);
                case CardType.Machinegun:
                    return new MachinegunCard(t);
            }
            return null;
        }

        public CardType ICardToCardType(ICard card)
        {
            switch (card)
            {
                case BangCard:
                    return CardType.Bang;
                case DodgeCard:
                    return CardType.Dodge;
                case GuardCard:
                    return CardType.Guard;
                case StealCard:
                    return CardType.Steal;
                case RecorveryCard:
                    return CardType.Recorvery;
                case SupplyCard:
                    return CardType.Supply;
                case SprayCard:
                    return CardType.Spray;
                case LazerCard:
                    return CardType.Lazer;
                case RevolverCard:
                    return CardType.Revolver;
                case ShotgunCard:
                    return CardType.Shotgun;
                case SniperCard:
                    return CardType.Sniper;
                case MachinegunCard:
                    return CardType.Machinegun;
                case ScopeCard:
                    return CardType.Scope;
                default:
                    return CardType.None;
            }
        }

        public Sprite GetSprite(ICard card)
        {
            if (instance.cardSprites[card.Type] != null)
                return instance.cardSprites[card.Type];
            else
                return null;

        }
    }
}