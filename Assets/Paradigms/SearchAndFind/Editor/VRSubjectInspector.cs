using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

using Assets.BeMoBI.Scripts.Controls;

[CustomEditor(typeof(VRSubjectController))]
public class VRSubjectInspector : Editor
{
    VRSubjectController instance;

    public override void OnInspectorGUI()
    {
        instance = target as VRSubjectController;

        base.OnInspectorGUI();

        GUILayout.BeginVertical();


        if (GUILayout.Button("Toggle Rectile"))
        {
            instance.ToggleRectile();
        }

        if(GUILayout.Button("Toogle Fog"))
        {
            instance.ToggleFog();
        }

        EditorGUILayout.Space();

        var availableBodyController = instance.GetComponents<IBodyMovementController>().Where(c => !(c is IHeadMovementController));
        var availableHeadController = instance.GetComponents<IHeadMovementController>().Where(c => !(c is IBodyMovementController));
        var availableCombinedController = instance.GetComponents<ICombinedControl>();

        EditorGUILayout.LabelField("Available Combi Controller");

        if (!availableCombinedController.Any())
            EditorGUILayout.HelpBox("No Combined Controller Implementations found! \n Attache them to this GameObject!", MessageType.Info);

        foreach (var combiController in availableCombinedController)
        {
            if (GUILayout.Button((combiController as IHeadMovementController).Identifier))
            {
                instance.ChangeHeadController(combiController);
                instance.ChangeBodyController(combiController);
            }
        }
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

    }
} 