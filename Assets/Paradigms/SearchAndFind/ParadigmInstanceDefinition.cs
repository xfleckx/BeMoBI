using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParadigmInstanceDefinition : ScriptableObject
{
    public string Subject;
    public string BodyController;
    public string HeadController;

    [SerializeField]
    public List<TrialDefinition> Trials;

}