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
        static FBUsers m_users = new FBUsers();
        static FBMyRoom m_myRoom = new FBMyRoom();
        static FBMyUser m_myUser = new FBMyUser();
        static FBMyBattle m_myBattle = new FBMyBattle();
        static FBTargetUser m_targetUser = new FBTargetUser();
        static FBTargetBattle m_targetBattle = new FBTargetBattle();

        static DatabaseReference m_referenceRoot;

        public static void Init()
        {
            FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(FBConstants.DatabaseUrlId);
            FirebaseApp.DefaultInstance.SetEditorP12FileName(FBConstants.P12FileName);
            FirebaseApp.DefaultInstance.SetEditorServiceAccountEmail(FBConstants.ServiceAccountEmail);
            FirebaseApp.DefaultInstance.SetEditorP12Password(FBConstants.P12Password);
            m_referenceRoot = FirebaseDatabase.DefaultInstance.RootReference;

            m_users.Init(m_referenceRoot);
            m_myUser.Init(m_referenceRoot, "isdf");
            RemoveLastRoom();
        }

        static void MakeRoom(Dictionary<string, object> result)
        {
            UserModel updateTarget = new UserModel(result.Values.First());
            string targetId = result.Keys.First();

            m_myRoom.InitHost(m_referenceRoot, m_myUser.GetUserId(), targetId);
            m_myUser.UpdateMyUser(false, m_myRoom.GetRoomId());
            m_myBattle.Init(m_referenceRoot, m_myRoom.GetRoomId(), m_myUser.GetUserId());

            SetTargetUserAndBattle(targetId);
            m_targetUser.UpdateGuestData(updateTarget, m_myRoom.GetRoomId());

            PutBattleData("", 0);
        }

        static void WaitRoom()
        {
            m_myUser.UpdateMyUser(true, string.Empty);
            m_myUser.SetJoinRoom(JoinRoom);
        }

        static void JoinRoom(string roomId)
        {
            m_myRoom.InitGuest(m_referenceRoot, roomId, SetTargetUserAndBattle);
            m_myBattle.Init(m_referenceRoot, m_myRoom.GetRoomId(), m_myUser.GetUserId());
            PutBattleData("", 0);
        }

        static void SetTargetUserAndBattle(string targetId)
        {
            m_targetUser.Init(m_referenceRoot, targetId);
            m_targetBattle.Init(m_referenceRoot, m_myRoom.GetRoomId(), targetId);
        }

        public static void PutBattleData(string fieldData, int attackData)
        {
            m_myBattle.PutBattleData(fieldData, attackData);
        }

        public static void RemoveLastRoom()
        {
            m_referenceRoot
                .Child(FBConstants.Rooms)
                .Child(m_myUser.GetUserId())
                .RemoveValueAsync()
                .ContinueWith((System.Action<System.Threading.Tasks.Task>)(task =>
                {
                    if (task.Exception != null)
                    {
                        Debug.LogError(task.Exception);
                    }
                    else if (task.IsCompleted)
                    {
                        RemoveLastBattle();
                    }
                    else
                    {
                        Debug.LogWarning(task);
                    }
                }));

        }

        public static void RemoveLastBattle()
        {
            m_referenceRoot
                .Child(FBConstants.Battles)
                .Child(m_myUser.GetUserId())
                .RemoveValueAsync()
                .ContinueWith((System.Action<System.Threading.Tasks.Task>)(task =>
                {
                    if (task.Exception != null)
                    {
                        Debug.LogError(task.Exception);
                    }
                    else if (task.IsCompleted)
                    {
                        m_users.SearchWaitingUser(MakeRoom, WaitRoom);
                    }
                    else
                    {
                        Debug.LogWarning(task);
                    }
                }));

        }
    }
}
