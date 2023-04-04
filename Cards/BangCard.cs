using UnityEngine;

namespace SpaceBangBang
{
    public class BangCard : ICard
    {
        public BangCard(CardType t)
        {
            Type = t;
        }

        public CardType Type { get; set; }

        public bool UseCard(Player p)
        {
            if (p.gun.WeaponType == WeaponTypes.Machinegun)
            {
                if (Random.Range(0f, 1f) < 0.5f)
                {
                    return false;
                }
            }
            return true;
        }
    }
}