using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace SpaceBangBang
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private List<RoomInfo> roomList = new List<RoomInfo>();

        public LobbyScene lobbyScene;
        public RoomScene roomScene;
        public Action<Photon.Realtime.Player[]> EndBattle;
        public Action LeftPlayer;

        public static NetworkManager Instance;

        public List<Player> alivePlayers = new List<Player>();

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.SendRate = 90;
            PhotonNetwork.SerializationRate = 90;

            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                if (Instance != this)
                {
                    Destroy(this.gameObject);
                }
            }
        }

        // ������ ���� (��ư���� ȣ��)
        public bool Connect()
        {
            return PhotonNetwork.ConnectUsingSettings();
        }

        public void LeaveLobby()
        {
            PhotonNetwork.Disconnect();
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
        }

        // ?�버?�??�결???�어졌을 ???�출?�는 ?�수�??�태??초기?�함
        public override void OnDisconnected(DisconnectCause cause)
        {
            Destroy(gameObject);
            SceneManager.LoadScene("MainScene");
        }

        // �κ� ������ �� ȣ��Ǵ�?�Լ��� ���� ���µ� ����
        public override void OnJoinedLobby()
        {
            if (SceneManager.GetActiveScene().name.CompareTo("BattleScene") != 0)
                SceneManager.LoadScene("LobbyScene");

            else
            {
                AsyncOperation ao = SceneManager.LoadSceneAsync("LobbyScene");
                GameManager.Scene.LoadScene(ao, SceneManager.GetActiveScene().name, "LobbyScene");
            }
            GameManager.Instance.isEndGame = false;
        }

        public override void OnLeftLobby()
        {
            SceneManager.LoadScene("LoginScene");
        }

        // �??�성 ?�수 (버튼?�로 ?�출)
        public bool CreateRoom(string roomName)
        {
            if (!String.IsNullOrWhiteSpace(roomName))
            {
                RoomOptions roomOptions = new RoomOptions();
                roomOptions.MaxPlayers = 4;
                roomOptions.CustomRoomProperties = new Hashtable() { { "isStartCharacterSelect", false }, { "isStartGame", false }, { "startTime", 0.0 } };
                return PhotonNetwork.CreateRoom(roomName, roomOptions);
            }

            return false;
        }

        // �κ� �ִ� ���ϵ��� ������Ʈ ����. ���� ���ο��� �� ���� ����?���ϰ� ��ư�� ��ȣ�ۿ��� ����.
        public void RenewRoomList(Button[] roomConnectButtons, int pageNum, Button nextButton)
        {
            for (int i = 0; i < roomConnectButtons.Length; i++)
            {
                if (i + pageNum * roomConnectButtons.Length < roomList.Count)
                {
                    nextButton.interactable = true;

                    if (roomList[i + pageNum * roomConnectButtons.Length].PlayerCount == roomList[i + pageNum * roomConnectButtons.Length].MaxPlayers)
                    {
                        roomConnectButtons[i].interactable = false;
                        roomConnectButtons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = roomList[i + pageNum * roomConnectButtons.Length].Name;
                        roomConnectButtons[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = roomList[i + pageNum * roomConnectButtons.Length].PlayerCount.ToString() + " / " + roomList[i].MaxPlayers.ToString();
                    }
                    else
                    {
                        roomConnectButtons[i].interactable = true;
                        roomConnectButtons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = roomList[i + pageNum * roomConnectButtons.Length].Name;
                        roomConnectButtons[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = roomList[i + pageNum * roomConnectButtons.Length].PlayerCount.ToString() + " / " + roomList[i].MaxPlayers.ToString();
                    }
                }
                else
                {
                    nextButton.interactable = false;
                    roomConnectButtons[i].interactable = false;
                    roomConnectButtons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
                    roomConnectButtons[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                }
            }
        }

        // ������ ���� �����?������ �� ȣ��Ǵ�?�Լ��� �̸� �̿��� �� ����Ʈ���� �ҷ����� RenewRoomList�� ȣ���� �����?������Ʈ ����.
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            int roomCount = roomList.Count;
            for (int i = 0; i < roomCount; i++)
            {
                if (!roomList[i].RemovedFromList)
                {
                    if (!this.roomList.Contains(roomList[i])) this.roomList.Add(roomList[i]);
                    else this.roomList[this.roomList.IndexOf(roomList[i])] = roomList[i];
                }
                else if (this.roomList.IndexOf(roomList[i]) != -1) this.roomList.RemoveAt(this.roomList.IndexOf(roomList[i]));
            }
            if (SceneManager.GetActiveScene().name == "LobbyScene") StartCoroutine(OnRoomListUpdateCoroutine());
        }

        private IEnumerator OnRoomListUpdateCoroutine()
        {
            while (lobbyScene == null)
            {
                yield return null;
            }
            lobbyScene.RenewRoomList();
        }

        // �� ���� �Լ� (��ư���� ȣ��)
        public bool JoinRoom(int num)
        {
            return PhotonNetwork.JoinRoom(roomList[num].Name);
        }

        // �÷��̾ �濡 ������ �� ȣ��Ǵ�?�Լ�(���� �����?ȣ��)�� ���� ���� ������Ʈ�� �÷��̾��� Ŀ���� ������Ƽ�� �߰�. 
        public override void OnJoinedRoom()
        {
            SceneManager.LoadScene("RoomScene");
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "isReady", false }, { "Character", -1 }, { "isLoadingDone", false } }); ;
            roomList.Remove(PhotonNetwork.CurrentRoom);
        }

        // �÷��̾ �濡 ������ �� ���?�÷��̾ ȣ��Ǵ�?�Լ��� �� ���� ������Ʈ.
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            roomScene.RenewRoom();
        }

        public override void OnLeftRoom()
        {
        }

        // �÷��̰� �濡 ������ �� ���?�÷��̾ ȣ��Ǵ�?�Լ��� �� ���� ������Ʈ ��, ������ ���� ���� �ÿ��� ���� ����. 
        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            if (!GameManager.Instance.isEndGame && (bool)PhotonNetwork.CurrentRoom.CustomProperties["isStartCharacterSelect"])
            {
                alivePlayers.Remove((Player)otherPlayer.TagObject);
                if (alivePlayers.Find(x => x == (Player)PhotonNetwork.LocalPlayer.TagObject) == null)
                    LeftPlayer.Invoke();
                if (alivePlayers.Count <= 1 && PhotonNetwork.IsMasterClient)
                    photonView.RPC("EndGameRPC", RpcTarget.All, new int[] { alivePlayers[0].photonView.OwnerActorNr });
            }
            else if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["isStartGame"])
            {
                roomScene.RenewRoom();
            }
        }

        // �� ���� ������Ʈ �Լ�. ȭ�鿡 �÷��̾� �г��� ���?�� ���� ����, �غ� �Ϸ� ��ư�� Ȱ��ȭ
        public void RenewRoom(TextMeshProUGUI[] roomPlayerNicknameTexts, GameObject gameStartButtonObj, GameObject gameReadyButtonObj)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                gameStartButtonObj.SetActive(true);
                gameReadyButtonObj.SetActive(false);
            }
            else
            {
                gameStartButtonObj.SetActive(false);
                gameReadyButtonObj.SetActive(true);
            }

            for (int i = 0; i < roomPlayerNicknameTexts.Length; i++)
            {
                if (i < PhotonNetwork.CurrentRoom.Players.Count)
                {
                    roomPlayerNicknameTexts[i].text = PhotonNetwork.CurrentRoom.Players.Values.ToList()[i].NickName;
                    UpdateReadyStatusPlayer(PhotonNetwork.CurrentRoom.Players.Values.ToList()[i], PhotonNetwork.CurrentRoom.Players.Values.ToList()[i].CustomProperties);
                }
                else
                {
                    roomPlayerNicknameTexts[i].text = "";
                }
            }
        }

        // �÷��̾��� Ŀ���� ������Ƽ�� ����ɽ�?ȣ�� �Ǵ� �Լ�. (������ �÷��̾��� ���¸� �����ϱ� ����, ���?�÷��̾ ĳ���͸� �����ߴ��� üũ�ϱ� ����.)
        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
        {
            UpdateReadyStatusPlayer(targetPlayer, changedProps);
            if (PhotonNetwork.IsMasterClient && (bool)PhotonNetwork.CurrentRoom.CustomProperties["isStartCharacterSelect"]) CheckAllPlayerSelectCharacter();
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            if (SceneManager.GetActiveScene().name.Equals("BattleScene") && (double)PhotonNetwork.CurrentRoom.CustomProperties["startTime"] != 0)
            {
                GameManager.Instance.StartGameTimer();
            }
        }

        // ������ �÷��̾��� ������ �ٲ���.
        private void UpdateReadyStatusPlayer(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
        {
            if (SceneManager.GetActiveScene().name.Equals("RoomScene"))
            {
                if (!changedProps.ContainsKey("isReady"))
                {
                    roomScene.SetPlayerColor(targetPlayer.NickName, Color.black);
                }
                else if (targetPlayer.IsMasterClient)
                {
                    roomScene.SetPlayerColor(targetPlayer.NickName, Color.black);
                }
                else if ((bool)changedProps["isReady"])
                {
                    roomScene.SetPlayerColor(targetPlayer.NickName, Color.blue);
                }
                else if (!(bool)changedProps["isReady"])
                {
                    roomScene.SetPlayerColor(targetPlayer.NickName, Color.black);
                }
            }
        }

        // 모든 ?�레?�어가 캐릭?��? ?�택?�는지 체크 ??만약 모든 ?�레?�어가 ?�택???�다�?StartGameRPC�??�출??게임 ?�작??
        private void CheckAllPlayerSelectCharacter()
        {
            if (SceneManager.GetActiveScene().name.Equals("RoomScene"))
            {
                var notSelectingPlayer = PhotonNetwork.PlayerList.ToList().Find(x => ((int)x.CustomProperties["Character"]) == -1);

                if (notSelectingPlayer == null)
                {
                    photonView.RPC("StartGameRPC", RpcTarget.All);
                }
            }
        }

        // ���� ���� �Լ�.
        [PunRPC]
        private void StartCharacterSelectRPC()
        {
            roomScene.StartCharacterSelect(false);
            GameManager.Instance.StartCharacterSelectTimer();
        }

        // ?�레?�어?�의 ?�태�??�인?�여 게임???�낼지 말�? ?�단??
        public void CheckGameStatus(bool isEnd = false)
        {
            if (GameManager.Instance.isEndGame)
            {
                return;
            }

            if (alivePlayers.Count == 1)
            {
                photonView.RPC("EndGameRPC", RpcTarget.All, new int[] { alivePlayers[0].photonView.OwnerActorNr });
            }
            else if (isEnd)
            {
                alivePlayers = alivePlayers.OrderByDescending(x => x.Stat.HP).ToList();

                var maxPlayer = alivePlayers[0];
                List<Player> winPlayers =  alivePlayers.FindAll(x => x._stat.HP == maxPlayer._stat.HP);

                if (winPlayers.Count == alivePlayers.Count)
                {
                    photonView.RPC("EndGameRPC", RpcTarget.All, new int[] {});
                }
                else
                {
                    photonView.RPC("EndGameRPC", RpcTarget.All, winPlayers.Select(x => x.photonView.OwnerActorNr).ToArray());
                }
            }
        }

        [PunRPC]
        private void EndGameRPC(int[] winPlayersActorNumers)
        {
            GameManager.Instance.isEndGame = true;
            GameManager.Instance.timerText.gameObject.SetActive(false);
            List<Photon.Realtime.Player> winPlayers = new List<Photon.Realtime.Player>();

            for (int i = 0; i < alivePlayers.Count; i++)
            {
                for (int j = 0; j < winPlayersActorNumers.Length; j++)
                {
                    if (alivePlayers[i].photonView.OwnerActorNr == winPlayersActorNumers[j])
                    {
                        winPlayers.Add(alivePlayers[i].photonView.Owner);
                        break;
                    }
                }
            }

            EndBattle?.Invoke(winPlayers.ToArray());
        }

        [PunRPC]
        private void StartGameRPC()
        {
            roomScene.ChangeScene();
            AsyncOperation ao = SceneManager.LoadSceneAsync("BattleScene");
            if (ao != null)
                GameManager.Scene.LoadScene(ao, SceneManager.GetActiveScene().name, "BattleScene");
        }

        // ���� ������.
        public void ExitGame()
        {
            alivePlayers.Clear();
            GameManager.Instance.timerText.gameObject.SetActive(false);
            PhotonNetwork.LeaveRoom();
        }
    }
}
