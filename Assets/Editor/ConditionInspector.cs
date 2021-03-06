﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using Assets.BeMoBI.Paradigms.SearchAndFind;
using UnityEngine.Events;

[CustomEditor(typeof(ConditionController))]
public class ConditionInspector : Editor {

    public event Action OnPostGUICommands;
    
    private ConditionController instance;
    
    public override void OnInspectorGUI()
    {
        instance = target as ConditionController;

        //base.OnInspectorGUI();

        var currentCondition = instance.currentCondition == null ? "no condition" : instance.currentCondition.Identifier;

        EditorGUILayout.LabelField("Current Condition", currentCondition);
        
        var currentTrial = instance.currentTrial == null ? "no trial" : instance.currentTrial.name;

        EditorGUILayout.LabelField("Current Trial", currentTrial);
        
        if (instance.paradigm == null)
        {
            EditorGUILayout.HelpBox("Missing reference to paradigm controller instance", MessageType.Error);
            return;
        }

        if (instance.paradigm.Config == null)
        {
            EditorGUILayout.HelpBox("Load a Paradigm Config in the ParadigmController", MessageType.Error);
            return;
        }
         
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Available Condition configs");

        foreach (var config in instance.paradigm.Config.conditionConfigurations)
        {
            if (config == null)
                continue;

            EditorGUILayout.BeginHorizontal();

            config.ConditionID = EditorGUILayout.TextField(config.ConditionID);
            
            if (GUILayout.Button("Load")){
                instance.conditionConfig = config;
            }

            if (GUILayout.Button("Copy"))
            {
                var newCopyCommand = new Action(() =>
                {
                    //var copy = ScriptableObject.Instantiate<ConditionConfiguration>(config);
                    //copy.hideFlags = HideFlags.HideAndDontSave;
                    var copy = config.Clone() as ConditionConfiguration;
                    //copy.name = copy.name + "(Clone)";
                    instance.paradigm.Config.conditionConfigurations.Add(copy);
                });

                OnPostGUICommands += newCopyCommand;
            }

            if (GUILayout.Button("Delete"))
            {
                var newCopyCommand = new Action(() =>
                { 
                    instance.paradigm.Config.conditionConfigurations.Remove(config);
                    //DestroyImmediate(config);
                    GC.Collect();
                });

                OnPostGUICommands += newCopyCommand;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        if(GUILayout.Button("Create Default Condition"))
        {
            instance.paradigm.Config.conditionConfigurations.Add(ConditionConfiguration.GetDefault());
        }

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPauseBegin"));
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPauseEnd"));

        serializedObject.ApplyModifiedProperties();

        if (OnPostGUICommands != null) { 
            OnPostGUICommands();
            OnPostGUICommands = null;
        }
    }
}

