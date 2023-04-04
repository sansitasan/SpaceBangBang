using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceBangBang
{
    public class WinPanelUI : MonoBehaviour
    {
        public CAudio cAudio;
        public AudioClip clip;
        [field: SerializeField]
        private Transform _winPanel { get; set; }
        [field: SerializeField]
        private List<Image> playerImg { get; set; }
        [field: SerializeField]
        private TextMeshProUGUI desctiptTxt { get; set; }
        [SerializeField]
        private Image _fade;

        public void Init(List<SpriteRenderer> spriteRenderer, List<string> nickName)
        {
            int i = 0;
            _winPanel.localScale = new Vector3(1, 0, 1);
            _winPanel.DOScale(Vector3.one, 0.3f);
            for (i = 0; i < spriteRenderer.Count; ++i)
            {
                playerImg[i].sprite = spriteRenderer[i].sprite;
                playerImg[i].transform.parent.gameObject.SetActive(true);
            }

            desctiptTxt.text = "NICKNAME :" + "\n";
            for (i = 0; i < nickName.Count; ++i)
                desctiptTxt.text += nickName[i] +"\n";
            desctiptTxt.text.Replace("\\n", "\n");
            
            cAudio = new CAudio(GameManager.Instance.GetOrAddComponent<AudioSource>(gameObject), Sound.Effect);
        }

        // Update is called once per frame
        public void OnClickExit()
        {
            cAudio.PlaySound(clip,Sound.Effect);
            _winPanel.DOScale(new Vector3(1, 0, 1), 0.3f);
            ChangeScene();
        }
        
        
        private void ChangeScene()
        {
            
            _fade.gameObject.SetActive(true);

            _fade.DOColor(Color.black, 1.5f)
                .OnComplete(() => NetworkManager.Instance.ExitGame());
        }
    }
}