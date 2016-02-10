﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;
namespace Assets.BeMoBI.Paradigms.SearchAndFind
{ 
    public class ParadigmModel : ScriptableObject
    {
        public string Subject;

        public ParadigmConfiguration Configuration;
        
        [SerializeField]
        public List<ConditionDefinition> Conditions;

        public ConditionDefinition Get(string condition)
        {
            if(!Conditions.Any(c => c.Identifier.Equals(condition)))
            {
                throw new ArgumentException(string.Format("Expected condition '{0}' not found!", condition));
            }

            return Conditions.Where(c => c.Identifier.Equals(condition)).FirstOrDefault();
        }
    }


    [Serializable]
    public class ConditionDefinition
    {
        [SerializeField]
        public string Identifier = "default";

        [SerializeField]
        public ConditionConfiguration Config;

        [SerializeField]
        public List<TrialDefinition> Trials;
    }
    
    [DebuggerDisplay("{TrialType} {MazeName} Path: {Path} {Category} {ObjectName}")]
    [Serializable]
    public class TrialDefinition
    {
        [SerializeField]
        public string TrialType;
        [SerializeField]
        public string MazeName;
        [SerializeField]
        public int Path;
        [SerializeField]
        public string Category;
        [SerializeField]
        public string ObjectName;
    } 
}