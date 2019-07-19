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
    public class FBBaseBattle
    {
        protected DatabaseReference m_referenceBattle;

        virtual public void Init(DatabaseReference referenceRoot, string roomId, string userId)
        {
            m_referenceBattle = referenceRoot.Child(FBConstants.Battles).Child(roomId).Child(userId);
        }

        public void PutBattleData(string fieldData, int attackData)
        {
            Debug.Log("PutBattleData");
            BattleModel battle = new BattleModel(fieldData, attackData);
            string json = JsonUtility.ToJson(battle);
            string date = DateTime.Now.ToString("yyMMddHHmmssfff");
            m_referenceBattle.Child(date).SetRawJsonValueAsync(json);
        }
    }
}
