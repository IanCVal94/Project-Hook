using UnityEditor;
using UnityEngine;
using VFX;

namespace Editor
{
    [CustomEditor(typeof(Pendulum), true)]
    public class PendulumEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var script = target as Pendulum;
            if (GUILayout.Button("Reset Rotation")) script.ResetRot(script.EditorMaxRot);
        }
    }
}