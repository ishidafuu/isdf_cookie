using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomModel
{
    public static readonly string HostId = "hostId";
    public static readonly string GuestId = "guestId";
    public static readonly string Phase = "phase";
    public static readonly string IsGuestReady = "isGuestReady";

    public string hostId;
    public string guestId;
    public int phase;
    public bool isGuestReady;

    public RoomModel(string hostUserId, string guestUserId)
    {
        this.hostId = hostUserId;
        this.guestId = guestUserId;
        this.phase = 0;
        this.isGuestReady = false;
    }
}
