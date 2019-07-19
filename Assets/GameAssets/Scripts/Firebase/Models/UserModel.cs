using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserModel
{
    public static readonly string UserName = "userName";
    public static readonly string IsWaiting = "isWaiting";
    public static readonly string RoomId = "roomId";
    public string userName;
    public bool isWaiting;
    public string roomId;

    public UserModel(string username, bool isWaiting, string roomId)
    {
        this.userName = username;
        this.isWaiting = isWaiting;
        this.roomId = roomId;
    }

    public UserModel(object dictObject)
    {
        if (dictObject is Dictionary<string, object>)
        {
            var dict = dictObject as Dictionary<string, object>;
            this.userName = dict.ContainsKey(UserName) ? dict[UserName].ToString() : string.Empty;
            this.isWaiting = dict.ContainsKey(IsWaiting) ? (bool)dict[IsWaiting] : false;
            this.roomId = dict.ContainsKey(RoomId) ? dict[RoomId].ToString() : string.Empty;
        }
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}
