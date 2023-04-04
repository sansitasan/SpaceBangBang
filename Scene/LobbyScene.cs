using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using TMPro;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBangBang
{
    public class LobbyScene : MonoBehaviour
    {
        [SerializeField]
        private Button[] roomConnectButtons;
        [SerializeField]
        private TMP_InputField roomCreateNameText; // �� �̸� ���� ����

        [SerializeField]
        private BasePanel _roomPanel;

        [Header("NN")]
        [SerializeField]
        private BasePanel _NNCanvas;

        [SerializeField]
        private TMP_InputField _NNInputField;

        [Header("Popup")]
        [SerializeField]
        private MessagePanel _popupCanvas;

        [Header("Button")]
        [SerializeField]
        private Button nextButton;
        [SerializeField]
        private Button prevButton;

        private int pageNum;
        
        public CAudio cAudio;
        public AudioClip clip;

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            _roomPanel.BActive(true);
            NetworkManager.Instance.lobbyScene = this;
            cAudio = new CAudio(GameManager.Instance.GetOrAddComponent<AudioSource>(gameObject), Sound.Effect);

            switch (GameManager.Data.CheckUserNickName())
            {
                case BTask.TRUE:
                    if (_NNCanvas.gameObject.activeInHierarchy)
                        _NNCanvas.BActive(false);
                    break;
                case BTask.NNFALSE:
                    _NNCanvas.BActive(true);
                    break;
                case BTask.CANCELFALSE:
                    LeaveLobby();
                    break;
            }
            RenewRoomList();

            var EndGame = this.FixedUpdateAsObservable()
                .Where(_ => !OptionPanel.instance.gameObject.activeSelf)
                .Where(_ => Input.GetKeyDown(KeyCode.Escape));

            EndGame.Buffer(EndGame.Throttle(TimeSpan.FromMilliseconds(500)))
                .Where(x => x.Count >= 2)
                .Subscribe(_ => LeaveLobby());
        }

        // �� ���� ��ư Ŭ���� ȣ��
        public void JoinRoom(int num)
        {
            cAudio.PlaySound(clip,Sound.Effect);
            DOTween.Sequence()
                .AppendCallback(() => _roomPanel.BActive(false))
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    bool bsuc = NetworkManager.Instance.JoinRoom(roomConnectButtons.Length * pageNum + num);
                    if (!bsuc)
                    {
                        _popupCanvas.SetMessage("방 입장에 실패했습니다");
                        _roomPanel.BActive(true);
                    }
                });
        }

        public void RenewRoomList()
        {
            NetworkManager.Instance.RenewRoomList(roomConnectButtons, pageNum, nextButton);
        }

        public void ChageRoomPage(int num)
        {
            cAudio.PlaySound(clip,Sound.Effect);
            pageNum = Mathf.Max(0, pageNum + num);
            RenewRoomList();
        }

        public void CreateRoom()
        {
            cAudio.PlaySound(clip,Sound.Effect);
            if (!String.IsNullOrWhiteSpace(roomCreateNameText.text))
                DOTween.Sequence()
                    .AppendCallback(() => _roomPanel.BActive(false))
                    .AppendInterval(0.5f)
                    .AppendCallback(() =>
                    {
                        bool bsuc = NetworkManager.Instance.CreateRoom(roomCreateNameText.text);
                        if (!bsuc)
                        {
                            _popupCanvas.SetMessage("방 생성에 실패했습니다");
                            _roomPanel.BActive(true);
                        }
                    });
        }

        public void LeaveLobby()
        {
            cAudio.PlaySound(clip,Sound.Effect);
            DOTween.Sequence()
                .AppendCallback(() => GameManager.Data.SignOut())
                .AppendCallback(() => _roomPanel.BActive(false))
                .AppendInterval(0.5f)
                .AppendCallback(() => NetworkManager.Instance.LeaveLobby());
        }

        public async void SetNN()
        {
            cAudio.PlaySound(clip,Sound.Effect);
            int unicodenum = 0;
            int len = _NNInputField.text.Length;
            int i = 0;
            if (len > 8 || len == 0)
                _popupCanvas.SetMessage("닉네임은 최소 1자부터 7자까지 가능합니다");

            else
            {
                for (i = 0; i < _NNInputField.text.Length; ++i)
                {
                    unicodenum = _NNInputField.text[i];

                    //�����ڵ� ���� ã��
                    if (unicodenum > 44047 && unicodenum < 0xd7b0)//�ѱ�
                        continue;
                    else if (unicodenum > 47 && unicodenum < 58)//����
                        continue;
                    else if (unicodenum > 64 && unicodenum < 91)//�빮��
                        continue;
                    else if (unicodenum > 96 && unicodenum < 123)//�ҹ���
                        continue;
                    else
                        break;
                }

                if (i < len)
                    _popupCanvas.SetMessage("닉네임에 지원되지 않는 언어, 문자가 있습니다");

                else
                {
                    BTask suc = await GameManager.Data.SetUserNN(_NNInputField.text);

                    switch (suc)
                    {
                        case BTask.TRUE:
                            _popupCanvas.SetMessage("닉네임 설정 완료");
                            _NNCanvas.BActive(false);
                            break;

                        case BTask.NNFALSE:
                            _popupCanvas.SetMessage("닉네임 설정에 실패했습니다.");
                            break;

                        case BTask.CANCELFALSE:
                            _popupCanvas.SetMessage("네트워크를 확인해주세요.");
                            break;
                    }
                }
            }
        }

        public void OptionButton()
        {
            OptionPanel.instance.BActive(true);
        }
    }
}
