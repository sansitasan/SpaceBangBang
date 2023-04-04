using UnityEngine;
using Photon.Pun;
namespace SpaceBangBang
{
    public class Shotgun : Gun
    {
        protected override void Init()
        {
            base.Init();
            WeaponType = WeaponTypes.Shotgun;
            _weaponStat = GameManager.Data.WeaponDict[(int)WeaponTypes.Shotgun];
        }

        public override bool Shot(bool hasEffect = true)
        {
            if (CanShot)
            {
                CanShot = false;

                if (hasEffect)
                {
                    photonView.RPC("ShotEffectRPC", RpcTarget.All);
                    ShakeCamera();
                }

                float initialAngle = Mathf.Atan2(-bulletHole.right.y, -bulletHole.right.x) * Mathf.Rad2Deg;
                float angleTransition = _weaponStat.Spread / _weaponStat.BulletCountPerShot;

                for (int i = 0; i < _weaponStat.BulletCountPerShot; i++)
                {
                    float angle = initialAngle - _weaponStat.Spread / 2 + angleTransition * i;

                    Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);

                    ObjectPoolManager.Instance.photonView.RPC("GetBulletRPC", RpcTarget.All, bulletHole.position, _weaponStat.Range, dir, PhotonNetwork.LocalPlayer);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}