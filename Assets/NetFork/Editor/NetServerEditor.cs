using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NetServer))]
public class NetServerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        ShowControlButtons();
        EditorGUILayout.EndHorizontal();
    }

    private void ShowControlButtons()
    {
        if (GUILayout.Button("Start Server"))
        {
            var targetObject = target as NetServer;
            targetObject.StartServer();
        }

        if (GUILayout.Button("Stop Server"))
        {
            var targetObject = target as NetServer;
            targetObject.StopServer();
        }
    }
}
