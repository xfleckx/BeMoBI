using UnityEngine;
using UnityEditor;
using Assets.Paradigms.MultiMazePathRetrieval.Scripts.Controls;

[CustomEditor(typeof(OculusRift))]
public class OculusHeadControlInspector : Editor
{
    OculusRift instance;

    public override void OnInspectorGUI()
    {
        instance =  target as OculusRift;

        instance.UseMonoscopigRendering = EditorGUILayout.Toggle("Monoscopic Rendering", instance.UseMonoscopigRendering);

        if (GUILayout.Button("Recenter"))
        {
            instance.Recenter();
        }
    }
}