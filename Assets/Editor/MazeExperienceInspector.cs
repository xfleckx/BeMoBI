using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MazeExperience))]
public class MazeExperienceInspector : Editor
{
    MazeExperience instance;

    public override void OnInspectorGUI()
    {
        instance = target as MazeExperience;

        base.OnInspectorGUI();

        if (GUILayout.Button("Start Paradígm"))
        {
            instance.StartParadigm();
        }

        if (GUILayout.Button("InitTrial"))
        {
            instance.InitTrial();
        }

        if (GUILayout.Button("Set Player Height"))
        {
            instance.SetTestHeight();
        }
    }
} 