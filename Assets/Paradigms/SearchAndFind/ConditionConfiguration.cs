using UnityEngine;
using System.Collections;

namespace Assets.BeMoBI.Paradigms.SearchAndFind
{
    public class ConditionConfiguration : ScriptableObject
    {
        public static ConditionConfiguration GetDefault()
        {
            var config = CreateInstance<ConditionConfiguration>();

            return config;
        }

        public string ConditionID = ParadigmConfiguration.NAME_FOR_DEFAULT_CONFIG;
        
        [SerializeField]
        public bool useTeleportation = false;

        [SerializeField]
        public string BodyControllerName = "KeyboardCombi";

        [SerializeField]
        public string HeadControllerName = "KeyboardCombi";

        [SerializeField]
        public float TimeToDisplayObjectToRememberInSeconds = 3;

        [SerializeField]
        public float TimeToDisplayObjectWhenFoundInSeconds = 2;

        [SerializeField]
        public float offsetToTeleportation = 2;

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
    }
}