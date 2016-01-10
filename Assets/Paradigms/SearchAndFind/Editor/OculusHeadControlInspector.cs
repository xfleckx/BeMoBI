using UnityEngine;
using UnityEditor;
using Assets.BeMoBI.Scripts.Controls;

[CustomEditor(typeof(OculusRiftController))]
public class OculusHeadControlInspector : Editor
{
    OculusRiftController instance;

    public override void OnInspectorGUI()
    {
        instance =  target as OculusRiftController;

        instance.UseMonoscopigRendering = EditorGUILayout.Toggle("Monoscopic Rendering", instance.UseMonoscopigRendering);

        if (GUILayout.Button("Recenter"))
        {
            instance.Recenter();
        }
    }
}