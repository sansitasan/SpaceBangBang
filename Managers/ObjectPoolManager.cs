using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;
using DG.Tweening;

namespace SpaceBangBang
{
    public enum EffectType
    {
        Ricochet,
        DropGun,
        DodgeText,
        StealText
    }

    public class ObjectPoolManager : MonoBehaviourPun
    {
        public static ObjectPoolManager Instance;

        [SerializeField]
        private int objectPoolCount; // 초기 생성 오브제트 개수

        private Queue<GameObject> poolingRicochetQueue = new Queue<GameObject>();
        private Queue<GameObject> poolingDropGun = new Queue<GameObject>();
        private Queue<GameObject> poolingTextQueue = new Queue<GameObject>();

        private Dictionary<EffectType, Queue<GameObject>> poolingParticleDictionary = new Dictionary<EffectType, Queue<GameObject>>(); // 파티클 이름에 해당하는 큐 리턴하는 딕셔너리

        private Dictionary<int, Bullet> bulletViewIdDictionry = new Dictionary<int, Bullet>(); // viewID에 해당하는 bullet 클래스 리턴 (getComponent 최소화로 최적화하기 위한 목적)
        private Dictionary<int, Gun> gunViewIdDictionary = new Dictionary<int, Gun>();
        private Dictionary<int, GuardObject> guardObjectViewIdDictionary = new Dictionary<int, GuardObject>();

        // 플레이어마다 소유하는 풀링된 오브젝트들에 접근하는 딕셔너리.
        private Dictionary<Photon.Realtime.Player, Queue<Bullet>> poolingBullet = new Dictionary<Photon.Realtime.Player, Queue<Bullet>>();
        private Dictionary<Photon.Realtime.Player, List<Gun>> poolingGun = new Dictionary<Photon.Realtime.Player, List<Gun>>();
        private Dictionary<Photon.Realtime.Player, Queue<GuardObject>> poolingGuardObject = new Dictionary<Photon.Realtime.Player, Queue<GuardObject>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void Initialize()
        {
            poolingParticleDictionary.Add(EffectType.Ricochet, poolingRicochetQueue);
            poolingParticleDictionary.Add(EffectType.DropGun, poolingDropGun);
            poolingParticleDictionary.Add(EffectType.DodgeText, poolingTextQueue);
            poolingParticleDictionary.Add(EffectType.StealText, poolingTextQueue);

            Queue<Bullet> poolingBulletQueue = new Queue<Bullet>();
            List<Gun> poolingGunList = new List<Gun>();
            Queue<GuardObject> poolingGuardQueue = new Queue<GuardObject>();

            int[] bulletIDs = new int[objectPoolCount];
            int[] guardObjectIDs = new int[5];

            for (int i = 0; i < objectPoolCount; i++)
            {
                bulletIDs[i] = PhotonNetwork.Instantiate("Prefabs/Gun/TestBullet", Vector3.zero, Quaternion.identity).GetPhotonView().ViewID;
                EnqueueParticle(Instantiate(Resources.Load("Prefabs/Effect/Ricochet"), Vector3.zero, Quaternion.identity) as GameObject, EffectType.Ricochet);
            }

            for (int i = 0; i < 15; i++)
            {
                EnqueueParticle(Instantiate(Resources.Load("Prefabs/Effect/DropGun"), Vector3.zero, Quaternion.identity) as GameObject, EffectType.DropGun);
                EnqueueParticle(Instantiate(Resources.Load("Prefabs/Effect/TextEffect"), Vector3.zero, Quaternion.identity) as GameObject, EffectType.DodgeText);
            }

            for (int i = 0; i < 5; i++)
            {
                guardObjectIDs[i] = PhotonNetwork.Instantiate("Prefabs/Map/GuardObject", Vector3.zero, Quaternion.identity).GetPhotonView().ViewID;
            }

            photonView.RPC("EnqueueBulletRPC", RpcTarget.All, PhotonNetwork.LocalPlayer, bulletIDs);
            photonView.RPC("EnqueueGuardObjectRPC", RpcTarget.All, PhotonNetwork.LocalPlayer, guardObjectIDs);

            int[] gunIDs = new int[(int)WeaponTypes.Machinegun];

            for (int i = 0; i < (int)WeaponTypes.Machinegun; i++)
            {
                gunIDs[i] = PhotonNetwork.Instantiate("Prefabs/Gun/" + ((WeaponTypes)i).ToString(), Vector3.zero, Quaternion.identity).GetPhotonView().ViewID;
            }

            photonView.RPC("AddGunRPC", RpcTarget.All, PhotonNetwork.LocalPlayer, gunIDs);
        }

