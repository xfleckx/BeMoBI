using UnityEngine;
using UnityEditor;
using System.Collections;
using Assets.BeMoBI.Scripts.Controls;

[CustomEditor(typeof(JoystickControl))]
public class JoystickInspector : Editor {

    private JoystickControl instance;
     
    public override void OnInspectorGUI()
    {
        instance = target as JoystickControl;

        base.OnInspectorGUI();
        
        EditorGUILayout.FloatField("Raw X:", instance.body_raw_X);
    
        EditorGUILayout.FloatField("Raw Y:", instance.body_raw_Y);
    }
}
