using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBangBang
{
    public class DrawPanelUI : MonoBehaviour
    {
        public CAudio cAudio;
        public AudioClip clip;
        [field: SerializeField]
        private Transform _drawPanel { get; set; }
        [SerializeField]
        private Image _fade;

        public void Init()
        {
            _drawPanel.localScale = new Vector3(1, 0, 1);
            _drawPanel.DOScale(Vector3.one, 1);
            cAudio = new CAudio(GameManager.Instance.GetOrAddComponent<AudioSource>(gameObject), Sound.Effect);
        }

        // Update is called once per frame
        private void ChangeScene()
        {
            _fade.gameObject.SetActive(true);

            _fade.DOColor(Color.black, 1.5f)
                .OnComplete(() => NetworkManager.Instance.ExitGame());
        }
        
        public void OnClickExit()
        {
            cAudio.PlaySound(clip,Sound.Effect);
            _drawPanel.DOScale(new Vector3(1, 0, 1), 0.3f);
            ChangeScene();
        }
    }
}