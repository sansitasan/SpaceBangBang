namespace SpaceBangBang
{
    public class Sniper : Gun
    {
        protected override void Init()
        {
            base.Init();
            WeaponType = WeaponTypes.Sniper;
            _weaponStat = GameManager.Data.WeaponDict[(int)WeaponTypes.Sniper];
        }
    }
}