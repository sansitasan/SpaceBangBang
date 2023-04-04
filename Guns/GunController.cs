using Photon.Pun;
using System.Collections;
using UnityEngine;
namespace SpaceBangBang
{
    public class GunController : MonoBehaviourPun
    {
        private WeaponTypes _weaponType;

        private Gun _gun;
        private WeaponStat _weaponStat;
        [SerializeField]
        private ParticleSystem _muzzleFlashParticle;
        [SerializeField]
        private Transform _bulletHole;

        [SerializeField]
        private AudioSource _audioSource;

        private bool _isEightDirectionShot;

        private Vector3 Scale { get => transform.localScale; set => photonView.RPC(nameof(SetScaleRPC), RpcTarget.All, value); }
        public bool IsEightDirectionShot { get => _isEightDirectionShot; }

        [PunRPC] protected void SetScaleRPC(Vector3 value) => transform.localScale = value;

        void Start()
        {
            _audioSource = gameObject.AddComponent<AudioSource>(); // 임시 오디오 소스
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Shot() // 총 발사
        {
            photonView.RPC("ShotEffectRPC", RpcTarget.All);
            ObjectPoolManager.Instance.photonView.RPC("GetBulletRPC", RpcTarget.All, _bulletHole.position, _weaponStat.Range, -_bulletHole.right, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        public virtual IEnumerator ShotEightDirectionCoroutine() // 8방향으로 쏘는 함수
        {
            _isEightDirectionShot = true;
            float initialAngle = transform.rotation.eulerAngles.z + 180;

            for (int i = 0; i < 8; i++)
            {
                RotateGun(initialAngle + i * (360f / 8f));
                Shot();
                yield return GameManager.Instance.Wfs(5);
            }

            _isEightDirectionShot = false;
        }

        public void RotateGun(float angle) // 총 회전 함수
        {
            if (photonView.IsMine)
            {
                transform.rotation = Quaternion.AngleAxis(angle + 180, Vector3.forward);

                if (angle > 0)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y, 1);
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y, -1);
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
        }

        // 사운드, 파티클 이펙트 출력 함수
        [PunRPC]
        private void ShotEffectRPC()
        {
            _muzzleFlashParticle.Emit(1);
        }

        [PunRPC]
        public void ChangeGunRPC(WeaponTypes weapontype)
        {
            _weaponStat = GameManager.Data.WeaponDict[(int)weapontype];
            _weaponType = weapontype;
            _muzzleFlashParticle = _bulletHole.transform.GetChild((int)weapontype).GetComponent<ParticleSystem>();
        }
    }
}