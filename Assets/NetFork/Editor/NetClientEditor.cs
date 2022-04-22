using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NetClient))]
public class NetClientEditor : Editor 
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
        if (GUILayout.Button("Start Client"))
        {
            var targetObject = target as NetClient;
            targetObject.Connect();
        }

        if (GUILayout.Button("Stop Client"))
        {
            var targetObject = target as NetClient;
            targetObject.StopClient();
        }
    }
}
