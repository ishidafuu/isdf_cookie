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
    public static class FireBaseManager
    {
        static DatabaseReference m_referenceRoot;
        static DatabaseReference m_referenceUsers;
        static DatabaseReference m_referenceMyUser;
        static DatabaseReference m_referenceMyRoom;
        static DatabaseReference m_referenceMyBattle;
        static DatabaseReference m_referenceTargetBattle;

        static string m_roomId;
        static string m_userId;
        static string m_myName;
        static string m_targetName;
        static string m_targetId;
        static bool m_isHost;

        static readonly string DatabaseUrlId = "https://isdfcookie.firebaseio.com/";
        static readonly string P12FileName = "isdfcookie-e06f173e25c9.p12";
        static readonly string ServiceAccountEmail = "isdfcookie@appspot.gserviceaccount.com";
        static readonly string P12Password = "notasecret";
        static readonly string UserId = "userId";

        static readonly string Users = "users";
        static readonly string Rooms = "rooms";
        static readonly string Battles = "battles";

        public static void Init()
        {
            FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(DatabaseUrlId);
            FirebaseApp.DefaultInstance.SetEditorP12FileName(P12FileName);
            FirebaseApp.DefaultInstance.SetEditorServiceAccountEmail(ServiceAccountEmail);
            FirebaseApp.DefaultInstance.SetEditorP12Password(P12Password);
            m_referenceRoot = FirebaseDatabase.DefaultInstance.RootReference;
            // m_referenceRoot.ChildChanged += OnChildChanged;
            m_referenceUsers = m_referenceRoot.Child(Users);
            m_myName = "isdf";
            m_userId = PlayerPrefs.GetString(UserId, string.Empty);
            if (m_userId == string.Empty)
            {
                m_userId = System.Guid.NewGuid().ToString();
                PlayerPrefs.SetString(UserId, m_userId);
                PlayerPrefs.Save();
            }
            Debug.Log($"userId:{m_userId}");
            SetNewUser(m_userId, m_myName);

            var query = m_referenceUsers
                .OrderByChild(UserModel.IsWaiting)
                .EqualTo(true)
                .LimitToFirst(1);

            query.GetValueAsync().ContinueWith((System.Action<System.Threading.Tasks.Task<DataSnapshot>>)(task =>
            {
                if (task.Exception != null)
                {
                    Debug.LogError(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    var result = snapshot.GetValue(true)as Dictionary<string, object>;
                    Debug.Log(snapshot.GetRawJsonValue());
                    Debug.Log(result.Count);

                    if (result.Count > 0)
                    {
                        MakeRoom(result);
                    }
                    else
                    {
                        WaitRoom();
                    }
                }
                else
                {
                    Debug.LogWarning(task);
                }

            }));

        }

        private static void WaitRoom()
        {
            UserModel update = new UserModel(m_myName, true, string.Empty);
            m_referenceMyUser.SetRawJsonValueAsync(update.ToJson());
        }

        private static void MakeRoom(Dictionary<string, object> result)
        {
            UserModel updateTarget = new UserModel(result.Values.First());
            FireBaseManager.SetNewRoom(m_userId);
            m_targetId = result.Keys.First();
            m_targetName = updateTarget.userName;
            m_isHost = true;
            UserModel updateMe = new UserModel(m_myName, false, m_roomId);
            updateTarget.roomId = m_roomId;
            updateTarget.isWaiting = false;

            m_referenceMyUser.SetRawJsonValueAsync(updateMe.ToJson());
            m_referenceUsers.Child(m_targetId).SetRawJsonValueAsync(updateTarget.ToJson());
            UpdateReferenceTargetBattle();
        }

        private static void UpdateReferenceTargetBattle()
        {
            m_referenceTargetBattle = m_referenceRoot.Child(Battles).Child(m_roomId).Child(m_targetId);
            m_referenceTargetBattle.ChildChanged += OnTargetBattleChildChanged;
        }

        static private void SetNewUser(string userId, string name)
        {
            Debug.Log("SetNewUser");
            UserModel user = new UserModel(name, false, string.Empty);
            string json = JsonUtility.ToJson(user);
            m_referenceMyUser = m_referenceRoot.Child(Users).Child(userId);
            m_referenceMyUser.SetRawJsonValueAsync(json);
            m_referenceMyUser.ChildChanged += OnMyUserChildChanged;
        }

        static private void SetNewRoom(string hostsUserId)
        {
            Debug.Log("SetNewRoom");
            m_roomId = System.Guid.NewGuid().ToString();
            RoomModel room = new RoomModel(hostsUserId);
            string json = JsonUtility.ToJson(room);
            m_referenceMyRoom = m_referenceRoot.Child(Rooms).Child(m_roomId);
            m_referenceMyRoom.SetRawJsonValueAsync(json);
            m_referenceMyRoom.ChildChanged += OnMyRoomChildChanged;
        }

        static private void SetNewBattle()
        {
            Debug.Log("SetNewBattle");
            BattleModel battle = new BattleModel("", 0);
            string json = JsonUtility.ToJson(battle);
            m_referenceMyBattle = m_referenceRoot.Child(Battles).Child(m_roomId).Child(m_userId);
            m_referenceMyBattle.Child(DateTime.Now.ToString("yyMMddHHmmssfff")).SetRawJsonValueAsync(json);

            if (!m_isHost)
            {
                m_referenceMyRoom.Child(RoomModel.GuestId).SetValueAsync(m_userId);
            }
        }

        static private void AddBattle(string data)
        {
            Debug.Log("AddBattle");
            BattleModel battle = new BattleModel("", 0);
            string json = JsonUtility.ToJson(battle);
            m_referenceMyBattle.Push().SetRawJsonValueAsync(json);

        }

        // static private string WriteBattleField(string fieldData)
        // {

        //     m_referenceMyRoom = m_referenceRoot.Child(Rooms).Child(roomId);
        //     m_referenceMyRoom.SetRawJsonValueAsync(json);

        //     return roomId;
        // }

        static void OnMyUserChildChanged(object sender, ChildChangedEventArgs args)
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
                // Debug.Log($"OnChildChanged:{args.Snapshot.Key}");
                // Debug.Log(args.Snapshot.GetValue(true));
                // Debug.Log(args.Snapshot.ToString());
                return;
            }
            // args.Snapshot
            // Do something with the data in args.Snapshot
        }

        private static void GetJoinRoomId()
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
                        Debug.Log($"GetJoinRoomId{task.Result.GetValue(true)}");
                        m_roomId = task.Result.GetValue(true).ToString();
                        m_referenceMyRoom = m_referenceRoot.Child(Rooms).Child(m_roomId);
                        GetRoomHostId();
                    }
                    else
                    {
                        Debug.LogWarning(task);
                    }

                }));
        }

        private static void GetRoomHostId()
        {
            Debug.Log($"GetRoomHostId");
            m_referenceMyRoom.Child(RoomModel.HostId)
                .GetValueAsync().ContinueWith((System.Action<System.Threading.Tasks.Task<DataSnapshot>>)(task =>
                {
                    if (task.Exception != null)
                    {
                        Debug.LogError(task.Exception);
                    }
                    else if (task.IsCompleted)
                    {
                        Debug.Log($"GetTargetId{task.Result.GetValue(true)}");
                        m_targetId = task.Result.GetValue(true).ToString();
                        UpdateReferenceTargetBattle();
                        SetNewBattle();
                        m_referenceMyRoom.Child(RoomModel.IsGuestReady).SetValueAsync(true);
                    }
                    else
                    {
                        Debug.LogWarning(task);
                    }

                }));
        }

        static void OnMyRoomChildChanged(object sender, ChildChangedEventArgs args)
        {
            Debug.Log($"OnMyRoomChildChanged");
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            else
            {
                Debug.Log($"OnMyRoomChildChanged:{args.Snapshot.Key}{args.Snapshot.GetValue(true)}");
                if (args.Snapshot.Key == RoomModel.IsGuestReady)
                {
                    if ((bool)args.Snapshot.GetValue(true))
                    {
                        SetNewBattle();
                    }
                }
                return;
            }
            // args.Snapshot
            // Do something with the data in args.Snapshot
        }

        static void OnTargetBattleChildChanged(object sender, ChildChangedEventArgs args)
        {
            Debug.Log($"OnTargetBattleChildChanged");
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            else
            {
                SetNewBattle();
                Debug.Log($"OnTargetBattleChildChanged:{args.Snapshot.Key}");
                Debug.Log(args.Snapshot.GetValue(true));
                Debug.Log(args.Snapshot.ToString());
                return;
            }
            // args.Snapshot
            // Do something with the data in args.Snapshot
        }

        // static void OnValueChanged(object sender, ValueChangedEventArgs args)
        // {
        //     if (args.DatabaseError != null)
        //     {
        //         Debug.LogError(args.DatabaseError.Message);
        //         return;
        //     }
        //     else
        //     {
        //         Debug.Log($"OnValueChanged:{args.Snapshot.GetRawJsonValue()}");
        //         return;
        //     }
        //     // args.Snapshot
        //     // Do something with the data in args.Snapshot
        // }

    }
}
