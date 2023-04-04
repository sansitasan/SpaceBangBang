using System;
using System.IO;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBangBang
{
    [Serializable]
    public struct SoundVolume
    {
        public float BGM;
        public float Effect;

        public SoundVolume(float b = 1, float e = 1)
        {
            BGM = b;
            Effect = e;
        }
    }

    public class OptionPanel : BasePanel
    {
        public CAudio cAudio;
        public AudioClip clip;
        public static OptionPanel instance { get; private set; }
        [SerializeField]
        private Slider _Bgm;
        [SerializeField]
        private Slider _Effect;

        private float _prevbgm;
        private float _preveffect;

        private void OnEnable()
        {
            _prevbgm = _Bgm.value;
            _preveffect = _Effect.value;
        }

        protected override void Init()
        {
            SetOption();
            cAudio = new CAudio(GameManager.Instance.GetOrAddComponent<AudioSource>(gameObject), Sound.Effect);
            this.FixedUpdateAsObservable().Where(_ => Input.GetKeyDown(KeyCode.Escape))
                .Subscribe(_ => CancelClick());
            base.Init();
            DontDestroyOnLoad(gameObject.transform.parent.gameObject);
            instance = this;
        }

        public void SetOption()
        {
            SoundVolume v = GameManager.Data.LoadVolume(Path.Combine(Application.persistentDataPath, "volume"));
            _Bgm.value = v.BGM;
            _Effect.value = v.Effect;
            _prevbgm = _Bgm.value;
            _preveffect = _Effect.value;
            GameManager.Data.EffectAction?.Invoke(_Effect.value);
            GameManager.Data.BgmAction?.Invoke(_Bgm.value);
        }

        public void SaveClick()
        {
            //save
            cAudio.PlaySound(clip,Sound.Effect);
            GameManager.Data.SaveVolume(new SoundVolume(_Bgm.value, _Effect.value));
            BActive(false);
        }

        public void CancelClick()
        {
            cAudio.PlaySound(clip,Sound.Effect);
            _Bgm.value = _prevbgm;
            _Effect.value = _preveffect;
            GameManager.Data.EffectAction?.Invoke(_preveffect);
            GameManager.Data.BgmAction?.Invoke(_prevbgm);
            BActive(false);
        }
    }
}