        #region Bullet
        [PunRPC]
        private void EnqueueBulletRPC(Photon.Realtime.Player player, int[] ids) // 오브젝트 큐에 삽입
        {
            Queue<Bullet> bulletQueue = poolingBullet.ContainsKey(player) ? poolingBullet[player] : new Queue<Bullet>();

            for (int i = 0; i < ids.Length; i++)
            {
                var newObj = PhotonView.Find(ids[i]).GetComponent<Bullet>();
                bulletViewIdDictionry.Add(ids[i], newObj);
                newObj.gameObject.SetActive(false);
                newObj.transform.SetParent(Instance.transform);
                bulletQueue.Enqueue(newObj);
            }

            if (!poolingBullet.ContainsKey(player)) poolingBullet.Add(player, bulletQueue);
        }

        [PunRPC]
        public void GetBulletRPC(Vector3 startPos, float range, Vector3 dir, Photon.Realtime.Player player) // 풀링된 오브젝트 활성화
        {
            if (poolingBullet[player].Count > 0)
            {
                var obj = poolingBullet[player].Dequeue();
                obj.Init(dir, startPos);
                obj.gameObject.SetActive(true);

                if (PhotonNetwork.IsMasterClient)
                    obj.ReturnCoroutine = StartCoroutine(ReturnBulletCoroutine(obj.photonView.ViewID, range, player));
            }
            else
            {
                if (PhotonNetwork.LocalPlayer == player)
                {
                    photonView.RPC("EnqueueBulletRPC", RpcTarget.All, player, new int[] { PhotonNetwork.Instantiate("Prefabs/Gun/TestBullet", Vector3.zero, Quaternion.identity).GetPhotonView().ViewID });
                    photonView.RPC("GetBulletRPC", RpcTarget.All, startPos, range, dir, player);
                }
            }
        }

        [PunRPC]
        public void ReturnBulletRPC(int id, Photon.Realtime.Player player)
        {
            Bullet obj = bulletViewIdDictionry[id];
            if (PhotonNetwork.IsMasterClient)
                StopCoroutine(obj.ReturnCoroutine);
            if (obj != null || obj.gameObject.activeSelf)
            {
                obj.gameObject.SetActive(false);
                Instance.poolingBullet[player].Enqueue(obj);
            }
        }

        IEnumerator ReturnBulletCoroutine(int id, float range, Photon.Realtime.Player player)
        {
            yield return GameManager.Instance.Wfs((int)(range * 100));

            photonView.RPC("ReturnBulletRPC", RpcTarget.AllBuffered, id, player);
        }
        #endregion

        #region Effect
        private void EnqueueParticle(GameObject obj, EffectType type)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(Instance.transform);
            poolingParticleDictionary[type].Enqueue(obj);
        }

        [PunRPC]
        public void GetEffectRPC(EffectType type, Vector3 pos, Vector3 dir, float duration, Photon.Realtime.Player player) // 풀링된 오브젝트 활성화
        {
            if (Instance.poolingParticleDictionary[type].Count > 0)
            {
                var obj = Instance.poolingParticleDictionary[type].Dequeue();

                obj.transform.position = pos;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 180;
                obj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle)); // 동기화

                switch (type)
                {
                    case EffectType.Ricochet:
                        obj.transform.position += obj.transform.right * 1.5f;
                        break;
                    case EffectType.DropGun:
                        if (((Player)player.TagObject).gun == null)
                        {
                            return;
                        }
                        obj.GetComponent<DropGun>().Init(((Player)player.TagObject).gun.WeaponType);
                        break;
                    case EffectType.DodgeText:
                    case EffectType.StealText:
                        obj.GetComponent<TextEffect>().Init(type);
                        obj.transform.position += Vector3.left * Random.Range(-1.5f, 1.5f);
                        obj.transform.rotation = Quaternion.identity;
                        break;
                    default:
                        break;
                }

                obj.gameObject.SetActive(true);

