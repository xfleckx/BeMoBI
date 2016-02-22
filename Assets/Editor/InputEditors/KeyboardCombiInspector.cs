using UnityEngine;
using UnityEditor;
using System.Collections;
using Assets.BeMoBI.Scripts.Controls;

[CustomEditor(typeof(KeyboardCombined))]
public class KeyboardCombiInspector : Editor {

    private KeyboardCombined instance;

    private bool showOptions = false;

    public override void OnInspectorGUI()
    {
        instance = target as KeyboardCombined;

        EditorGUILayout.FloatField("Head Raw X:", instance.head_raw_X);

        EditorGUILayout.FloatField("Head Raw Y:", instance.head_raw_Y);
        
        EditorGUILayout.FloatField("Body Raw X:", instance.body_raw_X);

        EditorGUILayout.FloatField("Body Raw Y:", instance.body_raw_Y);

        EditorGUILayout.Vector3Field("Current Forward:", instance.currentForward);

        EditorGUILayout.Vector3Field("Desired Move:", new Vector3(instance.desiredMove.x, instance.desiredMove.y, instance.desiredMove.z));

        EditorGUILayout.Vector4Field("Head rot (Quat.):", new Vector4( instance.headTargetRotation.x, instance.headTargetRotation.y, instance.headTargetRotation.z, instance.headTargetRotation.w));

        EditorGUILayout.Vector4Field("Head rot:", new Vector4(instance.sourceRotation.x, instance.sourceRotation.y, instance.sourceRotation.z, instance.sourceRotation.w));

        EditorGUILayout.Vector4Field("Head local rot:", new Vector4(instance.sourceLocalRotation.x, instance.sourceLocalRotation.y, instance.sourceLocalRotation.z, instance.sourceLocalRotation.w));

        EditorGUILayout.Vector4Field("Body rot (Quat.):", new Vector4(instance.targetRotation.x, instance.targetRotation.y, instance.targetRotation.z, instance.targetRotation.w));

        EditorGUILayout.Space();

        showOptions = EditorGUILayout.Foldout(showOptions, "Show Options");

        if (showOptions)
            base.OnInspectorGUI();

    }
}
