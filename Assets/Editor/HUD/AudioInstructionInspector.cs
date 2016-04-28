using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(AudioInstructions))]
public class AudioInstructionInspector : Editor
{
    private Action callAtTheEndOfOnGUI;

    public override void OnInspectorGUI()
    {
        var instance = target as AudioInstructions;
        
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("Available Clips:");

        for (int i = 0; i < instance.clipMap.Count; i++)
        {
            var item = instance.clipMap[i];

            EditorGUILayout.BeginHorizontal();

            item.Name = EditorGUILayout.TextField(item.Name);
            item.clip = EditorGUILayout.ObjectField(item.clip, typeof(AudioClip), true) as AudioClip;

            if (GUILayout.Button("x"))
            {
                callAtTheEndOfOnGUI += () =>
                {
                    instance.clipMap.RemoveAt(i);
                    callAtTheEndOfOnGUI = null;
                }; 
            }

            EditorGUILayout.EndVertical();

            if(callAtTheEndOfOnGUI != null)
                callAtTheEndOfOnGUI();
        }

        EditorGUILayout.EndVertical();

        if(GUILayout.Button("Add new empty Clip"))
        {
            instance.clipMap.Add(new NameToClipMapping());
        }
    }
}