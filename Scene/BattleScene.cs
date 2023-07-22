using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cinemachine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace SpaceBangBang
{
    public class BattleScene : MonoBehaviourPun
    {
        [SerializeField]
        private MapController mc;
        [SerializeField]
        private Volume volume;
        [SerializeField]
        private CinemachineVirtualCamera productionCM;
        [SerializeField]
        private CinemachineVirtualCamera followCM;
        [SerializeField]
        private BattleSceneTouchPanel _touchPanel;

        private LensDistortion lensDistortion;
        private MotionBlur motionBlur;
        [SerializeField]
        private Transform t;
        private int playerIndex = 0; // 관전할 때 사용하는 인덱스
        private WinPanelUI winPanel;
        private DrawPanelUI drawPanel;
        private Player player;

        [SerializeField]
        private Image ItemBoxImage;

        void Start()
        {
            ObjectPoolManager.Instance.Initialize();
            Hashtable CP = PhotonNetwork.LocalPlayer.CustomProperties;
            CP["isLoadingDone"] = true;
            PhotonNetwork.LocalPlayer.SetCustomProperties(CP);
            StartCoroutine(StartProductionCoroutine());
        }

        private void Init()
        {
            GameManager.Instance.timerText.gameObject.SetActive(false);
            PhotonNetwork.CurrentRoom.CustomProperties["startTime"] = 0.0;
            player = PhotonNetwork.Instantiate("Prefabs/Player/" + ((CharacterType)((int)PhotonNetwork.LocalPlayer.CustomProperties["Character"])).ToString(),
                mc.Spawnpos[PhotonNetwork.LocalPlayer.ActorNumber - 1].position, Quaternion.identity).GetComponent<Player>();
            NetworkManager.Instance.EndBattle -= EndBattle;
            NetworkManager.Instance.EndBattle += EndBattle;
            NetworkManager.Instance.LeftPlayer -= ChangeFollowingCam;
            NetworkManager.Instance.LeftPlayer += ChangeFollowingCam;

            _touchPanel.Touch -= ChangeFollowingCam;
            _touchPanel.Touch += ChangeFollowingCam;
            _touchPanel.gameObject.SetActive(false);
            
            winPanel = Resources.Load<WinPanelUI>("Prefabs/UI/WinPanelCanvas");
            drawPanel = Resources.Load<DrawPanelUI>("Prefabs/UI/DrawPanelCanvas");
        }

        // 카메라 연출
        private IEnumerator StartProductionCoroutine()
        {
            List<Photon.Realtime.Player> players = PhotonNetwork.PlayerList.ToList();

            while (players.Find(x => (bool)x.CustomProperties["isLoadingDone"] == false) != null)
            {
                yield return null;
            }

            Init();

            Player player = (Player)PhotonNetwork.LocalPlayer.TagObject;
            player._camera = followCM;
            followCM.Follow = player.transform;
            followCM.LookAt = player.transform;
            followCM.m_Lens.OrthographicSize = 20;
            volume.profile.TryGet(out lensDistortion);
            volume.profile.TryGet(out motionBlur);
            
            DOTween.Sequence()
                .Append(t.DOMoveY(-110, 5).SetEase(Ease.InOutQuart).SetRelative())
                .AppendInterval(1f)
                .Append(t.DOMoveX(player.transform.position.x, 0.7f).SetEase(Ease.InOutQuart))
                .Join(t.DOMoveY(player.transform.position.y, 0.7f).SetEase(Ease.InOutQuart))
                .Join(productionCM.transform.DOShakePosition(0.7f, 10, 15).SetEase(Ease.InOutQuart).SetRelative())
                .Join(DOTween.To(() => productionCM.m_Lens.OrthographicSize, x => productionCM.m_Lens.OrthographicSize = x, 10, 0.7f).SetEase(Ease.InOutQuart)
                .OnStart(() => productionCM.GetComponent<CinemachineConfiner2D>().enabled = true))
                .Join(DOTween.To(() => lensDistortion.intensity.value, x => lensDistortion.intensity.value = x, -0.7f, 0.35f).SetEase(Ease.OutQuart)
                    .OnComplete(() => DOTween.To(() => lensDistortion.intensity.value, x => lensDistortion.intensity.value = x, 0f, 0.1f).SetEase(Ease.InSine)))
                .AppendCallback(() =>
                {
                    productionCM.Priority = 0;
                    SetStartUI(player);
                    if (PhotonNetwork.IsMasterClient)
                    {
                        Hashtable CP = PhotonNetwork.CurrentRoom.CustomProperties;
                        CP["startTime"] = PhotonNetwork.Time;
                        CP["isStartGame"] = true;
                        PhotonNetwork.CurrentRoom.SetCustomProperties(CP);
                    }
                    motionBlur.intensity.value = 0.1f;
                    productionCM.GetComponent<CinemachineConfiner2D>().enabled = false;
                });
        }

        private void SetStartUI(Player p)
        {
            GameObject joyStick = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Joystick"));
            joyStick.GetComponentInChildren<GunJoystickUI>().DIP(p);
            joyStick.GetComponentInChildren<PlayerJoystickUI>().DIP(p);
            joyStick.GetComponentInChildren<RemoveCardButton>().Init(p);

            CardHandlUI _cardHandleUI = GameObject.Instantiate(Resources.Load<CardHandlUI>("Prefabs/UI/CardCanvas"));
            _cardHandleUI.Init(p);
            p._cardHandleUI = _cardHandleUI;
            p.photonView.RPC("CardInit", RpcTarget.All);
            p.Clear -= TouchPanelActive;
            p.Clear += TouchPanelActive;

            SwitchItemBoxUI(true);
        }

        private void TouchPanelActive(Player p)
        {
            p.Clear -= TouchPanelActive;
            _touchPanel.gameObject.SetActive(true);
        }

        private void EndBattle(Photon.Realtime.Player[] winner)
        {
            SwitchItemBoxUI(false);

            _touchPanel.Touch -= ChangeFollowingCam;
            NetworkManager.Instance.EndBattle -= EndBattle;
            NetworkManager.Instance.LeftPlayer -= ChangeFollowingCam;
            followCM.Follow = null;
            followCM.LookAt = null;
            float x = followCM.transform.position.x;
            
            float y = followCM.transform.position.y;

            DOTween.Sequence()
            .Append(DOTween.To(() => followCM.m_Lens.OrthographicSize, x => followCM.m_Lens.OrthographicSize = x, 75, 3).SetEase(Ease.InOutQuart))
            .Join(followCM.transform.DOMoveY(10 - y, 3).SetEase(Ease.InOutQuart).SetRelative())
            .Join(followCM.transform.DOMoveX(123 - x, 3).SetEase(Ease.InOutQuart).SetRelative())
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                SetUI(winner);
            });
        }

        private void SetUI(Photon.Realtime.Player[] winner)
        {
            _touchPanel.gameObject.SetActive(false);
            if (winner.Length != 0)
            {
                List<SpriteRenderer> players = new List<SpriteRenderer>();
                List<string> nns = new List<string>();
                for (int i = 0; i < winner.Length; ++i)
                {
                    players.Add(((Player)winner[i].TagObject).Spriterenderer);
                    nns.Add(winner[i].NickName);
                }
                WinPanelUI temp = Instantiate(winPanel);
                temp.Init(players, nns);
            }

            else
            {
                DrawPanelUI temp = Instantiate(drawPanel);
                temp.Init();
            }
        }

        private void ChangeFollowingCam()
        {
            if (NetworkManager.Instance.alivePlayers.Count == 1)
                return;          

            playerIndex = (playerIndex + 1) % NetworkManager.Instance.alivePlayers.Count;

            followCM.Follow = NetworkManager.Instance.alivePlayers[playerIndex].transform;
            followCM.LookAt = NetworkManager.Instance.alivePlayers[playerIndex].transform;
        }

        public void UpdateItemBoxUI(Vector3 pos)
        {
            if (player == null)
            {
                return;
            }

            ItemBoxImage.rectTransform.anchoredPosition = (pos - player.transform.position).normalized * 200;
        }

        public void SwitchItemBoxUI(bool _)
        {
            ItemBoxImage.gameObject.SetActive(_);
        }

        private async UniTaskVoid EndProduction()
        {
            followCM.LookAt = null;

            while (followCM.m_Lens.OrthographicSize < 75)
            {
                ++followCM.m_Lens.OrthographicSize;

                await UniTask.Delay(TimeSpan.FromMilliseconds(50));
            }

        }
    }
}