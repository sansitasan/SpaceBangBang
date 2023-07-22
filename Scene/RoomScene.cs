using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace SpaceBangBang
{
    public class RoomScene : MonoBehaviour
    {
        [SerializeField]
        private BasePanel roomPanel;
        [SerializeField]
        private BasePanel characterSelectPanel;
        [SerializeField]
        private Image _fade;
        [SerializeField]
        private MessagePanel _popupCanvas;

        [SerializeField]
        private TextMeshProUGUI[] roomPlayerNicknameTexts;
        [SerializeField]
        private GameObject gameStartButtonObj;
        [SerializeField]
        private GameObject gameReadyButtonObj;

        private IDisposable cancel;
        
        public CAudio cAudio;
        public AudioClip clip;

        private void Awake()
        {
            NetworkManager.Instance.roomScene = this;
            //�÷��̾� �г��� �������� �޾ƿ���
        }

        private void Start()
        {
            roomPanel.BActive(true);
            RenewRoom();

            cAudio = new CAudio(GameManager.Instance.GetOrAddComponent<AudioSource>(gameObject), Sound.Effect);
            cancel = this.FixedUpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Escape))
                .Subscribe(_ => LeaveRoom());
        }

        public void RenewRoom()
        {
            NetworkManager.Instance.RenewRoom(roomPlayerNicknameTexts, gameStartButtonObj, gameReadyButtonObj);
        }

        public void StartCharacterSelect(bool isMaster)
        {
            if (isMaster)
            {
				SoundButton();
                //tlqkf
                var notReadyPlayers = PhotonNetwork.PlayerListOthers.ToList().Find(x => (bool)x.CustomProperties["isReady"] == false);

                if (true/*notReadyPlayers == null && PhotonNetwork.CurrentRoom.PlayerCount >= 2*/)
                {
                    cancel.Dispose();
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                    PhotonNetwork.CurrentRoom.MaxPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    Hashtable CP = PhotonNetwork.CurrentRoom.CustomProperties;
                    CP["isStartCharacterSelect"] = true;
                    CP["startTime"] = PhotonNetwork.Time;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(CP);
                    NetworkManager.Instance.photonView.RPC("StartCharacterSelectRPC", RpcTarget.All);
                }
                //else if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
                    //_popupCanvas.SetMessage("인원이 부족합니다");
                //else if(notReadyPlayers != null)
                    //_popupCanvas.SetMessage("준비되지 않은 인원이 있습니다");
            }
            else
            {
                cancel.Dispose();
                roomPanel.BActive(false);
                characterSelectPanel.BActive(true);
            }
        }

        public void ConvertReadyGame()
        {
            SoundButton();
            Hashtable CP = PhotonNetwork.LocalPlayer.CustomProperties;
            CP["isReady"] = !(bool)CP["isReady"];
            PhotonNetwork.LocalPlayer.SetCustomProperties(CP);
        }

        public void SelectCharacter(string name)
        {
            SoundButton();
            if (name.Equals("Random"))
            {
                name = GameManager.Data.PlayerDict.Values.ToList()[UnityEngine.Random.Range(0, GameManager.Data.PlayerDict.Values.ToList().Count)].Name;
            }

            Hashtable CP = PhotonNetwork.LocalPlayer.CustomProperties;
            switch (name)
            {
                case "Captain":
                    CP["Character"] = 0;
                    break;
                case "Dandy":
                    CP["Character"] = 2;
                    break;
                case "Grizzled":
                    CP["Character"] = 1;
                    break;
            }
            PhotonNetwork.LocalPlayer.SetCustomProperties(CP);
        }

        public void LeaveRoom()
        {
            SoundButton();
            DOTween.Sequence()
                .AppendCallback(() => roomPanel.BActive(false))
                .AppendInterval(1)
                .AppendCallback(() => NetworkManager.Instance.LeaveRoom());
        }

        public void SetPlayerColor(string name, Color color)
        {
            Array.Find(roomPlayerNicknameTexts, x => x.text == name).color = color;
        }

        public void ChangeScene()
        {
            _fade.gameObject.SetActive(true);
            _fade.DOColor(Color.black, 1.5f);
        }
        
        public void SoundButton()
        {
            cAudio.PlaySound(clip,Sound.Effect);
        }
    }
}
