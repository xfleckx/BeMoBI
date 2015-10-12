using UnityEngine;
using UnityEditor;

using System.Collections;

[CustomEditor(typeof(MultiMazePathRetrieval))]
public class MMPREditor : Editor {

    private MultiMazePathRetrieval instance;

    public override void OnInspectorGUI()
    {
        instance = target as MultiMazePathRetrieval;

        if (GUILayout.Button("Open Control & \n Configuration Window", GUILayout.Height(40)))
        {
            var controlWindow = EditorWindow.CreateInstance<M2PRControl>();

            controlWindow.Initialize(instance);

            controlWindow.Show();
        }

        base.OnInspectorGUI();
        

    }
}
