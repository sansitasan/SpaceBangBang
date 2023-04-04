using UnityEngine;
using TMPro;
using UniRx;
using UniRx.Triggers;

namespace SpaceBangBang
{
    public class SignUp : BasePanel
    {
        [Header("Text")]
        [SerializeField]
        private TMP_InputField _targetemail;
        [SerializeField]
        private TMP_InputField _targetpw;

        [Header("Parent")]
        [SerializeField]
        private LoginScene _loginScene;
        
        public CAudio cAudio;
        public AudioClip clip;

        protected override void Init()
        {
            cAudio = new CAudio(GameManager.Instance.GetOrAddComponent<AudioSource>(gameObject), Sound.Effect);
            this.FixedUpdateAsObservable()
                .Where(_ => Input.GetKey(KeyCode.Escape))
                .Subscribe(_ => ExitSignUp());
            base.Init();
        }

        public void ExitSignUp()
        {
            BActive(false);
            _loginScene.Register(false);
        }

        public async void SendEmailButtonClick()
        {
            BTask btask = await GameManager.Data.SignUpCheckEmailandPW(_targetemail.text, _targetpw.text);

            switch (btask)
            {
                case BTask.TRUE:
                    SendEmail();
                    break;

                case BTask.IDFALSE:
                    _loginScene.SetMessage("���Ŀ� ���� �ʴ� �̸����Դϴ�.");
                    break;

                case BTask.PWFALSE:
                    _loginScene.SetMessage("���Ŀ� ���� �ʴ� ��й�ȣ�Դϴ�.");
                    break;

                case BTask.CHECKFALSE:
                    _loginScene.SetMessage("�̹� ���Ե� �̸����Դϴ�.");
                    break;

                case BTask.VERIFYFALSE:
                    _loginScene.SetMessage("�̹� ���Ե� �̸����Դϴ�.");
                    break;

                case BTask.CANCELFALSE:
                    _loginScene.SetMessage("�ٽ� �õ����ֽñ� �ٶ��ϴ�.");
                    break;
            }
        }

        //���� �̸��� ������
        private async void SendEmail()
        {
            BTask btask = await GameManager.Data.SendEmailAsync();

            switch (btask)
            {
                case BTask.TRUE:
                    _loginScene.SetMessage("�̸����� ���½��ϴ�.");
                    VerifiedUser();
                    break;

                case BTask.SENDEMAILFALSE:
                    _loginScene.SetMessage("�̸��� ���� ����");
                    break;

                case BTask.CANCELFALSE:
                    _loginScene.SetMessage("�ٽ� �õ����ֽñ� �ٶ��ϴ�.");
                    break;
            }
        }

        //���� �̸��� ���� ��
        private async void VerifiedUser()
        {
            BTask btask = await GameManager.Data.VerifiyUserAsync();

            switch (btask)
            {
                case BTask.TRUE:
                    _loginScene.SetMessage("ȸ������ ����");
                    ExitSignUp();
                    break;

                case BTask.VERIFYFALSE:
                    _loginScene.SetMessage("���� ����");
                    break;
            }
        }

        public void OptionClick()
        {
            OptionPanel.instance.BActive(true);
        }
        
        public void SoundClick()
        {
            cAudio.PlaySound(clip,Sound.Effect);
        }
    }
}
