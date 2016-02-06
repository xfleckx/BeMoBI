using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.BeMoBI.Paradigms.SearchAndFind
{
    public class InstanceDefinitionFactory
    {
        public ParadigmConfiguration config;
        
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


            if(config.mazesToUse > availableMazes) {

                config.mazesToUse = availableMazes;

                var warningMessage = string.Format("Warning! You want to use more mazes {0} than available {1}", config.mazesToUse, availableMazes);

                Debug.Log(warningMessage);
            }

            if (config.useExactOnCategoryPerMaze)
            {
                if (config.mazesToUse > availableCategories)
                    config.mazesToUse = availableCategories;
            } // warning no else condition defined! TODO... what should happen when multiple categories per maze are available?
            
            CheckIfEnoughPathsAreAvailable();

            CheckIfEnoughObjectsAreAvailable();
        }

        #region Generator logic - bad code here... needs to be encapsulated

        private ObjectPool objectPool;
        private List<beMobileMaze> mazeInstances;

        private Dictionary<beMobileMaze, Category> mazeCategoryMap;
        // use stack for asserting that every category will be used once
        private Stack<Category> availableCategories;

        public bool IsAbleToGenerate {
            get
            {
                return CheckGenerationConstraints();
            }
        }

        public bool CheckGenerationConstraints()
        {
            var result = objectPool != null;
            result = mazeInstances.Count >= config.mazesToUse && (config.mazesToUse > 0) && result;
            result = enoughPathsAreAvailable && result;
            result = enoughObjectsAreAvailable && result;
            
            return result;
        }

        public void CheckIfEnoughPathsAreAvailable()
        {
            var atLeastAvailablePaths = 0;

            foreach (var maze in mazeInstances)
            {
                var pathController = maze.GetComponent<PathController>();

                var availablePathsAtThisMaze = pathController.GetAvailablePathIDs().Length;

                if (atLeastAvailablePaths == 0)
                {
                    atLeastAvailablePaths = availablePathsAtThisMaze;
                }

                if (availablePathsAtThisMaze < atLeastAvailablePaths)
                {
                    atLeastAvailablePaths = availablePathsAtThisMaze;
                }
            }

            atLeastAvailblePathsPerMaze = atLeastAvailablePaths;

            if (config.pathsToUsePerMaze > atLeastAvailblePathsPerMaze)
                enoughPathsAreAvailable = false;
            else
            {
                enoughPathsAreAvailable = true;
            }
        }

        public void CheckIfEnoughObjectsAreAvailable()
        {
            var atLeastAvailableObjectsPerCategory = 0;

            foreach (var category in objectPool.Categories)
            {
                var availableObjectsFromThisCategory = category.AssociatedObjects.Count;

                if (atLeastAvailableObjectsPerCategory == 0 || atLeastAvailableObjectsPerCategory > availableObjectsFromThisCategory) { 
                    atLeastAvailableObjectsPerCategory = availableObjectsFromThisCategory;
                }
            }

            // for the case that categories should be used exclusively
            if(atLeastAvailableObjectsPerCategory >= config.pathsToUsePerMaze)
            {
                enoughObjectsAreAvailable = true;
                return;
            }

            Debug.Log(string.Format("Max {0} objects available but expected {1}", atLeastAvailableObjectsPerCategory, config.pathsToUsePerMaze));
            enoughObjectsAreAvailable = false;
        }

        public ParadigmInstanceDefinition Generate(string subjectID, List<string> conditions)
        {
            mazeCategoryMap = new Dictionary<beMobileMaze, Category>();

            var newConfig = UnityEngine.ScriptableObject.CreateInstance<ParadigmInstanceDefinition>();

            newConfig.Subject = subjectID;

            newConfig.name = string.Format("VP_Def_{0}", subjectID);

            foreach (var condition in conditions)
            {
                var shuffledCategories = objectPool.Categories.OrderBy((i) => Guid.NewGuid()).ToList();

                availableCategories = new Stack<Category>(shuffledCategories);

                for (int i = 0; i < config.mazesToUse; i++)
                {
                    var maze = mazeInstances[i];
                    ChooseCategoryFor(maze);
                }
                
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

                var newCondition = new ConditionDefinition();

                newCondition.Trials = new List<TrialDefinition>();

                var trainingTrials = new List<TrialDefinition>();
                var experimentalTrials = new List<TrialDefinition>();

                foreach (var trialDefinition in possibleTrials)
                {
                    for (int i = 0; i < config.objectVisitationsInTraining; i++)
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

                    for (int i = 0; i < config.objectVisitationsInExperiment; i++)
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

                if (config.groupByMazes)
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

                        newCondition.Trials.AddRange(shuffledTrainingPerMaze);
                        newCondition.Trials.AddRange(shuffledExperimentPerMaze);
                    }

                }
                else
                {
                    newCondition.Trials.AddRange(trainingTrials);

                    var shuffledExperimentalTrials = experimentalTrials.OrderBy(trial => Guid.NewGuid());

                    newCondition.Trials.AddRange(shuffledExperimentalTrials);
                }

            }
               
            return newConfig;
        }

        private IEnumerable<TrialConfig> MapPathsToObjects(beMobileMaze maze, Category category)
        {
            var paths = maze.GetComponent<PathController>().Paths.Where(p => p.Available).ToArray();
            var resultConfigs = new List<TrialConfig>();

            // be aware that pathsToUsePerMaze must be up-to-date
            for (int i = 0; i < config.pathsToUsePerMaze; i++)
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
