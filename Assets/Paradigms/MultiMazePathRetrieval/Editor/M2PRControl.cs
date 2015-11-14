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
        private ObjectPool objectPool;


        private int mazesToUse;
        private int pathsToUsePerMaze; // corresponds with the available objects - one distinct object per path per maze
        [SerializeField]
        private int objectVisitationsInTraining; // how often an object should be visisted while trainings trial
        [SerializeField]
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

            if (lastGeneratedInstanceConfig == null)
                EditorGUILayout.HelpBox("Try \"Find Possible Configuration\" ", MessageType.Info);

            if(GUILayout.Button(new GUIContent("Find Possible Configuration","Search the current Scene for all necessary elements!")))
                EstimateConfigBasedOnAvailableElements();
            
            mazesToUse = EditorGUILayout.IntField("Mazes",mazesToUse);

            pathsToUsePerMaze = EditorGUILayout.IntField("Paths (Objects) per Maze", pathsToUsePerMaze);

            EditorGUILayout.HelpBox("Remember that only one path per maze per object is allowed", MessageType.Info);

            EditorGUILayout.LabelField("Count of object visitations");

            objectVisitationsInTraining =  EditorGUILayout.IntField("Training", objectVisitationsInTraining);
            objectVisitationsInExperiment = EditorGUILayout.IntField("Experiment", objectVisitationsInExperiment);

            if (GUILayout.Button("Generate Instance Config", GUILayout.Height(35))) 
            {
                lastGeneratedInstanceConfig = Generate();
            }

            lastGeneratedInstanceConfig = EditorGUILayout.ObjectField("Last Generated Config", lastGeneratedInstanceConfig, typeof(ParadigmInstanceConfig), false) as ParadigmInstanceConfig;

            if(lastGeneratedInstanceConfig != null && GUILayout.Button("Show Config"))
            {
                throw new NotImplementedException("TODO Implement config viewer");
            }
        }

        private void EstimateConfigBasedOnAvailableElements()
        {
            var mazes = FindObjectsOfType<beMobileMaze>();

            var availableMazes = mazes.Length;
            
            var atLeastAvailblePathsPerMaze = 0;

            foreach (var maze in mazes)
            {
                var pathController = maze.GetComponent<PathController>();

                var availablePathsAtThisMaze = pathController.GetAvailablePathIDs().Length;

                if (atLeastAvailblePathsPerMaze == 0 || atLeastAvailblePathsPerMaze > availablePathsAtThisMaze)
                    atLeastAvailblePathsPerMaze = availablePathsAtThisMaze;
            }

            objectPool = FindObjectOfType<ObjectPool>();

            var availableCategories = objectPool.Categories.Count;

            var atLeastAvailableObjectsPerCategory = 0;

            foreach (var category in objectPool.Categories)
            {
                var availableObjectsFromThisCategory = category.AssociatedObjects.Count;
                
                if (atLeastAvailableObjectsPerCategory == 0 || atLeastAvailableObjectsPerCategory > availableObjectsFromThisCategory)
                    atLeastAvailableObjectsPerCategory = availableObjectsFromThisCategory;
            }

            mazesToUse = availableMazes;
            pathsToUsePerMaze = atLeastAvailblePathsPerMaze;

        }

        private ParadigmInstanceConfig Generate()
        {
            var newConfig = CreateInstance<ParadigmInstanceConfig>();

            newConfig.TrialConfigs = new List<TrialConfig>();

            for (int i = 0; i < objectVisitationsInTraining; i++)
            {
                
                var newTrainingsConfig = new TrialConfig()
                {
                    TrialType = typeof(Training).Name,
                    
                };
                 
            }



            return newConfig;
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