﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Diagnostics;
using NLog;
using NLogger = NLog.Logger;

namespace Assets.Paradigms.SearchAndFind
{
    public class ParadigmControlWindow : EditorWindow
    {
        NLogger log = LogManager.GetCurrentClassLogger();

        private ParadigmController instance;

        [SerializeField]
        private string subject_ID = "TestSubject";


        private ParadigmInstanceDefinition lastGeneratedInstanceConfig;

        internal void Initialize(ParadigmController target)
        {
            instance = target;

            titleContent = new GUIContent("Paradigm Control");

            this.minSize = new Vector2(500, 600);

            log.Info("Initialize Paradigma Control Window");
        }
         
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            if (instance == null && (instance = TryGetInstance()) == null) { 
                EditorGUILayout.HelpBox("No Paradigm Controller available! \n Open another scene or create a paradigm controller instance!", MessageType.Info);
                return;
            }
            
            EditorGUILayout.BeginVertical();

            RenderControlGUI(); 

            EditorGUILayout.EndVertical();
        }

        private ParadigmController TryGetInstance()
        {
            return FindObjectOfType<ParadigmController>();
        }

        private void RenderControlGUI()
        {
            GUILayout.Label("Control", EditorStyles.largeLabel);

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Please start the playmode through this start button!", MessageType.Info);

                RenderRunVariables();

                if (GUILayout.Button("Start playmode", GUILayout.Height(30)))
                { 
                    InjectCmdArgs();

                    EditorApplication.ExecuteMenuItem("Edit/Play");
                }


                if(GUILayout.Button("Open Survey"))
                {
                    var httpRequest = FormatSurveyRequest();

                    Process.Start("explorer", httpRequest);
                }


                if (GUILayout.Button("Open logs"))
                {
                    Process.Start("explorer", ".");
                }

                return;
            }

            if (!instance.IsRunning) {
                
                if (GUILayout.Button("Start First Trial", GUILayout.Height(25)))
                {
                    instance.StartTheExperimentFromBeginning();
                }

                if(GUILayout.Button("Run Training"))
                {
                    instance.StartASubsetOfTrials<Training>();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Paradigma is already running", MessageType.Info);


                if(instance.currentTrial != instance.pause && GUILayout.Button("Inject Pause Trial"))
                { 
                    instance.InjectPauseTrial();
                }

                if(instance.currentTrial == instance.pause && GUILayout.Button("End Pause"))
                {
                    instance.currentTrial.ForceTrialEnd();
                }
                
                EditorGUILayout.Space();

                if (GUILayout.Button("End current run", GUILayout.Height(25)))
                {
                    instance.ForceSaveEnd();
                }

                EditorGUILayout.Space();

                if(GUILayout.Button("Save Paradigma State \n and Quit"))
                {
                    instance.PerformSaveInterupt();
                }


            }

        }

        private string FormatSurveyRequest()
        {
            return @"http:\\localhost\limesurvey\index.php\197498?lang=en" + "?subject=test?pose=bla";
        }

        private void RenderRunVariables()
        {
            EditorGUILayout.BeginVertical();

            GUILayout.Label("Subject ID:");
            subject_ID = EditorGUILayout.TextField(subject_ID);


            GUILayout.Label("Run definition:");
            instance.InstanceDefinition = EditorGUILayout.ObjectField(instance.InstanceDefinition, typeof(ParadigmInstanceDefinition), false) as ParadigmInstanceDefinition;


            EditorGUILayout.EndVertical();
        }

        private void InjectCmdArgs()
        {
            instance.SubjectID = subject_ID;
        }
        
        const string DEFINITION_PREVIEW_PATTERN = "{0}: {1} -> {2} = {3} from {4}";
        private Vector2 configPreviewScrollState;
        
    }
}