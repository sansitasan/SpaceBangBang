using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceBangBang
{
    public class BgmPlayer : MonoBehaviour
    {
        private enum Bgms
        {
            None = -1,
            Main = 0,
            Battle = 1
        }

        private CAudio _cAudio;
        [SerializeField]
        private AudioClip[] _clips;

        void Start()
        {
            Init();
        }

        private void Init()
        {
            _cAudio = new CAudio(GameManager.Instance.GetOrAddComponent<AudioSource>(gameObject), Sound.Bgm);
            DontDestroyOnLoad(gameObject);
            GameManager.Scene.OnSceneLoaded -= BgmVolumeFade;
            GameManager.Scene.OnSceneLoaded += BgmVolumeFade;
            SceneManager.activeSceneChanged -= SelectBgm;
            SceneManager.activeSceneChanged += SelectBgm;

            _cAudio.PlaySound(_clips[(int)Bgms.Main], Sound.Bgm);
        }

        private void BgmVolumeFade(string prevscene, string nextscene)
        {
            if (nextscene.CompareTo("BattleScene") == 0)
                _cAudio.StopSoundFade();
            else if (prevscene.CompareTo("BattleScene") == 0 && nextscene.CompareTo("LobbyScene") == 0)
                _cAudio.StopSoundFade();
        }

        private void SelectBgm(Scene prev, Scene next)
        {
            if (next.name.CompareTo("BattleScene") != 0)
                _cAudio.PlaySound(_clips[(int)Bgms.Main], Sound.Bgm);
            else
                _cAudio.PlaySound(_clips[(int)Bgms.Battle], Sound.Bgm);
        }
    }
}
