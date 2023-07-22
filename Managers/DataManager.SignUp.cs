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
        //�̸��� �α���
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
                    //���� �� �� email ȸ�������� ��
                    if (site == Sites.None && !_user.IsEmailVerified)
                        btask = BTask.VERIFYFALSE;
                }
            });

            await task;

            return btask;
        }

        //�̸��ϰ� ��й�ȣ�� ���̾�̽� ���Ŀ� �´��� üũ
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

        //�̸��ϰ� ��й�ȣ�� ���̾�̽� ���Ŀ� �´��� üũ
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

        //���� ȸ������
        public async Task<BTask> SignUpUserAsync(string email, string pw)
        {
            //TODO - ȸ�������� ���� ���´µ� ���� �������� ���� ���¿���
            //��й�ȣ�� �ٲ㼭 �ٽ� ȸ�������� ��û�ϴ� ��� - �� ���� �۽�...
            //�̹� �ٸ� ������� ȸ�������� �� ���

            BTask btask = await BFirstSign(email);

            if (btask != BTask.CHECKFALSE)
            {
                Task task = _auth.CreateUserWithEmailAndPasswordAsync(email, pw).ContinueWithOnMainThread(task =>
                {
                    //password�� Ʋ�����Ƿ� �� ���� ȸ�� �̸��Ϸ� ���� �ް� ����ٲٱ�
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

        //���Խ�û�� �̸��Ͽ� �޼����� ����
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

        //������ �̸��� ������ �ߴ���
        public async Task<BTask> VerifiyUserAsync()
        {
            bool bverified = _user.IsEmailVerified;
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

            //������ �����Ҷ����� ������
            while (!bverified)
            {
                //1�ʵ��� ����
                await Task.Delay(1000);

                //5�е��� �� ������ ���������� return false
                if (cts.IsCancellationRequested)
                    break;

                //������ �����ߴ��� Ȯ��
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
                    //task.Result.First()�� �ֱ� �α����� �α��� ����� ���´�.
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

        //�α׾ƿ�
        public async void SignOut()
        {
            if (_user.IsAnonymous)
                await _user.DeleteAsync();
            _auth.SignOut();
            _user = null;
        }

        //ȸ��Ż��
        public async void DeleteUser()
        {
            if (_user == null)
                _user = _auth.CurrentUser;
            Task task = _user.DeleteAsync();
            await task;
        }
    }
}
