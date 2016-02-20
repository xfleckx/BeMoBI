using Assets.BeMoBI.Scripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.BeMoBI.Paradigms.SearchAndFind
{
    public class ParadigmModelFactory
    {
        public ParadigmConfiguration config;
        
        public ParadigmModelFactory()
        {
            objectPool = UnityEngine.Object.FindObjectOfType<ObjectPool>();

            vrManager = UnityEngine.Object.FindObjectOfType<VirtualRealityManager>();

            mazeInstances = vrManager.transform.AllChildren().Where(c => c.GetComponents<beMobileMaze>() != null).Select(c => c.GetComponent<beMobileMaze>()).ToList();

            availableCategoriesCount = objectPool.Categories.Count;
            availableMazesCount = mazeInstances.Count;
        }

        public int atLeastAvailblePathsPerMaze = 0;

        private bool enoughPathsAreAvailable = false;
        private bool enoughObjectsAreAvailable = false;

        private ObjectPool objectPool;
        private List<beMobileMaze> mazeInstances;
        private VirtualRealityManager vrManager;

        int availableCategoriesCount;

        int availableMazesCount;

        public void EstimateConfigBasedOnAvailableElements()
        {
            foreach (var condConfig in config.conditionConfigurations)
            {
                if (condConfig.mazesToUse > availableMazesCount)
                {
                    var message = string.Format("Warning! You want to use more mazes {0} than available {1}", condConfig.mazesToUse, availableMazesCount);

                    throw new ArgumentException(message);
                }

                if (condConfig.useExactOnCategoryPerMaze)
                {
                    if (condConfig.mazesToUse > availableCategoriesCount) {

                        var message = string.Format("Error on 'useExactOnCategoryPerMaze! Not enough categories {0} than mazes {1}", condConfig.mazesToUse, availableCategories);

                        throw new ArgumentException(message);
                    }
                } // warning no else condition defined! TODO... what should happen when multiple categories per maze are available?

                CheckIfEnoughPathsAreAvailable(condConfig);

                CheckIfEnoughObjectsAreAvailable(condConfig);
            }


            
        }

        #region Generator logic - bad code here... needs to be encapsulated


        private Dictionary<beMobileMaze, Category> mazeCategoryMap;
        // use stack for asserting that every category will be used once
        private Stack<Category> availableCategories;
        
        public bool CheckGenerationConstraints(ConditionConfiguration config)
        {
            var result = objectPool != null;
            result = mazeInstances.Count >= config.mazesToUse && (config.mazesToUse > 0) && result;
            result = enoughPathsAreAvailable && result;
            result = enoughObjectsAreAvailable && result;
            
            return result;
        }

        public void CheckIfEnoughPathsAreAvailable(ConditionConfiguration config)
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

        public void CheckIfEnoughObjectsAreAvailable(ConditionConfiguration config)
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

            UnityEngine.Debug.Log(string.Format("Max {0} objects available but expected {1}", atLeastAvailableObjectsPerCategory, config.pathsToUsePerMaze));
            enoughObjectsAreAvailable = false;
        }

        public ParadigmModel Generate(string subjectID, List<ConditionConfiguration> availableConfigurations)
        {
            mazeCategoryMap = new Dictionary<beMobileMaze, Category>();

            var newModel = UnityEngine.ScriptableObject.CreateInstance<ParadigmModel>();

            newModel.Subject = subjectID;

            newModel.name = string.Format("Model_VP_{0}", subjectID);

            newModel.Conditions = new List<ConditionDefinition>();

            foreach (var conditionConfig in availableConfigurations)
            {
                var shuffledCategories = objectPool.Categories.Shuffle();

                availableCategories = new Stack<Category>(shuffledCategories);

                var shuffledMazes = mazeInstances.Shuffle();
                var selectedMazes = shuffledMazes.Take(conditionConfig.mazesToUse); ;

                foreach (var maze in selectedMazes) { 
                    
                    ChooseCategoryFor(maze);
                }
                
                #region create all possible trial configurations

                var possibleTrials = new List<TrialConfig>();

                foreach (var association in mazeCategoryMap)
                {
                    var maze = association.Key;
                    var category = association.Value;

                    var trialConfigs = MapPathsToObjects(maze, category, conditionConfig);
                    possibleTrials.AddRange(trialConfigs);
                }

                #endregion

                #region now create the actual Paradigma instance defintion by duplicating the possible configurations for trianing and experiment

                var newCondition = new ConditionDefinition();

                newCondition.Identifier = conditionConfig.ConditionID;

                newCondition.Trials = new List<TrialDefinition>();

                var trainingTrials = new List<TrialDefinition>();
                var experimentalTrials = new List<TrialDefinition>();

                foreach (var trialDefinition in possibleTrials)
                {
                    for (int i = 0; i < conditionConfig.objectVisitationsInTraining; i++)
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

                    for (int i = 0; i < conditionConfig.objectVisitationsInExperiment; i++)
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

                if (conditionConfig.groupByMazes)
                {
                    var tempAllTrials = new List<TrialDefinition>();
                    tempAllTrials.AddRange(trainingTrials);
                    tempAllTrials.AddRange(experimentalTrials);

                    var groupedByMaze = tempAllTrials.GroupBy((td) => td.MazeName);

                    foreach (var group in groupedByMaze)
                    {
                        var groupedByPath = group.GroupBy(td => td.Path).Shuffle();

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
                        var shuffledTrainingPerMaze = trainingPerMaze.Shuffle();
                        var shuffledExperimentPerMaze = experimentPerMaze.Shuffle();

                        newCondition.Trials.AddRange(shuffledTrainingPerMaze);
                        newCondition.Trials.AddRange(shuffledExperimentPerMaze);
                    }

                }
                else
                {
                    newCondition.Trials.AddRange(trainingTrials);

                    var shuffledExperimentalTrials = experimentalTrials.Shuffle();

                    newCondition.Trials.AddRange(shuffledExperimentalTrials);
                }

                newCondition.Config = conditionConfig;
                newModel.Conditions.Add(newCondition);
            }
               
            return newModel;
        }

        private IEnumerable<TrialConfig> MapPathsToObjects(beMobileMaze maze, Category category, ConditionConfiguration config)
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


    /// <summary>
    /// A temporary configuration of values describing the configuration of a trial
    /// this is used during the generation process
    /// </summary>
    [DebuggerDisplay("{MazeName} {Path} {Category} {ObjectName}")]
    public struct TrialConfig : ICloneable
    {
        public string MazeName;
        public int Path;
        public string Category;
        public string ObjectName;

        public object Clone()
        {
            return new TrialConfig()
            {
                MazeName = this.MazeName,
                Path = this.Path,
                Category = this.Category,
                ObjectName = this.ObjectName
            };
        }
    }
}
