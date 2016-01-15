using UnityEngine;
using UnityEditor;
using Assets.BeMoBI.Scripts.Controls;
using System;

[CustomEditor(typeof(OculusRiftController))]
public class OculusHeadControlInspector : Editor
{
    OculusRiftController instance;

    private float lastIPDValue;

    public override void OnInspectorGUI()
    {
        instance =  target as OculusRiftController;

        instance.UseMonoscopigRendering = EditorGUILayout.Toggle("Monoscopic Rendering", instance.UseMonoscopigRendering);

        EditorGUILayout.LabelField("IPD", lastIPDValue.ToString());

        if (GUILayout.Button("Set Mono per IPD to zero"))
        {
            instance.ChangeIPDValue(0f); // Won't work... :/
            lastIPDValue = instance.IPD;
        }

        if (GUILayout.Button("Reset IPD value"))
        {
            instance.RestoreOriginalIpd();
            lastIPDValue = instance.IPD;
        }
        
        if (GUILayout.Button("Recenter"))
        {
            instance.Recenter();
        }
    }
}