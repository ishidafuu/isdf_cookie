using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine;

namespace NKPB
{
    public class FBMyUser
    {
        DatabaseReference m_referenceMyUser;
        Action<string> m_joinRoom;

        string m_userId;
        string m_userName;
        public string GetUserId() => m_userId;
        public string GetUserName() => m_userName;

        public void Init(DatabaseReference referenceRoot, string userName)
        {
            m_userName = userName;
            m_userId = LoadOrCreateMyUserId();
            UserModel user = new UserModel(userName, false, string.Empty);
            string json = JsonUtility.ToJson(user);
            m_referenceMyUser = referenceRoot.Child(FBConstants.Users).Child(m_userId);
            m_referenceMyUser.SetRawJsonValueAsync(json);
            m_referenceMyUser.ChildChanged += OnMyUserChildChanged;
            m_joinRoom = null;
        }

        private string LoadOrCreateMyUserId()
        {
            string userId = PlayerPrefs.GetString(FBConstants.UserId, string.Empty);
            if (userId == string.Empty)
            {
                userId = System.Guid.NewGuid().ToString();
                PlayerPrefs.SetString(FBConstants.UserId, userId);
                PlayerPrefs.Save();
            }
            Debug.Log($"userId:{m_userId}");

            return userId;
        }

        public void UpdateMyUser(bool isWaiting, string roomId)
        {
            UserModel update = new UserModel(m_userName, isWaiting, roomId);
            m_referenceMyUser.SetRawJsonValueAsync(update.ToJson());
        }

        public void SetJoinRoom(Action<string> joinRoom)
        {
            m_joinRoom = joinRoom;
        }

        void OnMyUserChildChanged(object sender, ChildChangedEventArgs args)
        {
            Debug.Log($"OnMyUserChildChanged");
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            else
            {
                Debug.Log($"OnMyUserChildChanged:{args.Snapshot.Key}{args.Snapshot.GetValue(true)}{args.Snapshot.Key == UserModel.IsWaiting}");
                if (args.Snapshot.Key == UserModel.IsWaiting)
                {
                    if ((bool)args.Snapshot.GetValue(true) == false)
                    {
                        GetJoinRoomId();
                    }
                }
                return;
            }
        }

        void GetJoinRoomId()
        {
            Debug.Log($"GetJoinRoomId");
            m_referenceMyUser.Child(UserModel.RoomId)
                .GetValueAsync()
                .ContinueWith((System.Action<System.Threading.Tasks.Task<DataSnapshot>>)(task =>
                {
                    if (task.Exception != null)
                    {
                        Debug.LogError(task.Exception);
                    }
                    else if (task.IsCompleted)
                    {
                        if (m_joinRoom != null)
                        {
                            string roomId = task.Result.GetValue(true).ToString();
                            m_joinRoom(roomId);
                        }
                    }
                    else
                    {
                        Debug.LogWarning(task);
                    }
                }));
        }
    }
}
