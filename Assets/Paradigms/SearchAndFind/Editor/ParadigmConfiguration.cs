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
    public class ConfigurationControl : EditorWindow
    {
        NLogger log = LogManager.GetCurrentClassLogger();

        private ParadigmController instance;

        [SerializeField]
        private string subject_ID = "TestSubject";
        [SerializeField]
        private int categoriesPerMaze = 1;
        [SerializeField]
        private int mazesToUse;
        [SerializeField]
        private int pathsToUsePerMaze; // corresponds with the available objects - one distinct object per path per maze
        [SerializeField]
        private int objectVisitationsInTraining = 1; // how often an object should be visisted while trainings trial
        [SerializeField]
        private int objectVisitationsInExperiment = 1; // " while Experiment
        [SerializeField]
        private bool useExactOnCategoryPerMaze = true;
        [SerializeField]
        private bool groupByMazes = true;


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

            EditorGUILayout.BeginHorizontal(GUILayout.Width(400)); 

            EditorGUILayout.BeginVertical();

            RenderRunVariables();

            RenderConfigurationGUI();

            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();

            RenderPreviewGUI();

            EditorGUILayout.EndVertical();
        }

        private ParadigmController TryGetInstance()
        {
            return FindObjectOfType<ParadigmController>();
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

            EditorGUILayout.EndVertical();
        }

        private void InjectCmdArgs()
        {
            instance.SubjectID = subject_ID;
        }

        private void RenderConfigurationGUI()
        {
            GUILayout.Label("Configuration", EditorStyles.largeLabel);

            useExactOnCategoryPerMaze = EditorGUILayout.Toggle(
                new GUIContent("Use category exclusive", "A category will never be shared within multiple mazes"),
                useExactOnCategoryPerMaze);

            groupByMazes = EditorGUILayout.Toggle(
                new GUIContent("Group by Mazes and Paths", "Trials are set as tuples of training and experiment trials per Maze and Path"), 
                groupByMazes);

            if (lastGeneratedInstanceConfig == null)
                EditorGUILayout.HelpBox("Try \"Find Possible Configuration\" ", MessageType.Info);

            if (GUILayout.Button(new GUIContent("Find Possible Configuration", "Search the current Scene for all necessary elements!")))
                EstimateConfigBasedOnAvailableElements();
            
            mazesToUse = EditorGUILayout.IntField("Mazes", mazesToUse);

            pathsToUsePerMaze = EditorGUILayout.IntField("Paths (Objects) per Maze", pathsToUsePerMaze);

            if (!useExactOnCategoryPerMaze)
            {
                categoriesPerMaze = EditorGUILayout.IntField(
                    new GUIContent("Categories per Maze", "Declares the amount of categories \n from which objects are choosen."),
                    categoriesPerMaze);
            }

            EditorGUILayout.HelpBox("Remember that only one path per maze per object is allowed", MessageType.Info);

            EditorGUILayout.LabelField("Count of object visitations");

            objectVisitationsInTraining = EditorGUILayout.IntField("Training", objectVisitationsInTraining);
            objectVisitationsInExperiment = EditorGUILayout.IntField("Experiment", objectVisitationsInExperiment);

            if (objectPool != null && GUILayout.Button("Generate Instance Config", GUILayout.Height(35)))
            {
                lastGeneratedInstanceConfig = Generate();
            }

            lastGeneratedInstanceConfig = EditorGUILayout.ObjectField("Last Generated Config", lastGeneratedInstanceConfig, typeof(ParadigmInstanceDefinition), false) as ParadigmInstanceDefinition;

            if (lastGeneratedInstanceConfig == null)
                return;

            previewConfig = EditorGUILayout.Toggle("Show definition", previewConfig);

            if (GUILayout.Button("Save Instance Config"))
            {
                var fileName = string.Format("Assets/Paradigms/SearchAndFind/Resources/VP_{0}_InstanceDefinition.asset", subject_ID);

                AssetDatabase.CreateAsset(lastGeneratedInstanceConfig, fileName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void RenderPreviewGUI()
        {
            if (lastGeneratedInstanceConfig == null)
                return;

            EditorGUILayout.LabelField("Preview;");

            if (lastGeneratedInstanceConfig.Trials != null && previewConfig)
            {
                configPreviewScrollState = EditorGUILayout.BeginScrollView(configPreviewScrollState);
                string lastMazeName = string.Empty;
                foreach (var tdef in lastGeneratedInstanceConfig.Trials)
                {
                    if (!lastMazeName.Equals(tdef.MazeName))
                        EditorGUILayout.Space();

                    EditorGUILayout.LabelField(
                        string.Format(DEFINITION_PREVIEW_PATTERN,
                        tdef.TrialType,
                        tdef.MazeName,
                        tdef.Path,
                        tdef.ObjectName,
                        tdef.Category));
                    lastMazeName = tdef.MazeName;
                }

                EditorGUILayout.EndScrollView();
            }
        }

        const string DEFINITION_PREVIEW_PATTERN = "{0}: {1} -> {2} = {3} from {4}";
        private Vector2 configPreviewScrollState;

        private void EstimateConfigBasedOnAvailableElements()
        {
            var vrManager = FindObjectOfType<VirtualRealityManager>();
            
            mazeInstances = vrManager.transform.AllChildren().Where(c => c.GetComponents<beMobileMaze>() != null ).Select(c => c.GetComponent<beMobileMaze>()).ToList();

            objectPool = FindObjectOfType<ObjectPool>();

            var availableCategories = objectPool.Categories.Count;

            var atLeastAvailableObjectsPerCategory = 0;

            foreach (var category in objectPool.Categories)
            {
                var availableObjectsFromThisCategory = category.AssociatedObjects.Count;

                if (atLeastAvailableObjectsPerCategory == 0 || atLeastAvailableObjectsPerCategory > availableObjectsFromThisCategory)
                    atLeastAvailableObjectsPerCategory = availableObjectsFromThisCategory;
            }
            
            var availableMazes = mazeInstances.Count;

            if (availableMazes > availableCategories)
                mazesToUse = availableCategories;
            else
                mazesToUse = availableMazes;

            var atLeastAvailblePathsPerMaze = 0;

            foreach (var maze in mazeInstances)
            {
                var pathController = maze.GetComponent<PathController>();

                var availablePathsAtThisMaze = pathController.GetAvailablePathIDs().Length;

                if (atLeastAvailblePathsPerMaze == 0 || atLeastAvailblePathsPerMaze > availablePathsAtThisMaze)
                    atLeastAvailblePathsPerMaze = availablePathsAtThisMaze;
            }

            pathsToUsePerMaze = atLeastAvailblePathsPerMaze;

        }

        #region Generator logic - bad code here... needs to be encapsulated

        private ObjectPool objectPool;
        private List<beMobileMaze> mazeInstances;

        private Dictionary<beMobileMaze, Category> mazeCategoryMap;
        // use stack for asserting that every category will be used once
        private Stack<Category> availableCategories;
        private bool previewConfig;

        private ParadigmInstanceDefinition Generate()
        { 
            #region assert some preconditions for the algorithm
            
            mazeCategoryMap = new Dictionary<beMobileMaze, Category>();

            var shuffledCategories = objectPool.Categories.OrderBy((i) => Guid.NewGuid()).ToList();

            availableCategories = new Stack<Category>(shuffledCategories);

            for (int i = 0; i < mazesToUse; i++)
            {
                var maze = mazeInstances[i];
                ChooseCategoryFor(maze);
            }
            
            #endregion

            #region create all possible trial configurations

            var possibleTrials = new List<TrialConfig>();

            foreach (var association in mazeCategoryMap)
            {
                var maze = association.Key;
                var category = association.Value;

                var configs = MapPathsToObjects(maze, category);
                possibleTrials.AddRange(configs);
            }

            #endregion

            #region now create the actual Paradigma instance defintion by duplicating the possible configurations for trianing and experiment

            var newConfig = CreateInstance<ParadigmInstanceDefinition>();
            newConfig.Subject = subject_ID;
            newConfig.name = string.Format("VP_Def_{0}", subject_ID);

            newConfig.Trials = new List<TrialDefinition>();

            var trainingTrials = new List<TrialDefinition>();
            var experimentalTrials = new List<TrialDefinition>();

            foreach (var trialDefinition in possibleTrials)
            {
                for (int i = 0; i < objectVisitationsInTraining; i++)
                {
                    var newTrainingsTrialDefinition = new TrialDefinition()
                    {
                        TrialType = typeof(Training).Name,
                        Category = trialDefinition.Category,
                        MazeName = trialDefinition.MazeName,
                        Path = trialDefinition.Path,
                        ObjectName = trialDefinition.ObjectName
                    };

                    trainingTrials.Add(newTrainingsTrialDefinition);
                }

                for (int i = 0; i < objectVisitationsInExperiment; i++)
                {
                    var newExperimentTrialDefinition = new TrialDefinition()
                    {
                        TrialType = typeof(Experiment).Name,
                        Category = trialDefinition.Category,
                        MazeName = trialDefinition.MazeName,
                        Path = trialDefinition.Path,
                        ObjectName = trialDefinition.ObjectName
                    };

                    experimentalTrials.Add(newExperimentTrialDefinition);

                }
            }

            #endregion

            if (groupByMazes) {

                var tempAllTrials = new List<TrialDefinition>();
                tempAllTrials.AddRange(trainingTrials);
                tempAllTrials.AddRange(experimentalTrials);

                var groupedByMaze = tempAllTrials.GroupBy((td) => td.MazeName);

                foreach (var group in groupedByMaze)
                {
                    var groupedByPath = group.GroupBy(td => td.Path).OrderBy(g => Guid.NewGuid());

                    List<TrialDefinition> trainingPerMaze = new List<TrialDefinition>();
                    List<TrialDefinition> experimentPerMaze = new List<TrialDefinition>();

                    foreach (var pathGroup in groupedByPath)
                    {
                        var pathGroupTraining = pathGroup.Where(td => td.TrialType.Equals(typeof(Training).Name));
                        trainingPerMaze.AddRange(pathGroupTraining);

                        var pathGroupExperiment = pathGroup.Where(td => td.TrialType.Equals(typeof(Experiment).Name));
                        experimentPerMaze.AddRange(pathGroupExperiment);
                    }

                    // using Guid is a trick to randomly sort a set
                    var shuffledTrainingPerMaze = trainingPerMaze.OrderBy(td => Guid.NewGuid());
                    var shuffledExperimentPerMaze = experimentPerMaze.OrderBy(td => Guid.NewGuid());

                    newConfig.Trials.AddRange(shuffledTrainingPerMaze);
                    newConfig.Trials.AddRange(shuffledExperimentPerMaze);
                } 

            }
            else
            {
                newConfig.Trials.AddRange(trainingTrials);

                var shuffledExperimentalTrials = experimentalTrials.OrderBy(trial => Guid.NewGuid());
                newConfig.Trials.AddRange(shuffledExperimentalTrials);
            }

            return newConfig;
        }

        private IEnumerable<TrialConfig> MapPathsToObjects(beMobileMaze maze, Category category)
        {
            var paths = maze.GetComponent<PathController>().Paths.Where(p => p.Available).ToArray();
            var resultConfigs = new List<TrialConfig>();

            // be aware that pathsToUsePerMaze must be up-to-date
            for (int i = 0; i < pathsToUsePerMaze; i++)
            {
                var objectFromCategory = category.SampleWithoutReplacement();
                var path = paths[i];

                var trialConfig = new TrialConfig()
                {
                    Category = category.name,
                    MazeName = maze.name,
                    Path = path.ID,
                    ObjectName = objectFromCategory.name
                };

                resultConfigs.Add(trialConfig);
            }

            category.ResetSamplingSequence();

            return resultConfigs;
        }

        private void ChooseCategoryFor(beMobileMaze m)
        {
            if (!mazeCategoryMap.ContainsKey(m))
            {
                //TODO first.. apply sample extension to categories
                mazeCategoryMap.Add(m, availableCategories.Pop());
            }
        } 

        #endregion
    }
}