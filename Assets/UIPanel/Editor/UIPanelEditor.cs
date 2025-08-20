using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UIPanel
{
    [CustomEditor(typeof(UIPanel))]
    [CanEditMultipleObjects]
    public class UIPanelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open"))
            {
                var myTarget = (UIPanel)target;
                myTarget.Open();
            }
            if (GUILayout.Button("Close"))
            {
                var myTarget = (UIPanel)target;
                myTarget.Close();
            }
            DrawDefaultInspector();
        }
    }
}
