using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Paradigms.SearchAndFind
{
    [Serializable]
    public class ParadigmConfiguration : ScriptableObject
    {
        [SerializeField]
        public bool useTeleportation = false;

        [SerializeField]
        public bool writeStatistics = false;

        [SerializeField]
        public bool logMarkerToFile = true;

        [SerializeField]
        public bool ifNoInstanceDefinitionCreateOne = false;

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
