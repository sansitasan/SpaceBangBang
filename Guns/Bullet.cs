using Photon.Pun;
using UnityEngine;

namespace SpaceBangBang
{
    public class Bullet : MonoBehaviourPun
    {
        [SerializeField]
        private float _bulletSpeed;

        [SerializeField]
        private Rigidbody2D _rigidbody2D;

        private Vector3 _dir;
        private Vector3 _startPos;

        private Coroutine _returnCoroutine; // ObjectPoolManage���� Return �ڷ�ƾ�� ��Ƽ� Ư�� �̺�Ʈ�� ���� ��Ȱ��ȭ �ƴٸ� �� �ڷ�ƾ�� �������ν� �ߺ� ��Ȱ��ȭ ����.

        public Coroutine ReturnCoroutine { get => _returnCoroutine; set => _returnCoroutine = value; }

        public void Init(Vector3 dir, Vector3 startPos)
        {
            _dir = dir;
            _startPos = startPos;
        }

        private void OnEnable()
        {
            transform.position = _startPos;
            float angle = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg - 90;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle)); // ����ȭ
            _rigidbody2D.velocity = transform.up * _bulletSpeed;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (photonView.IsMine)
            {
                if (collision.CompareTag("Player"))
                {
                    if (!collision.gameObject.GetPhotonView().IsMine)
                    {
                        collision.GetComponent<Player>().Hit();
                        ObjectPoolManager.Instance.photonView.RPC("GetEffectRPC", RpcTarget.All, EffectType.Ricochet, transform.position, _dir, 3f, PhotonNetwork.LocalPlayer);
                        ObjectPoolManager.Instance.photonView.RPC("ReturnBulletRPC", RpcTarget.AllBuffered, photonView.ViewID, PhotonNetwork.LocalPlayer);
                    }
                }
                else if (collision.CompareTag("Bullet"))
                {
                    if (!collision.GetComponent<Bullet>().photonView.IsMine)
                    {
                        ObjectPoolManager.Instance.photonView.RPC("GetEffectRPC", RpcTarget.All, EffectType.Ricochet, transform.position, _dir, 3f, PhotonNetwork.LocalPlayer);
                        ObjectPoolManager.Instance.photonView.RPC("ReturnBulletRPC", RpcTarget.AllBuffered, photonView.ViewID, PhotonNetwork.LocalPlayer);
                    }
                }
                else if (collision.CompareTag("GuardObject"))
                {
                    if (!collision.gameObject.GetPhotonView().IsMine)
                    {
                        var obj = collision.GetComponent<GuardObject>();
                        if (obj.CurrentHP > 0)
                        {
                            collision.GetComponent<GuardObject>().photonView.RPC("HitRPC", RpcTarget.All);
                            ObjectPoolManager.Instance.photonView.RPC("GetEffectRPC", RpcTarget.All, EffectType.Ricochet, transform.position, _dir, 3f, PhotonNetwork.LocalPlayer);
                            ObjectPoolManager.Instance.photonView.RPC("ReturnBulletRPC", RpcTarget.AllBuffered, photonView.ViewID, PhotonNetwork.LocalPlayer);
                        }
                    }
                }
                else if (!collision.CompareTag("ItemBox"))
                {
                    ObjectPoolManager.Instance.photonView.RPC("GetEffectRPC", RpcTarget.All, EffectType.Ricochet, transform.position, _dir, 3f, PhotonNetwork.LocalPlayer);
                    ObjectPoolManager.Instance.photonView.RPC("ReturnBulletRPC", RpcTarget.AllBuffered, photonView.ViewID, PhotonNetwork.LocalPlayer);
                }
            }
        }
    }
}