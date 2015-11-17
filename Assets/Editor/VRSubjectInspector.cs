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

        var availableBodyController = instance.GetComponents<IBodyMovementController>();
        var availableHeadController = instance.GetComponents<IHeadMovementController>();

        EditorGUILayout.LabelField("Available Head Controller");

        if (!availableHeadController.Any())
            EditorGUILayout.HelpBox("No Head Controller Implementations found! \n Attache them to this GameObject!", MessageType.Info);

        foreach (var headController in availableHeadController)
        {
            if (GUILayout.Button(headController.Identifier))
            {
                instance.ChangeHeadController(headController);
            }
        }

        EditorGUILayout.LabelField("Available Body Controller");

        if (!availableBodyController.Any())
            EditorGUILayout.HelpBox("No Body Controller Implementations found! \n Attache them to this GameObject!", MessageType.Info);

        foreach (var bodyController in availableBodyController)
        {
            if(GUILayout.Button(bodyController.Identifier))
                instance.ChangeBodyController(bodyController);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Reset Controller"))
        {
            instance.ResetController();
        }
        
        GUILayout.EndVertical();

        instance.UseMonoscopigRendering = EditorGUILayout.Toggle("Monoscopic Rendering", instance.UseMonoscopigRendering);

        if (instance.UseMonoscopigRendering)
            instance.SetMonoscopic(instance.UseMonoscopigRendering);
    }
} 