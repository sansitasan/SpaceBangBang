using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering;

namespace SpaceBangBang
{
    public enum WeaponTypes
    {
        Revolver,
        Shotgun,
        K2,
        Sniper,
        Lazer,
        Machinegun,
        End
    }

    public class Gun : MonoBehaviourPun
    {
        public WeaponTypes WeaponType;

        [SerializeField]
        protected WeaponStat _weaponStat;

        [SerializeField]
        protected ParticleSystem muzzleFlashParticle;
        [SerializeField]
        protected Transform bulletHole;

        [SerializeField]
        protected AudioSource audioSource;
        [SerializeField]
        protected AudioClip shotSound;

        [SerializeField]
        protected SortingGroup sortingGroup;

        protected bool isEightDirectionShot;
        protected bool _canShot = true;

        private Vector3 Scale { get => transform.localScale; set => photonView.RPC(nameof(SetScaleRPC), RpcTarget.All, value); }
        public bool IsEightDirectionShot { get => isEightDirectionShot; }
        public bool CanShot { get => _canShot; protected set { _canShot = value; if(_canShot == false) StartCoroutine(SetCoolTimeCoroutine()); } }
        public WeaponStat WeaponStat { get => _weaponStat; }

        [PunRPC] protected void SetScaleRPC(Vector3 value) => transform.localScale = value;

        private void Awake()
        {
            Init();
        }

        protected virtual void Init()
        {
        }
#if UNITY_EDITOR
        protected virtual void Update() // 테스트용 총 발사 함수, 회전 함수
        {
            if (Input.GetKeyDown(KeyCode.A) && photonView.IsMine && CanShot)
            {
                Shot();
            }
            if (Input.GetKeyDown(KeyCode.D) && photonView.IsMine && !isEightDirectionShot)
            {
                SparyShot();
            }
        }
#endif
        public virtual bool Shot(bool hasEffect = true) // 총 발사
        {
            if (CanShot)
            {
                CanShot = false;
                if (hasEffect)
                {
                    ShakeCamera();
                    photonView.RPC("ShotEffectRPC", RpcTarget.All);
                }
                ObjectPoolManager.Instance.photonView.RPC("GetBulletRPC", RpcTarget.All, bulletHole.position, _weaponStat.Range, -bulletHole.right, PhotonNetwork.LocalPlayer);
                return true;
            }
            else
            {
                return false;
            }
        }

        private IEnumerator SetCoolTimeCoroutine()
        {
            yield return new WaitForSeconds(_weaponStat.CoolTime);
            _canShot = true;
        }

        public virtual void SparyShot()
        {
            isEightDirectionShot = true;
            float initialAngle = transform.rotation.eulerAngles.z - 180;
            float temp = initialAngle - 45;

            for (int i = 0; i < 8; i++)
            {
                temp += 45;
                if (temp > 180)
                {
                    temp -= 360;
                }

                photonView.RPC("RotateGunRPC", RpcTarget.All, temp);
                CanShot = true;
                Shot(i == 0);
            }

            photonView.RPC("RotateGunRPC", RpcTarget.All, initialAngle);
            isEightDirectionShot = false;
        }

        [PunRPC]
        public virtual void RotateGunRPC(float angle) // 총 회전 함수
        {

            transform.rotation = Quaternion.AngleAxis(angle + 180, Vector3.forward);

            if (angle > 0)
            {
                sortingGroup.sortingOrder = 1;
            }
            else
            {
                sortingGroup.sortingOrder = 3;
            }

            if (angle > 90 || angle < -90)
            {
                Scale = new Vector3(transform.localScale.x, Mathf.Abs(transform.localScale.y), transform.localScale.z);
            }
            else
            {
                Scale = new Vector3(transform.localScale.x, -1 * Mathf.Abs(transform.localScale.y), transform.localScale.z);
            }

        }

        // 사운드, 파티클 이펙트 출력 함수
        [PunRPC]
        protected void ShotEffectRPC()
        {
            audioSource.PlayOneShot(shotSound);
            muzzleFlashParticle.Emit(1);
        }

        protected void ShakeCamera()
        {
            CameraShake.Instance.ShakeCamera(_weaponStat.ShakeIntensity);
        }

        protected virtual void OnEnable()
        {
            CanShot = true;
        }
    }
}