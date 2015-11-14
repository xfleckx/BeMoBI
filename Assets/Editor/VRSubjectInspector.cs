using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VRSubjectController))]
public class VRSubjectInspector : Editor
{
    VRSubjectController instance;

    public override void OnInspectorGUI()
    {
        instance = target as VRSubjectController;

        base.OnInspectorGUI();

        GUILayout.BeginVertical();

        if (GUILayout.Button("Recenter"))
            instance.RecenterHeading();

        if (GUILayout.Button("Mouse & Keyboard"))
        {
            instance.SetInputMethod(instance.InitMouseKeyboardInput, instance.MouseKeyboardInput);
        }
        

        if (GUILayout.Button("Keyboard + RIFT"))
        {
            instance.SetInputMethod(() => { }, instance.KeyboardRiftInput);
        }

        if (GUILayout.Button("Rift + XBox Ctrl"))
        {
            instance.SetMultipleInputMethods(instance.RiftOnlyInput, instance.XBoxActions);
        }

        GUILayout.EndVertical();
    }
} 