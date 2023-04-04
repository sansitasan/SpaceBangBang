namespace SpaceBangBang
{
    public class K2 : Gun
    {
        protected override void Init()
        {
            base.Init();
            WeaponType = WeaponTypes.K2;
            _weaponStat = GameManager.Data.WeaponDict[(int)WeaponTypes.K2];
        }
    }
}