using MyBox;
using UnityEditor;
using UnityEngine;
using World;

namespace Bakers
{
    public class RoomAdjacencyBaker : MonoBehaviour, IBaker
    {
        [Foldout("Adjacency Settings", true)]
        [SerializeField] private Vector2 RoomAdjacencyTolerance;
        [SerializeField] private LayerMask RoomLayerMask;
        
        public void SetRoomAdjacency()
        {
            Room[] rooms = FindObjectsOfType<Room>();
            foreach (var room in rooms)
            {
                Room[] adjRooms = room.CalcAdjacentRooms(RoomAdjacencyTolerance, RoomLayerMask);
                room.AdjacentRooms = adjRooms;
                #if UNITY_EDITOR
                EditorUtility.SetDirty(room);
                #endif
            }
        }

        public void Bake()
        {
            SetRoomAdjacency();
        }
    }
}