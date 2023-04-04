using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBangBang
{
    [Serializable]
    public class Stat
    {
        public int ID;
        public string Name;
    }

    [Serializable]
    public class PlayerStat : Stat
    {
        public int MaxHP;
        public int CurrentHP;
        public int Speed;
        public int MaxCard;
        public int GetCardNum;

        public int HP
        {
            get => CurrentHP; set
            {
                CurrentHP = value;
                if (CurrentHP >= MaxHP)
                    CurrentHP = MaxHP;
            }
        }

        public PlayerStat Clone()
        {
            PlayerStat clone = new PlayerStat();
            clone.ID = this.ID;
            clone.Name = this.Name;
            clone.MaxHP = this.MaxHP;
            clone.CurrentHP = this.CurrentHP;
            clone.Speed = this.Speed;
            clone.MaxCard = this.MaxCard;
            clone.GetCardNum = this.GetCardNum;

            return clone;
        }
    }

    [Serializable]
    public class StatData<T> : ILoader<int, T> where T : Stat
    {
        public List<T> Stats;

        public Dictionary<int, T> Load()
        {
            Dictionary<int, T> dict = new Dictionary<int, T>();
            for (int i = 0; i < Stats.Count; ++i)
                dict.Add(Stats[i].ID, Stats[i]);

            return dict;
        }
    }

    [Serializable]
    public class WeaponStat : Stat
    {
        public float CoolTime;
        public float Range;
        public float ChargingTime;
        public int BulletCountPerShot;
        public float Spread;
        public Sprite[] Sprites;
        public float ShakeIntensity;

        public WeaponStat Clone()
        {
            WeaponStat clone = new WeaponStat();
            clone.ID = this.ID;
            clone.Name = this.Name;
            clone.CoolTime = this.CoolTime;
            clone.Range = this.Range;
            clone.ChargingTime = this.ChargingTime;
            clone.BulletCountPerShot = this.BulletCountPerShot;
            clone.Spread = this.Spread;
            clone.Sprites = this.Sprites;
            clone.ShakeIntensity = this.ShakeIntensity;

            return clone;
        }
    }
}