using DG.Tweening;
using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceBangBang
{
    public class MainScene : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private Image _fade;
        
        public CAudio cAudio;
        public AudioClip clip;

        public void OnPointerClick(PointerEventData eventData)
        {
            Fade();
        }

        private void Start()
        {
            cAudio = new CAudio(GameManager.Instance.GetOrAddComponent<AudioSource>(gameObject), Sound.Effect);
            //����
            var EndGame = this.FixedUpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Escape));

            EndGame.Buffer(EndGame.Throttle(TimeSpan.FromMilliseconds(500)))
                .Where(x => x.Count >= 2)
                .Subscribe(_ => Application.Quit());
        }

        private void Fade()
        {
            cAudio.PlaySound(clip,Sound.Effect);
            _fade.gameObject.SetActive(true);
            bool bcheck = false;
            bcheck = GameManager.Data.UserCheck();
            DOTween.Sequence()
                .Append(_fade.DOColor(Color.black, 1))
                .AppendCallback(() => 
                {
                    if (bcheck)
                    {
                        bool bsuc = NetworkManager.Instance.Connect();
                        if (!bsuc)
                            SceneManager.LoadScene("LoginScene");
                    }
                    else
                        SceneManager.LoadScene("LoginScene");
                });
        }
    }
}