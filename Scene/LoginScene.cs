using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBangBang
{
    public enum Sites
    {
        None,
        Kakao,
        Google
    }

    public class LoginScene : MonoBehaviour
    {
        [Header("Connect textInput")]
        [SerializeField]
        private TMP_InputField _idInputField;
        [SerializeField]
        private TMP_InputField _passwordInputField;

        [Header("MSG textInput")]
        [SerializeField]
        private TextMeshProUGUI _msg;

        [Header("Connect Button")]
        [SerializeField]
        private Button _kakaoButton;
        [SerializeField]
        private Button _connectButton;
        [SerializeField]
        private Button _registerButton;

        [Header("Child GameObject")]
        [SerializeField]
        private BasePanel _signUp;
        [SerializeField]
        private BasePanel _connect;
        [SerializeField]
        private MessagePanel _popupCanvas;
        
        public CAudio cAudio;
        public AudioClip clip;
        private void Start()
        {
            var EndGame = this.FixedUpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Escape));

            EndGame.Buffer(EndGame.Throttle(TimeSpan.FromMilliseconds(500)))
                .Where(x => x.Count >= 2)
                .Subscribe(_ => Application.Quit());

            _connect.BActive(true);
            cAudio = new CAudio(GameManager.Instance.GetOrAddComponent<AudioSource>(gameObject), Sound.Effect);
        }

        // ���� ��ư Ŭ���� ȣ��
        public void Connect()
        {
            Connect(_idInputField.text, _passwordInputField.text);
        }

        private async void Connect(string email, string pw, Sites site = Sites.None)
        {
            BTask btask = await GameManager.Data.SignInCheckEmailandPW(email, pw, site);
            CheckUserSignIn(btask);
        }

        private void CheckUserSignIn(BTask btask)
        {
            switch (btask)
            {
                case BTask.TRUE:
                    _connect.BActive(false);
                    NetworkManager.Instance.Connect();
                    break;

                case BTask.IDFALSE:
                    SetMessage("이메일 형식이 맞지 않습니다");
                    break;

                case BTask.PWFALSE:
                    SetMessage("비밀번호가 6자리 이상이 아닙니다");
                    break;

                case BTask.CHECKFALSE:
                    SetMessage("이미 다른 방식으로 회원가입을 했거나 회원가입 중입니다");
                    break;

                case BTask.CHECKIDFALSE:
                    SetMessage("확인되지 않은 오류");
                    break;

                case BTask.VERIFYFALSE:
                    SetMessage("회원가입하지 않은 이메일이거나 비밀번호가 틀렸습니다");
                    break;

                case BTask.NNFALSE:
                    break;

                case BTask.CANCELFALSE:
                    SetMessage("네트워크를 확인해주세요");
                    break;
            }
        }

        // ȸ������ ��ư Ŭ���� ȣ��
        public void Register(bool bsign = true)
        {
            if (bsign)
            {
                DOTween.Sequence()
                    .AppendCallback(() => _connect.BActive(false))
                    .AppendInterval(0.5f)
                    .AppendCallback(() => _signUp.BActive(true));
            }

            else
            {
                DOTween.Sequence()
                    .AppendInterval(0.5f)
                    .AppendCallback(() => _connect.BActive(true));
            }
            
        }

        public async void AnonymousButtonClick()
        {
            BTask btask = await GameManager.Data.AnonymousLoginAsync();
            Debug.Log(btask);
            switch (btask)
            {
                case BTask.TRUE:
                    _connect.BActive(false);
                    bool bsuc = false;
                    bsuc = NetworkManager.Instance.Connect();
                    if (!bsuc)
                        _connect.BActive(true);
                    break;

                default:
                    break;
            }
        }

        public async void KaKaoButtonClickCre()
        {
            BTask btask = await GameManager.Data.KaKaoSignUpAsync();

            CheckUserSignIn(btask);
        }

        //�޼��� ���
        public void SetMessage(string msg)
        {
            _popupCanvas.SetMessage(msg);
        }

        public void OptionButton()
        {
            OptionPanel.instance.BActive(true);
        }
        
        public void SoundButton()
        {
            cAudio.PlaySound(clip,Sound.Effect);
        }
    }
}