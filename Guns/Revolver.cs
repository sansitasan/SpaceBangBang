namespace SpaceBangBang
{
    public class Revolver : Gun
    {
        protected override void Init()
        {
            base.Init();
            _weaponStat = GameManager.Data.WeaponDict[(int)WeaponTypes.Revolver];
        }
    }
}