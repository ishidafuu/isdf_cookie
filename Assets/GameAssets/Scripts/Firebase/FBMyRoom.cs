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
    public class FBMyRoom
    {
        DatabaseReference m_referenceMyRoom;

        string m_roomId;

        bool m_isHost;
        bool m_isReady;

        public string GetRoomId() => m_roomId;
        public bool IsHost() => m_isHost;
        public bool IsReady() => m_isHost;

        public void InitHost(DatabaseReference referenceRoot, string hostsUserId, string guestUserId)
        {
            Init(referenceRoot, true, hostsUserId);

            RoomModel room = new RoomModel(hostsUserId, guestUserId);
            string json = JsonUtility.ToJson(room);
            m_referenceMyRoom.SetRawJsonValueAsync(json);
            m_referenceMyRoom.ChildChanged += OnHostMyRoomChildChanged;
        }

        public void InitGuest(DatabaseReference referenceRoot, string roomId, Action<string> setTarget)
        {
            Init(referenceRoot, false, roomId);

            m_isReady = true;
            m_referenceMyRoom.Child(RoomModel.IsGuestReady).SetValueAsync(m_isReady);
            SetTarget(setTarget);
        }

        void Init(DatabaseReference referenceRoot, bool isHost, string roomId)
        {
            m_isHost = isHost;
            m_roomId = roomId;
            m_isReady = false;
            m_referenceMyRoom = referenceRoot.Child(FBConstants.Rooms).Child(m_roomId);
        }

        void OnHostMyRoomChildChanged(object sender, ChildChangedEventArgs args)
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
                        m_isReady = true;
                    }
                }
                return;
            }
        }

        private void SetTarget(Action<string> setTarget)
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
                        string targetId = task.Result.GetValue(true).ToString();
                        setTarget(targetId);
                    }
                    else
                    {
                        Debug.LogWarning(task);
                    }
                }));
        }
    }
}