                StartCoroutine(ReturnParticleCoroutine(type, obj, duration));
            }
            else
            {
                if (type == EffectType.StealText || type == EffectType.DodgeText)
                {
                    EnqueueParticle(Instantiate(Resources.Load("Prefabs/Effect/TextEffect"), Vector3.zero, Quaternion.identity) as GameObject, type);
                }
                else
                {
                    EnqueueParticle(Instantiate(Resources.Load("Prefabs/Effect/" + type.ToString()), Vector3.zero, Quaternion.identity) as GameObject, type);
                }
                if (PhotonNetwork.IsMasterClient) photonView.RPC("GetEffectRPC", RpcTarget.All, type, pos, dir, duration, player);
            }
        }

        public void ReturnParticle(EffectType type, GameObject obj)
        {
            if (obj != null || obj.gameObject.activeSelf)
            {
                obj.gameObject.SetActive(false);
                Instance.poolingParticleDictionary[type].Enqueue(obj);
            }
        }

        IEnumerator ReturnParticleCoroutine(EffectType type, GameObject obj, float duration)
        {
            yield return GameManager.Instance.Wfs((int)(duration * 100));

            ReturnParticle(type, obj);
        }
        #endregion

        #region Gun
        [PunRPC]
        private void AddGunRPC(Photon.Realtime.Player player, int[] ids)
        {
            List<Gun> gunList = poolingGun.ContainsKey(player) ? poolingGun[player] : new List<Gun>();

            for (int i = 0; i < ids.Length; i++)
            {
                var newObj = PhotonView.Find(ids[i]).GetComponent<Gun>();
                gunViewIdDictionary.Add(ids[i], newObj);
                newObj.gameObject.SetActive(false);
                newObj.transform.SetParent(Instance.transform);
                gunList.Add(newObj);
            }

            if (!poolingGun.ContainsKey(player))
            {
                poolingGun.Add(player, gunList);
            }
        }

        [PunRPC]
        public void GetGunRPC(WeaponTypes weaponType, Photon.Realtime.Player player) // 풀링된 오브젝트 활성화
        {
            var gun = poolingGun[player].Find(x => x.WeaponStat.Name == weaponType.ToString());

            if (gun != null)
            {
                poolingGun[player].Remove(gun);
                gun.transform.SetParent(((Player)player.TagObject).transform);
                gun.transform.localPosition = new Vector3(0, -0.05f, 0);
                gun.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                gun.gameObject.SetActive(true);
            }
            else
            {
                if (PhotonNetwork.LocalPlayer == player)
                {
                    photonView.RPC("AddGunRPC", RpcTarget.AllBuffered, player, new int[] { PhotonNetwork.Instantiate("Prefabs/Gun/" + weaponType.ToString(), Vector3.zero, Quaternion.identity).GetPhotonView().ViewID });
                    photonView.RPC("GetGunRPC", RpcTarget.AllBuffered, weaponType, player);
                }
            }
        }

        [PunRPC]
        public void ReturnGunRPC(int id, Photon.Realtime.Player player)
        {
            Gun gun = gunViewIdDictionary[id];
            if (gun != null || gun.gameObject.activeSelf)
            {
                gun.gameObject.SetActive(false);
                gun.transform.SetParent(Instance.transform);
                poolingGun[player].Add(gun);
            }
        }
        #endregion

        #region GuardObject
        [PunRPC]
        private void EnqueueGuardObjectRPC(Photon.Realtime.Player player, int[] ids)
        {
            Queue<GuardObject> guardObjectQueue = poolingGuardObject.ContainsKey(player) ? poolingGuardObject[player] : new Queue<GuardObject>();

            for (int i = 0; i < ids.Length; i++)
            {
                var newObj = PhotonView.Find(ids[i]).GetComponent<GuardObject>();
                guardObjectViewIdDictionary.Add(ids[i], newObj);
                newObj.gameObject.SetActive(false);
                newObj.transform.SetParent(Instance.transform);
                guardObjectQueue.Enqueue(newObj);
            }

            if (!poolingGuardObject.ContainsKey(player)) poolingGuardObject.Add(player, guardObjectQueue);
        }

        [PunRPC]
        public void GetGuardObjectRPC(Vector3 startPos, float aliveTime, Photon.Realtime.Player player)
        {
            if (poolingGuardObject[player].Count > 0)
            {
                var obj = poolingGuardObject[player].Dequeue();
                obj.Init(startPos, player);
                obj.gameObject.SetActive(true);

                if (PhotonNetwork.IsMasterClient)
                    obj.ReturnCoroutine = StartCoroutine(ReturnGuardObjectCoroutine(obj.photonView.ViewID, aliveTime, player));
            }
            else
            {
                if (PhotonNetwork.LocalPlayer == player)
                {
                    photonView.RPC("EnqueueGuardObjectRPC", RpcTarget.All, player, new int[] { PhotonNetwork.Instantiate("Prefabs/Map/GuardObject", Vector3.zero, Quaternion.identity).GetPhotonView().ViewID });
                    photonView.RPC("GetGuardObjectRPC", RpcTarget.All, startPos, aliveTime, player);
                }
            }
        }

        [PunRPC]
        public void ReturnGuardObjectRPC(int id, Photon.Realtime.Player player)
        {
            GuardObject obj = guardObjectViewIdDictionary[id];
            if (PhotonNetwork.IsMasterClient)
                StopCoroutine(obj.ReturnCoroutine);
            if (obj != null || obj.gameObject.activeSelf)
            {
                obj.SpriteRenderer.DOFade(0, 1).OnComplete(() =>
                {
                    obj.gameObject.SetActive(false);
                    Instance.poolingGuardObject[player].Enqueue(obj);
                });
            }
        }

        IEnumerator ReturnGuardObjectCoroutine(int id, float aliveTime, Photon.Realtime.Player player)
        {
            yield return GameManager.Instance.Wfs((int)(aliveTime * 100));

            photonView.RPC("ReturnGuardObjectRPC", RpcTarget.AllBuffered, id, player);
        }
        #endregion
    }
}