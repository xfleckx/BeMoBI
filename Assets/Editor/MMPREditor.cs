using UnityEngine;
using UnityEditor;

using System.Collections;

[CustomEditor(typeof(MultiMazePathRetrieval))]
public class MMPREditor : Editor {

    private MultiMazePathRetrieval instance;

    public override void OnInspectorGUI()
    {
        instance = target as MultiMazePathRetrieval;

        base.OnInspectorGUI();

        if (GUILayout.Button("Start Training"))
        {
            instance.Begin(instance.training);
        }

        //if (GUILayout.Button("Start Experiment"))
        //{
        //    instance.BeginTraining();
        //}
    }
}
