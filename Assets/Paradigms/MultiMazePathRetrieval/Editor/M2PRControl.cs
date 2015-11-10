using UnityEngine;
using System.Collections;
using UnityEditor;

public class M2PRControl : EditorWindow {

    private MultiMazePathRetrieval instance;

    internal void Initialize(MultiMazePathRetrieval target)
    {
        instance = target;
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.Width(150));

        RenderControlGUI();

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUILayout.Width(200));

        RenderConfigurationGUI();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

    }

    private void RenderControlGUI()
    {
        GUILayout.Label("Control", EditorStyles.largeLabel);

        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("To use the controls, please start the playmode!", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Start Instruction Trial"))
        {
            instance.Begin(instance.instruction);
        }

        if (GUILayout.Button("Start Pause Trial"))
        {
            instance.Begin(instance.pause);
        }

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Start Training Trial"))
        {
            instance.Begin(instance.training);
        }

        if (GUILayout.Button("Start Experiment Trial"))
        {
            instance.Begin(instance.experiment);
        }
    }

    private void RenderConfigurationGUI()
    {

        GUILayout.Label("Configuration", EditorStyles.largeLabel);

    }
}
