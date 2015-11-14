using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Assets.Paradigms.MultiMazePathRetrieval
{
    public class M2PRControl : EditorWindow
    {
        private ParadigmController instance;

        private int mazesToUse;
        private int pathsToUsePerMaze; // corresponds with the available objects - one distinct object per path per maze
        private int objectVisitationsInTraining; // how often an object should be visisted while trainings trials
        private int objectVisitationsInExperiment; // " while Experiment

        private ParadigmInstanceConfig lastGeneratedInstanceConfig;
        
        internal void Initialize(ParadigmController target)
        {
            instance = target;
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(150));

            RenderControlGUI();

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(200));

            RenderConfigurationGUI();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

        }

        private void RenderControlGUI()
        {
            GUILayout.Label("Control", EditorStyles.largeLabel);

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("To use the controls, please start the playmode!", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Start Instruction Trial"))
            {
                instance.Begin(instance.instruction);
            }

            if (GUILayout.Button("Start Pause Trial"))
            {
                instance.Begin(instance.pause);
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Start Training Trial"))
            {
                instance.Begin(instance.training);
            }

            if (GUILayout.Button("Start Experiment Trial"))
            {
                instance.Begin(instance.experiment);
            }
        }

        private void RenderConfigurationGUI()
        {
            GUILayout.Label("Configuration", EditorStyles.largeLabel);



            if (GUILayout.Button("Generate Instance Config", GUILayout.Height(35)))
            {
                lastGeneratedInstanceConfig = Generate();
            }

        }

        private ParadigmInstanceConfig Generate()
        {
            throw new NotImplementedException("TODO");
        }
    }


    public class ParadigmInstanceConfig : ScriptableObject
    {
        public string BodyController;
        public string HeadController;

        public List<TrialConfig> TrialConfigs;

    }

    public class TrialConfig
    {
        public string TrialType;
        public string MazeName;
        public int Path;
        public string ObjectName;
    }

}