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
using Photon.Pun;
using WebSocketSharp;

namespace SpaceBangBang
{
    public partial class DataManager
    {
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
    }
}
