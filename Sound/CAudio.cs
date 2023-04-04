using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceBangBang
{
    public enum Sound
    {
        Effect,
        Bgm
    }

    public class CAudio
    {
        private AudioSource _audioSource;
        private Sound _type;

        private void InitVolume(Scene prev, Scene next)
        {
            if (_audioSource != null)
                switch (_type)
                {
                    case Sound.Effect:
                        _audioSource.volume = GameManager.Data.EffectVolume;
                        break;

                    case Sound.Bgm:
                        _audioSource.volume = GameManager.Data.BGMVolume;
                        break;
                }
            else
                Clear();
        }

        private void SetVolume(float value)
        {
            _audioSource.volume = value;
        }

        public CAudio(AudioSource a, Sound type)
        {
            _audioSource = a;
            _type = type;
            switch (_type)
            {
                case Sound.Effect:
                    _audioSource.volume = GameManager.Data.EffectVolume;
                    GameManager.Data.EffectAction -= SetVolume;
                    GameManager.Data.EffectAction += SetVolume;
                    break;

                case Sound.Bgm:
                    _audioSource.volume = GameManager.Data.BGMVolume;
                    GameManager.Data.BgmAction -= SetVolume;
                    GameManager.Data.BgmAction += SetVolume;
                    break;
            }

            SceneManager.activeSceneChanged -= InitVolume;
            SceneManager.activeSceneChanged += InitVolume;
        }

        public void PlaySound(AudioClip clip, Sound type, float pitch = 1.0f)
        {

            if (_audioSource.isPlaying)
            {
                if (_audioSource.clip != clip)
                    _audioSource.Stop();
                else
                    return;
            }

            _audioSource.pitch = pitch;

            if (type == Sound.Bgm)
            {
                _audioSource.clip = clip;
                _audioSource.Play();
            }

            else
                _audioSource.PlayOneShot(clip);
        }

        public void StopSoundFade(float time = 1.5f)
        {
            DOTween.Sequence()
            .Append(DOTween.To(() => _audioSource.volume, x => _audioSource.volume = x, 0, time).SetEase(Ease.InCubic));
        }

        public void StopSound()
        {
            if (_audioSource.isPlaying)
                _audioSource.Stop();
        }

        public void Clear()
        {
            SceneManager.activeSceneChanged -= InitVolume;
            GameManager.Data.EffectAction -= SetVolume;
            GameManager.Data.BgmAction -= SetVolume;
            
        }
    }
}
