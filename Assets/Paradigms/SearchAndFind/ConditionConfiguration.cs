using UnityEngine;
using System.Collections.Generic;
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
            var originalProperties = this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            var clone = new ConditionConfiguration();
            var cloneProperties = clone.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            for (int i = 0; i < originalProperties.Length; i++)
            {
                var originalValue = originalProperties[i].GetValue(this, null);
                cloneProperties[i].SetValue(clone, originalValue, null);
            }

            var originalFields = this.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            var cloneFields = clone.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            for (int i = 0; i < originalFields.Length; i++)
            {
                var originalFieldValue = originalFields[i].GetValue(this);
                cloneFields[i].SetValue(clone, originalFieldValue);
            }
            return clone;
        }

        [SerializeField]
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

        [SerializeField]
        public List<string> NamesOfMazes = new List<string>();
    }
}