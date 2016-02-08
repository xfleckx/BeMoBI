using UnityEngine;
using System.Collections;
using System;

namespace Assets.BeMoBI.Paradigms.SearchAndFind
{
    [Serializable]
    public class ConditionConfiguration : ICloneable
    {
        public static ConditionConfiguration GetDefault()
        {
            var config = new ConditionConfiguration();

            config.ConditionID = ParadigmConfiguration.NAME_FOR_DEFAULT_CONFIG;
             
            return config;
        }

        public object Clone()
        {

            var originalProperties = this.GetType().GetProperties();

            var clone = new ConditionConfiguration();
            var cloneProperties = clone.GetType().GetProperties();

            for (int i = 0; i < originalProperties.Length; i++)
            {
                var originalValue = originalProperties[i].GetValue(this, null);
                cloneProperties[i].SetValue(clone, originalValue, null);
            }

            return clone;
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
        public int mazesToUse = 1;

        [SerializeField]
        public int pathsToUsePerMaze = 1; // corresponds with the available objects - one distinct object per path per maze

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