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
            m_users.SearchWaitingUser(MakeRoom, WaitRoom);
        }

        static void MakeRoom(Dictionary<string, object> result)
        {
            m_myRoom.InitHost(m_referenceRoot, m_myUser.GetUserId());
            m_myUser.UpdateMyUser(false, m_myRoom.GetRoomId());
            m_myBattle.Init(m_referenceRoot, m_myRoom.GetRoomId(), m_myUser.GetUserId());

            UserModel updateTarget = new UserModel(result.Values.First());
            m_targetUser.UpdateGuestData(updateTarget, m_myRoom.GetRoomId());

            string targetId = result.Keys.First();
            SetTargetUserAndBattle(targetId);
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
        }

        static void SetTargetUserAndBattle(string targetId)
        {
            m_targetUser.Init(m_referenceRoot, targetId);
            m_targetBattle.Init(m_referenceRoot, m_myRoom.GetRoomId(), targetId);
        }

    }
}
