using Bakers;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(RoomAdjacencyBaker))]
    public class RoomAdjacencyBakerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var s = target as RoomAdjacencyBaker;
            if(GUILayout.Button("Set Adjacency"))
            {
                s.SetRoomAdjacency();
            }
        }
    }
}