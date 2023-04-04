namespace SpaceBangBang
{
    public class Machinegun : Gun
    {
        protected override void Init()
        {
            base.Init();
            WeaponType = WeaponTypes.Machinegun;
            _weaponStat = GameManager.Data.WeaponDict[(int)WeaponTypes.Machinegun];
        }
    }
}