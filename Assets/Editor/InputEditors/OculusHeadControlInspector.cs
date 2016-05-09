using UnityEngine;
using UnityEditor;
using Assets.BeMoBI.Scripts.Controls;
using System;

[CustomEditor(typeof(OculusRiftController))]
public class OculusHeadControlInspector : Editor
{
    OculusRiftController instance;

    private float lastIPDValue;

    private float newIpdValue = 0.064f;

    public override void OnInspectorGUI()
    {
        instance =  target as OculusRiftController;

        instance.UseMonoscopigRendering = EditorGUILayout.Toggle("Monoscopic Rendering", instance.UseMonoscopigRendering);

        EditorGUILayout.LabelField("IPD", lastIPDValue.ToString());

        if(GUILayout.Button("Update Rift Config Values"))
        {
            instance.RequestConfigValues();
        }

        if (GUILayout.Button("Set Mono per IPD to zero"))
        {
            instance.ChangeIPDValue(0f); // Won't work... :/
            lastIPDValue = instance.currentIPD;
        }

        newIpdValue = EditorGUILayout.FloatField("new Ipd", newIpdValue);

        if(GUILayout.Button("Set IPD"))
        {
            instance.ChangeIPDValue(newIpdValue);
        }

        if (GUILayout.Button("Reset IPD value"))
        {
            instance.RestoreOriginalIpd();
            lastIPDValue = instance.currentIPD;
        }
        
        if (GUILayout.Button("Recenter"))
        {
            instance.Recenter();
        }
    }
}