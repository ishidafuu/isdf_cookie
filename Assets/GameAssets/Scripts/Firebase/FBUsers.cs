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
    public class FBUsers
    {
        DatabaseReference m_referenceUsers;

        public void Init(DatabaseReference referenceRoot)
        {
            m_referenceUsers = referenceRoot.Child(FBConstants.Users);
        }

        public void SearchWaitingUser(Action<Dictionary<string, object>> makeRoom, Action waitRoom)
        {
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
                        makeRoom(result);
                    }
                    else
                    {
                        waitRoom();
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
