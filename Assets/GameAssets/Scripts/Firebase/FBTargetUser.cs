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
    public class FBTargetUser
    {
        DatabaseReference m_referenceTargetUser;

        public void Init(DatabaseReference referenceRoot, string targetUserId)
        {
            m_referenceTargetUser = referenceRoot.Child(FBConstants.Users).Child(targetUserId);
        }

        public void UpdateGuestData(UserModel updateTarget, string roomId)
        {
            updateTarget.isWaiting = false;
            updateTarget.roomId = roomId;
            string json = JsonUtility.ToJson(updateTarget);
            m_referenceTargetUser.SetRawJsonValueAsync(json);
        }

    }
}
