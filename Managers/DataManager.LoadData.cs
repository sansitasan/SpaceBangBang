using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System.Linq;
using Unity.VisualScripting;
using Firebase.Auth;
using UnityEditor;
using System.Threading.Tasks;
using System;
using System.Threading;
using Photon.Pun;
using WebSocketSharp;
using System.IO;

namespace SpaceBangBang
{
    //회원가입이 잘 되는지에 관한 enum
    public enum BTask
    {
        //성공
        TRUE,
        //이메일이 형식에 맞지 않음
        IDFALSE,
        //패스워드가 형식에 맞지 않음
        PWFALSE,
        //(SignIn)회원가입이 되어있지 않거나 (SignUp)회원가입 실패
        CHECKFALSE,
        //회원가입이 되어있으나 다시 회원가입을 시도하는 경우
        CHECKIDFALSE,
        //이메일 보내기 실패
        SENDEMAILFALSE,
        //인증 실패
        VERIFYFALSE,
        //닉네임을 아직 만들지 않음
        NNFALSE,
        //예기치 못한 오류 - 주로 팅김, 네트워크 끊김 등
        CANCELFALSE
    }

    public enum CharacterType
    {
        Captain,
        Grizzled,
        Dandy
    }

    public interface ILoader<Key, Value>
    {
        Dictionary<Key, Value> Load();
    }

    public partial class DataManager
    {
        //플레이어와 무기 사전 필요하면 Clone()으로 복사해서 사용하기
        public Dictionary<int, PlayerStat> PlayerDict { get; private set; } = new Dictionary<int, PlayerStat>();
        public Dictionary<int, WeaponStat> WeaponDict { get; private set; } = new Dictionary<int, WeaponStat>();
        private FirebaseDatabase _base;
        private FirebaseAuth _auth;
        private FirebaseUser _user;

        public float BGMVolume { get; private set; }
        public float EffectVolume { get; private set; }
        public Action<float> BgmAction;
        public Action<float> EffectAction;

        private AndroidJavaObject _androidJavaObject;

        public void Init()
        {
            _base = FirebaseDatabase.DefaultInstance;
            _auth = FirebaseAuth.DefaultInstance;
            _auth.StateChanged += AuthStateChanged;
            AuthStateChanged(this, null);
            _androidJavaObject = new AndroidJavaObject("com.DefaultCompany.SpaceBangBang.KakaoPlugin");
            LoadJS("Character");
            LoadJS("Weapon");
            BgmAction += (float value) => { BGMVolume = value; };
            EffectAction += (float value) => { EffectVolume = value; };
            GameObject.Instantiate(Resources.Load("Prefabs/UI/SettingCanvas"));
        }

        public bool UserCheck()
        {
            if (_user != null)
            {
                if (_user.IsAnonymous)
                    return true;
                else if (_user.ProviderData.First().ProviderId.CompareTo("password") != 0)
                    return true;
                else if (_user.IsEmailVerified)
                    return true;
            }
            return false;
        }

        //자동 로그인을 위한 함수
        private void AuthStateChanged(object sender, EventArgs eventArgs)
        {
            if (_auth.CurrentUser != _user)
            {
                bool signedIn = _user != _auth.CurrentUser && _auth.CurrentUser != null;

                _user = _auth.CurrentUser;
            }
        }

        //Json파일 읽어오기
        private Loader LoadJson<Loader, Key, Value>(string data) where Loader : ILoader<Key, Value>
        {
            return JsonUtility.FromJson<Loader>(data);
        }

        public void SaveVolume(SoundVolume vol)
        {
            string voldata = JsonUtility.ToJson(vol);
            string path = Path.Combine(Application.persistentDataPath, "volume");
            File.WriteAllText(path, voldata);
        }

        public SoundVolume LoadVolume(string path)
        {
            if (File.Exists(path))
            {
                string voldata = File.ReadAllText(path);
                return JsonUtility.FromJson<SoundVolume>(voldata);
            }
            else
            {
                return new SoundVolume(1, 1);
            }
        }

        //게임에 필요한 데이터들을 가져온다.
        private void LoadJS(string data)
        {
            _base.GetReference($"{data}").GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Application.Quit();
                }

                else if (task.IsCanceled)
                {
                    Application.Quit();
                }

                else if (task.IsCompletedSuccessfully)
                {
                    DataSnapshot dss = task.Result;
                    string d = dss.GetRawJsonValue();

                    if (data.CompareTo("Character") == 0)
                        PlayerDict = LoadJson<StatData<PlayerStat>, int, PlayerStat>(d).Load();
                    else
                    {
                        WeaponDict = LoadJson<StatData<WeaponStat>, int, WeaponStat>(d).Load();

                        WeaponDict.Values.ToList().ForEach(x => x.Sprites = Resources.Load($"Prefabs/Gun/{x.Name}").GetComponentsInChildren<SpriteRenderer>().Select(x => x.sprite).ToArray());
                        WeaponDict.Values.ToList().ForEach(x => x.Range *= 0.16f);
                    }
                }
            });
        }
    }
}