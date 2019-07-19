using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleModel
{
    public string fieldData;
    public int attackData;

    public BattleModel(string fieldData, int attackData)
    {
        this.fieldData = fieldData;
        this.attackData = attackData;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static BattleModel FromJson(string json)
    {
        return JsonUtility.FromJson<BattleModel>(json);
    }
}
