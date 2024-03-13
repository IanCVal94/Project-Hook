using ASK.Helpers;
using System.Collections.Generic;
using UnityEngine;
using World;

public class RoomList : MonoBehaviour, IFilterLoggerTarget
{
    public static List<Room> Rooms;

    [SerializeField] private Room firstRoom;
    
    private void Awake()
    {
        Rooms = new List<Room>(GetComponentsInChildren<Room>(true));
        if (firstRoom == null) firstRoom = Rooms[0];
        foreach (var r in Rooms)
        {
            if (r != firstRoom) r.RoomSetEnable(false);
        }
    }

    public LogLevel GetLogLevel()
    {
        return LogLevel.Info;
    }
}
