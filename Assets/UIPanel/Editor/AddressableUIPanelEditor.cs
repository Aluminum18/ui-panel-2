using UnityEditor;
using UnityEngine;

namespace UIPanel
{
    [CustomEditor(typeof(AddressableUIPanel))]
    [CanEditMultipleObjects]
    public class AddressableUIPanelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open"))
            {
                var myTarget = (AddressableUIPanel)target;
                myTarget.Open();
            }
            if (GUILayout.Button("Close"))
            {
                var myTarget = (AddressableUIPanel)target;
                myTarget.Close();
            }
            DrawDefaultInspector();
        }
    }

}