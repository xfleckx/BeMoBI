using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;
using System;

[CustomEditor(typeof(HUD_Instruction))]
public class InstructionEditor : Editor {

    private string headerPreview = string.Empty;
    private string wideTextPreview = string.Empty;
    private string smallTextPreview = string.Empty;
    private Texture leftImagePreview = null;
    private Texture centerImagePreview = null;

    private HUD_Instruction instance;

    public override void OnInspectorGUI()
    {
        instance = target as HUD_Instruction;

        base.OnInspectorGUI();

        headerPreview = EditorGUILayout.TextField("Header", headerPreview);
        wideTextPreview = EditorGUILayout.TextField("Paragraph", wideTextPreview);
        smallTextPreview = EditorGUILayout.TextField("Text w. Image", smallTextPreview);
        leftImagePreview = EditorGUILayout.ObjectField("Image w. Text", leftImagePreview, typeof(Texture2D), false) as Texture;
        centerImagePreview = EditorGUILayout.ObjectField("Center Image", centerImagePreview, typeof(Texture2D), false) as Texture;

        if (GUILayout.Button("Render Preview"))
        {
            ShowCurrentPreviewInstructions();
        }

        if (GUILayout.Button("Clear"))
        {
            instance.Clear();
        }
     }

    private void ShowCurrentPreviewInstructions()
    {
        if (centerImagePreview != null) { 
            instance.ShowInstruction(centerImagePreview);
            return;
        }

        if (headerPreview != string.Empty && wideTextPreview != string.Empty) { 
            instance.ShowInstruction(wideTextPreview, headerPreview);
            return;
        }

        if(headerPreview != string.Empty && smallTextPreview != string.Empty && leftImagePreview != null)
        {
            instance.ShowInstruction(smallTextPreview, headerPreview, leftImagePreview);
            return;
        }
    }
}
