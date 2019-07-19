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
    public class FBMyBattle : FBBaseBattle
    {
        override public void Init(DatabaseReference referenceRoot, string roomId, string userId)
        {
            base.Init(referenceRoot, roomId, userId);
        }
    }
}
