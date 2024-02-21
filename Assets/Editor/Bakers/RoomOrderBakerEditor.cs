using System.Linq;
using Bakers;
using UnityEditor;
using UnityEngine;
using World;

namespace Editor
{
    [CustomEditor(typeof(RoomOrderBaker))]
    public class RoomOrderBakerEditor : UnityEditor.Editor
    {
        private bool isIsolated;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var s = target as RoomOrderBaker;
            if(GUILayout.Button("Populate Rooms"))
            {
                s.PopulateRooms();
            }
            if(GUILayout.Button("Set Doors"))
            {
                s.SetDoors();
            }
            if(GUILayout.Button("Clear"))
            {
                s.Clear();
            }
            
            if(GUILayout.Button("Toggle Isolate Rooms"))
            {
                ToggleIsolate();
            }
        }

        private void ToggleIsolate()
        {
            isIsolated = !isIsolated;
            
            SceneVisibilityManager visMan = SceneVisibilityManager.instance;
            
            if (isIsolated)
            {
                Room[] rooms = FindObjectsOfType<Room>();
                var groundsE = rooms.Select(r => r.transform.Find("Grid/Ground").gameObject);
                GameObject[] grounds = groundsE.ToArray();

                GameObject[] elevators = FindObjectsOfType<Elevator>().Select(e => e.gameObject).ToArray();
                
                // GameObject roomList = FindObjectOfType<RoomList>().gameObject;
                GameObject self = (target as RoomOrderBaker)?.gameObject;
                visMan.Isolate(grounds, true);
                visMan.Show(self, true);
                visMan.Show(elevators, false);
            }
            else
            {
                visMan.ExitIsolation();
            }
        }
    }
}