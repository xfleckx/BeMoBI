using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace Assets.BeMoBI.Paradigms.SearchAndFind
{ 
    public class ParadigmInstanceDefinition : ScriptableObject
    {
        public string Subject;

        public ParadigmConfiguration Configuration;

        [SerializeField]
        public List<TrialDefinition> Trials;

    }
    
    /// <summary>
    /// 
    /// </summary>
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

    /// <summary>
    /// A temporary configuration of values describing the configuration of a trial
    /// </summary>
    /// 
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