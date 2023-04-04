using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.Rendering;
using DG.Tweening;

namespace SpaceBangBang
{
    public class GameManager : MonoBehaviour
    {
        static GameManager s_instance;
        public static GameManager Instance { get { Init(); return s_instance; } }//Manager ��������

        private DataManager _data = new DataManager();
        private SceneManagerEx _scene = null;

        public static DataManager Data { get { return s_instance._data; } }
        public static SceneManagerEx Scene { get { return s_instance._scene; } }
        public Dictionary<int, WaitForSeconds> WfsDict = new Dictionary<int, WaitForSeconds>();

        [SerializeField]
        public TextMeshProUGUI timerText;
        [SerializeField]
        private double selecctTime;
        [SerializeField]
        private double gameTime;

        private double currentTime;
        private float currentTimer;

        public bool isEndGame;

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            UpdateTimer();
            //스페이스바를 누르면
        }

        static void Init()
        {
            if (s_instance == null)
            {
                GameObject go = GameObject.Find("GameManager");
                if (go == null)
                {
                    go = new GameObject { name = "GameManager" };
                    go.AddComponent<GameManager>();
                }

                DontDestroyOnLoad(go);
                s_instance = go.GetComponent<GameManager>();
                s_instance._data.Init();
                s_instance._scene = s_instance.GetOrAddComponent<SceneManagerEx>(go);
                //y축 기준 정렬
                GraphicsSettings.transparencySortMode = TransparencySortMode.CustomAxis;
                GraphicsSettings.transparencySortAxis = Vector3.up;

                //프레임 60 고정
                Application.targetFrameRate = 60;
            }
        }

        //������Ʈ �ִ��� üũ
        public T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
                component = go.AddComponent<T>();

            return component;
        }

        public WaitForSeconds Wfs(int time)
        {
            if (WfsDict.ContainsKey(time))
                return WfsDict[time];
            else
            {
                WfsDict.Add(time, new WaitForSeconds(time * 0.01f));
                return WfsDict[time];
            }
        }

        #region Timer

        private void UpdateTimer()
        {
            if (PhotonNetwork.InRoom)
            {
                if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["isStartCharacterSelect"] && currentTimer > 0 && !isEndGame)
                {
                    double incTimer = PhotonNetwork.Time - (double)PhotonNetwork.CurrentRoom.CustomProperties["startTime"];

                    currentTimer = Mathf.Max(0, (float)(currentTime - incTimer));
                    timerText.text = $"{(int)currentTimer / 60} : {(int)currentTimer % 60}";

                    // 타이머가 0이 됏을 때 분기를 나눠 실행 (캐릭터 선택, 게임 시작)
                    if (currentTimer == 0 && (bool)PhotonNetwork.CurrentRoom.CustomProperties["isStartGame"])
                    {
                        // 가장 체력이 낮은 사람이 패배. (체력이 같으면 무승부)
                        if (PhotonNetwork.IsMasterClient)
                        {
                            NetworkManager.Instance.CheckGameStatus(true);
                            timerText.gameObject.SetActive(false);
                        }
                    }
                    else if (currentTimer == 0 && (int)PhotonNetwork.LocalPlayer.CustomProperties["Character"] == -1)
                    {
                        // ????? ?? ?????? ?? ???????? ????? ???????? ĳ???? ????.
                        NetworkManager.Instance.roomScene.SelectCharacter("Random");
                    }
                }
            }
        }

        public void StartCharacterSelectTimer()
        {
            timerText.gameObject.SetActive(true);
            currentTimer = (float)selecctTime;
            currentTime = selecctTime;
        }

        public void StartGameTimer()
        {
            timerText.gameObject.SetActive(true);
            currentTimer = (float)gameTime;
            currentTime = gameTime;
        }

        public void SetText(string text)
        {
            timerText.DOText(text, 1f);
        }

        #endregion
    }
}