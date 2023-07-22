using SpaceBangBang;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using Firebase.Auth;
using Firebase.Extensions;
using System.Linq;
using WebSocketSharp;

namespace SpaceBangBang
{
    public partial class DataManager
    {
        #region Email
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
    }
}
