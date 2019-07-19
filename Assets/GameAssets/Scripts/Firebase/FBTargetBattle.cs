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
    public class FBTargetBattle : FBBaseBattle
    {

        override public void Init(DatabaseReference referenceRoot, string roomId, string targetId)
        {
            base.Init(referenceRoot, roomId, targetId);
            m_referenceBattle.ChildChanged += OnTargetBattleChildChanged;
        }

        public void OnTargetBattleChildChanged(object sender, ChildChangedEventArgs args)
        {
            Debug.Log($"OnTargetBattleChildChanged");
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            else
            {
                // SetNewBattle();
                Debug.Log($"OnTargetBattleChildChanged:{args.Snapshot.Key}");
                Debug.Log(args.Snapshot.GetValue(true));
                Debug.Log(args.Snapshot.ToString());
                return;
            }
            // args.Snapshot
            // Do something with the data in args.Snapshot
        }
    }
}
