using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.IO;

public class InstructionEditorWindow : EditorWindow {

    public HUD_Instruction HudController;
    //private string fileName = string.Empty;
    //private int setSelectionIndex = 0;

    //void OnGUI()
    //{
    //    GUILayout.BeginHorizontal();

    //        GUILayout.BeginVertical(GUILayout.Width(150)); 
                 
    //            if (GUILayout.Button("Load Instruction set"))
    //            {
    //                fileName = EditorUtility.OpenFilePanel("Open a instruction file", Application.dataPath, "txt");

    //                if (!fileName.Equals(string.Empty)) { 

    //                    var loadedSet = InstructionFactory.ReadInstructionSetFrom(fileName);

    //                    if (loadedSet.instructions.Count == 0)
    //                    {
    //                        EditorGUILayout.HelpBox("The file was found, but did not contain any instructions!", MessageType.Error);
    //                    }

    //                    HudController.KnownSets.Add(loadedSet.name, loadedSet);
    //                    EditorUtility.SetDirty(HudController);
    //                }
    //            }
         
    //            if (GUILayout.Button("Save Instruction set"))
    //            {
    //                EditorUtility.SaveFilePanelInProject(fileName, HudController.currentInstructionSet.name, "txt", "Save the current Instruction");
    //            }
        
    //            if (HudController.KnownSets.Any())
    //            {
    //                RenderInstructionComboBox();
    //            }    

    //        GUILayout.EndVertical();

    //        if (!HudController.KnownSets.Any())
    //            EditorGUILayout.HelpBox("No instructions available! Start with loading a set!", MessageType.Info);

    //    GUILayout.EndHorizontal();

    //    if (HudController.currentInstructionSet == null) {
    //        HudController.currentInstructionSet = CreateExampleInstructionSet();
    //    }

    //    EditorGUILayout.Space();

    //    Render(HudController.currentInstructionSet);
    //}

    //private void RenderInstructionComboBox()
    //{
    //    //var optionCount = HudController.KnownSets.Keys.Count;
    //    //var options = new string[optionCount];
    //    //HudController.KnownSets.Keys.CopyTo(options, 0);
    //    //setSelectionIndex = EditorGUILayout.Popup(setSelectionIndex, options);

    //    //var sets = new InstructionSet[HudController.KnownSets.Keys.Count];
    //    //HudController.KnownSets.Values.CopyTo(sets, 0);

    //    setSelectionIndex = HudController.KnownSets.Values.RenderAsSelectionBox(setSelectionIndex);

    //    HudController.currentInstructionSet = HudController.KnownSets.ElementAt(setSelectionIndex).Value;
    //} 

    //private Vector2 scrollPosition;
    //private void Render(InstructionSet set)
    //{
    //    if (set.instructions == null) { 
    //        Debug.Log("Empty instructions");
    //        return;
    //    }
    //    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
    //    foreach (var item in set.instructions)
    //    {
    //        Render(item);
    //    } 

    //    EditorGUILayout.EndScrollView();
    //}

    //private void Render(Instruction instruction)
    //{
    //    GUILayout.Space(instructionBoxMargin.y);

    //    GUILayout.BeginVertical();

    //    GUILayout.BeginHorizontal();

    //    if (instruction.ImageTexture == null)
    //    {

    //        var texture = Resources.Load<Texture2D>(instruction.ImageName) as Texture2D;
    //        instruction.ImageTexture = texture;
    //        GUILayout.Box(instruction.ImageTexture, GUILayout.MaxHeight(100), GUILayout.MaxWidth(100));
    //    }
    //    else
    //    {
    //        GUILayout.Box(instruction.ImageName, GUILayout.MaxHeight(100), GUILayout.MaxWidth(100));
    //    }


    //    instruction.Text = GUILayout.TextArea(instruction.Text, GUILayout.MinHeight(50f)) as string;
    //    instruction.DisplayTime = EditorGUILayout.FloatField("Display Time", instruction.DisplayTime);

    //    GUILayout.EndHorizontal();

    //    GUILayout.EndVertical();
    //}

    void OnDestroy()
    {
        Debug.Log("Instruction Editor Window closed");

    }
    
    // Top, Bottom, Left, Right
    // private readonly Vector4 instructionBoxMargin = new Vector4(0f, 5f, 2f, 0f);
}
