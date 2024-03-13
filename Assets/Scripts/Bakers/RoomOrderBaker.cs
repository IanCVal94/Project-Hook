using System.Collections.Generic;
using ASK.Helpers;
using UnityEditor;
using UnityEngine;
using World;

namespace Bakers
{
    public class RoomOrderBaker : MonoBehaviour, IBaker
    {
        [SerializeField] private List<Room> rooms = new();
        [SerializeField] private Gradient colors;

        [SerializeField] private bool showArrowsElevators;
        [SerializeField] private bool showArrowsRooms;
        [SerializeField] private bool showLabels;

        
        public void PopulateRooms()
        {
            rooms = new List<Room>(FindObjectsOfType<Room>());
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        public void SetDoors()
        {
            ClearDoors();
            for (int i = 0; i < rooms.Count; ++i)
            {
                Room curRoom = rooms[i];
                
                if (i != rooms.Count - 1) curRoom.SetNextRoom(rooms[i + 1]);
                
                #if UNITY_EDITOR
                EditorUtility.SetDirty(curRoom);
                #endif
            }
        }

        public void ClearDoors()
        {
            foreach (var r in FindObjectsOfType<Room>())
            {
                var e = r.GetComponentInChildren<ElevatorOut>();
                e.Destination = null;
                #if UNITY_EDITOR
                EditorUtility.SetDirty(e);
                #endif
            }
        }

        public void Clear()
        {
            rooms = new ();
            ClearDoors();
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        public void Bake()
        {
            SetDoors();
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            for (var i = 0; i < rooms.Count; i++)
            {
                var r = rooms[i];
                Color c = colors.Evaluate((float)i / rooms.Count);
                
                Vector3 extents = r.GetExtents();
                Vector3 boxOrigin = r.GetCenter();

                if (r.ElevatorOut == null || r.ElevatorIn == null) continue;
                
                if (showArrowsElevators && r.ElevatorOut.Destination is { } d)
                {
                    Vector3 from = r.ElevatorOut.transform.position;
                    Vector3 to = d.GetComponentInParent<Room>().ElevatorIn.transform.position;
                    Handles.DrawDottedLine(from, to, 5);
                }
                
                if (showArrowsRooms && i < rooms.Count - 1)
                {
                    Room nextRoom = rooms[i+1];
                    Vector3 from = boxOrigin;
                    Vector3 to = nextRoom.GetCenter();
                    Helper.DrawArrow(from, to - from, c, 100);
                }

                if (showLabels)
                {
                    GUIStyle style = new();
                    style.fontSize = 32;
                    style.normal.textColor = c;
                    Handles.Label(boxOrigin, $"{r.name} ({i})", style);
                }
                
                BoxDrawer.DrawBox(boxOrigin, extents, Quaternion.identity, c);
            }
        }
        #endif
    }
}