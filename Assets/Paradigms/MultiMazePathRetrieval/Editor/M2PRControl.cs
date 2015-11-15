using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Diagnostics;

namespace Assets.Paradigms.MultiMazePathRetrieval
{
    public class M2PRControl : EditorWindow
    {
        private ParadigmController instance;

        [SerializeField]
        private int subject_ID = 0;
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

        private ParadigmInstanceDefinition lastGeneratedInstanceConfig;

        internal void Initialize(ParadigmController target)
        {
            instance = target;

            titleContent = new GUIContent("Paradigm Control");

            this.minSize = new Vector2(500, 600);
        }
         
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            if (instance == null) { 
                EditorGUILayout.HelpBox("No Paradigm Controller available! \n Open another scene or create a paradigm controller instance!", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal(GUILayout.Width(400));

            EditorGUILayout.BeginVertical(GUILayout.Width(150));

            RenderControlGUI();

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(250));

            RenderConfigurationGUI();

            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();

            RenderPreviewGUI();

            EditorGUILayout.EndVertical();
        }

        private void RenderControlGUI()
        {
            GUILayout.Label("Control", EditorStyles.largeLabel);

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("To use the controls, please start the playmode!", MessageType.Info);
                return;
            }

            //EditorGUILayout.BeginHorizontal();

            //if (GUILayout.Button("Start Instruction Trial"))
            //{
            //    instance.Begin(instance.instruction);
            //}

            //if (GUILayout.Button("Start Pause Trial"))
            //{
            //    instance.Begin(instance.pause);
            //}

            //EditorGUILayout.EndHorizontal();

            //if (GUILayout.Button("Start Training Trial"))
            //{
            //    instance.Begin(instance.training);
            //}

            //if (GUILayout.Button("Start Experiment Trial"))
            //{
            //    instance.Begin(instance.experiment);
            //}
        }

        private void RenderConfigurationGUI()
        {
            GUILayout.Label("Configuration", EditorStyles.largeLabel);

            if (lastGeneratedInstanceConfig == null)
                EditorGUILayout.HelpBox("Try \"Find Possible Configuration\" ", MessageType.Info);

            if (GUILayout.Button(new GUIContent("Find Possible Configuration", "Search the current Scene for all necessary elements!")))
                EstimateConfigBasedOnAvailableElements();


            subject_ID = EditorGUILayout.IntField("Subject", subject_ID);


            mazesToUse = EditorGUILayout.IntField("Mazes", mazesToUse);

            pathsToUsePerMaze = EditorGUILayout.IntField("Paths (Objects) per Maze", pathsToUsePerMaze);

            useExactOnCategoryPerMaze = EditorGUILayout.Toggle(
                new GUIContent("Use category exclusive", "A category will never be shared within multiple mazes"),
                useExactOnCategoryPerMaze);

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
                var fileName = string.Format("Assets/Paradigms/MultiMazePathRetrieval/Resources/VP_{0}_InstanceDefinition.asset", subject_ID);

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

                foreach (var tdef in lastGeneratedInstanceConfig.Trials)
                {
                    EditorGUILayout.LabelField(
                        string.Format(DEFINITION_PREVIEW_PATTERN,
                        tdef.TrialType,
                        tdef.MazeName,
                        tdef.Path,
                        tdef.ObjectName,
                        tdef.Category));
                }

                EditorGUILayout.EndScrollView();
            }
        }

        const string DEFINITION_PREVIEW_PATTERN = "{0}: {1} -> {2} = {3} from {4}";
        private Vector2 configPreviewScrollState;

        private void EstimateConfigBasedOnAvailableElements()
        {
            mazeInstances = FindObjectsOfType<beMobileMaze>().ToList();

            objectPool = FindObjectOfType<ObjectPool>();

            var availableMazes = mazeInstances.Count;

            var atLeastAvailblePathsPerMaze = 0;

            foreach (var maze in mazeInstances)
            {
                var pathController = maze.GetComponent<PathController>();

                var availablePathsAtThisMaze = pathController.GetAvailablePathIDs().Length;

                if (atLeastAvailblePathsPerMaze == 0 || atLeastAvailblePathsPerMaze > availablePathsAtThisMaze)
                    atLeastAvailblePathsPerMaze = availablePathsAtThisMaze;
            }
            
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

            availableCategories = new Stack<Category>(objectPool.Categories);

            mazeInstances.ForEach((m) => {
                ChooseCategoryFor(m);
            });

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
            newConfig.name = string.Format("VP_{0}_InstanceDef", subject_ID);

            newConfig.Trials = new List<TrialDefinition>();

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

                    newConfig.Trials.Add(newTrainingsTrialDefinition);
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

                    newConfig.Trials.Add(newExperimentTrialDefinition);
                }
            }

            #endregion

            newConfig.Trials = newConfig.Trials.OrderByDescending(t => t.TrialType).ToList();

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