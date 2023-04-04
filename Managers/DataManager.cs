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

    public class DataManager
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

        public async Task<BTask> AnonymousLoginAsync()
        {
            BTask btask = BTask.TRUE;

            Task SignIn = _auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                    btask = BTask.CANCELFALSE;

                else if (task.IsFaulted)
                    btask = BTask.CHECKFALSE;

                else if (task.IsCompletedSuccessfully)
                    _user = task.Result;
            });

            await SignIn;

            return btask;
        }

        #region Email LogIn
        //이메일 로그인
        private async Task<BTask> SignInCheckAsync(string email, string pw, Sites site = Sites.None)
        {
            BTask btask = BTask.TRUE;
            Task task = _auth.SignInWithEmailAndPasswordAsync(email, pw).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                    btask = BTask.CANCELFALSE;

                else if (task.IsFaulted)
                    btask = BTask.CHECKFALSE;

                else if (task.IsCompletedSuccessfully)
                {
                    _user = task.Result;
                    //인증 안 한 email 회원가입은 컷
                    if (site == Sites.None && !_user.IsEmailVerified)
                        btask = BTask.VERIFYFALSE;
                }
            });

            await task;

            return btask;
        }

        //이메일과 비밀번호가 파이어베이스 형식에 맞는지 체크
        public async Task<BTask> SignInCheckEmailandPW(string email, string pw, Sites site = Sites.None)
        {
            if (!email.Contains("@")
                || !email.Contains("."))
                return BTask.IDFALSE;

            if (pw.Length < 6)
                return BTask.PWFALSE;

            else
                return await SignInCheckAsync(email, pw, site);
        }

        //이메일과 비밀번호가 파이어베이스 형식에 맞는지 체크
        public async Task<BTask> SignUpCheckEmailandPW(string email, string pw)
        {
            if (!email.Contains("@")
                || !email.Contains("."))
                return BTask.IDFALSE;

            if (pw.Length < 6)
                return BTask.PWFALSE;

            else
                return await SignUpUserAsync(email, pw);
        }

        //유저 회원가입
        public async Task<BTask> SignUpUserAsync(string email, string pw)
        {
            //TODO - 회원가입을 전에 보냈는데 아직 인증하지 않은 상태에서
            //비밀번호를 바꿔서 다시 회원가입을 신청하는 경우 - 이 경우는 글쎄...
            //이미 다른 방식으로 회원가입을 한 경우

            BTask btask = await BFirstSign(email);

            if (btask != BTask.CHECKFALSE)
            {
                Task task = _auth.CreateUserWithEmailAndPasswordAsync(email, pw).ContinueWithOnMainThread(task =>
                {
                    //password가 틀렸으므로 이 때는 회사 이메일로 인증 받고 비번바꾸기
                    if (task.IsFaulted)
                        btask = BTask.VERIFYFALSE;

                    else if (task.IsCanceled)
                        btask = BTask.CANCELFALSE;

                    else if (task.IsCompletedSuccessfully)
                        _user = _auth.CurrentUser;
                });

                await task;
            }

            return btask;
        }

        //가입신청한 이메일에 메세지를 보냄
        public async Task<BTask> SendEmailAsync()
        {
            BTask btask = BTask.SENDEMAILFALSE;
            _user = _auth.CurrentUser;
            _auth.LanguageCode = "ko";
            Task t = _user.SendEmailVerificationAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                    btask = BTask.TRUE;

                else if (task.IsCanceled)
                    btask = BTask.CANCELFALSE;
            });

            await t;

            return btask;
        }

        //유저가 이메일 인증을 했는지
        public async Task<BTask> VerifiyUserAsync()
        {
            bool bverified = _user.IsEmailVerified;
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

            //유저가 인증할때까지 뺑뺑이
            while (!bverified)
            {
                //1초동안 쉬기
                await Task.Delay(1000);

                //5분동안 안 했으면 빠져나가서 return false
                if (cts.IsCancellationRequested)
                    break;

                //유저가 인증했는지 확인
                await _user.ReloadAsync();
                bverified = _user.IsEmailVerified;
            }
            if (bverified)
                return BTask.TRUE;
            return BTask.VERIFYFALSE;
        }
        #endregion

        public async Task<BTask> KaKaoSignUpAsync()
        {
            string token = _androidJavaObject.Call<string>("KakaoLogin");
            BTask btask = BTask.CHECKFALSE;
            if (!token.IsNullOrEmpty())
            {
                string email = _androidJavaObject.Call<string>("GetUserData");
                BTask first = await BFirstSign(email);
                if (first == BTask.TRUE || first == BTask.CHECKFALSE)
                {
                    Credential kakaoCre = OAuthProvider.GetCredential("oidc.kakao", token.Substring(0, token.IndexOf(" ")), token.Substring(token.IndexOf(" ") + 1));
                    await _auth.SignInWithCredentialAsync(kakaoCre).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCanceled)
                            btask = BTask.CANCELFALSE;

                        else if (task.IsCompletedSuccessfully)
                        {
                            _user = task.Result;
                            //Debug.Log($"unitylog: time {_user.Metadata.CreationTimestamp}, {_user.Metadata.LastSignInTimestamp}");
                            btask = BTask.TRUE;
                        }
                    });

                    if (first == BTask.TRUE)
                        await SetUserNN(string.Empty);
                }

                else
                    btask = BTask.CHECKIDFALSE;
            }

            return btask;
        }

        private async Task<BTask> BFirstSign(string email)
        {
            BTask first = BTask.VERIFYFALSE;
            await _auth.FetchProvidersForEmailAsync(email).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    if (task.Result.Count() == 0)
                        first = BTask.TRUE;
                    //task.Result.First()에 최근 로그인한 로그인 방식이 나온다.
                    else if (task.Result.First().CompareTo("password") != 0)
                        first = BTask.CHECKFALSE;
                }

                else if (task.IsCanceled)
                    first = BTask.CANCELFALSE;
            });
            //Debug.Log($"unity log: first {first}");
            return first;
        }

        //로그아웃
        public async void SignOut()
        {
            if (_user.IsAnonymous)
                await _user.DeleteAsync();
            _auth.SignOut();
            _user = null;
        }

        //회원탈퇴
        public async void DeleteUser()
        {
            if (_user == null)
                _user = _auth.CurrentUser;
            Task task = _user.DeleteAsync();
            await task;
        }

        //닉네임을 정했는가?
        public BTask CheckUserNickName()
        {
            BTask btask = BTask.TRUE;
            if (!_user.DisplayName.IsNullOrEmpty())
                PhotonNetwork.LocalPlayer.NickName = _user.DisplayName;
            else
                btask = BTask.NNFALSE;

            return btask;
        }

        public async Task<BTask> SetUserNN(string NN)
        {
            BTask btask = BTask.TRUE;
            UserProfile profile = new UserProfile { DisplayName = NN };
            Task UpdateNN = _user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                    btask = BTask.CANCELFALSE;

                else if (task.IsFaulted)
                    btask = BTask.NNFALSE;

                else if (task.IsCompletedSuccessfully)
                {
                    PhotonNetwork.LocalPlayer.NickName = NN;
                }
            });

            await UpdateNN;
            _user = _auth.CurrentUser;
            return btask;
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