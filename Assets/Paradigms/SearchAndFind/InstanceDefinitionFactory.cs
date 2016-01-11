using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Paradigms.SearchAndFind
{
    public class InstanceDefinitionFactory
    {
        [SerializeField]
        public int categoriesPerMaze = 1;
        [SerializeField]
        public int mazesToUse;
        [SerializeField]
        public int pathsToUsePerMaze; // corresponds with the available objects - one distinct object per path per maze
        [SerializeField]
        public int objectVisitationsInTraining = 1; // how often an object should be visisted while trainings trial
        [SerializeField]
        public int objectVisitationsInExperiment = 1; // " while Experiment
        [SerializeField]
        public bool useExactOnCategoryPerMaze = true;
        [SerializeField]
        public bool groupByMazes = true;
        
        [SerializeField]
        public string subject_ID = "TestSubject";

        public InstanceDefinitionFactory()
        {
        }

        public int atLeastAvailblePathsPerMaze = 0;

        private bool enoughPathsAreAvailable = false;
        private bool enoughObjectsAreAvailable = false;
        
        public void EstimateConfigBasedOnAvailableElements()
        {
            objectPool = UnityEngine.Object.FindObjectOfType<ObjectPool>();

            var vrManager = UnityEngine.Object.FindObjectOfType<VirtualRealityManager>();

            mazeInstances = vrManager.transform.AllChildren().Where(c => c.GetComponents<beMobileMaze>() != null).Select(c => c.GetComponent<beMobileMaze>()).ToList();

            var availableCategories = objectPool.Categories.Count;
            
            var availableMazes = mazeInstances.Count;

            if (availableMazes > availableCategories)
                mazesToUse = availableCategories;
            else
                mazesToUse = availableMazes;

            CheckIfEnoughPathsAreAvailable();
        }

        #region Generator logic - bad code here... needs to be encapsulated

        private ObjectPool objectPool;
        private List<beMobileMaze> mazeInstances;

        private Dictionary<beMobileMaze, Category> mazeCategoryMap;
        // use stack for asserting that every category will be used once
        private Stack<Category> availableCategories;

        public object IsAbleToGenerate {
            get
            {
                return CheckGenerationConstraints();
            }
        }

        public object CheckGenerationConstraints()
        {
            bool result = false;

            result = objectPool != null && result;
            result = mazeInstances.Count >= mazesToUse && result;
            result = enoughPathsAreAvailable && result;
            result = enoughObjectsAreAvailable && result;
            
            return false;
        }

        public void CheckIfEnoughPathsAreAvailable()
        {
            atLeastAvailblePathsPerMaze = 0;

            foreach (var maze in mazeInstances)
            {
                var pathController = maze.GetComponent<PathController>();

                var availablePathsAtThisMaze = pathController.GetAvailablePathIDs().Length;

                if (atLeastAvailblePathsPerMaze == 0 || atLeastAvailblePathsPerMaze > availablePathsAtThisMaze)
                {
                    enoughPathsAreAvailable = false;
                    atLeastAvailblePathsPerMaze = availablePathsAtThisMaze;
                }
                else
                {
                    enoughPathsAreAvailable = true;
                    pathsToUsePerMaze = atLeastAvailblePathsPerMaze;
                }
            }
        }

        public bool CheckIfEnoughObjectsAreAvailable()
        {
            var atLeastAvailableObjectsPerCategory = 0;

            foreach (var category in objectPool.Categories)
            {
                var availableObjectsFromThisCategory = category.AssociatedObjects.Count;

                if (atLeastAvailableObjectsPerCategory == 0 || atLeastAvailableObjectsPerCategory > availableObjectsFromThisCategory) { 
                    atLeastAvailableObjectsPerCategory = availableObjectsFromThisCategory;
                }

            }

            // TODO here

            return true;
        }

        public ParadigmInstanceDefinition Generate()
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

            var newConfig = UnityEngine.ScriptableObject.CreateInstance<ParadigmInstanceDefinition>();
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

            if (groupByMazes)
            {

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
