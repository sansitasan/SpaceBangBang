namespace SpaceBangBang
{
    public class Lazer : LazerGun
    {
        protected override void Init()
        {
            base.Init();
            WeaponType = WeaponTypes.Lazer;
            _weaponStat = GameManager.Data.WeaponDict[(int)WeaponTypes.Lazer];
        }
    }
}