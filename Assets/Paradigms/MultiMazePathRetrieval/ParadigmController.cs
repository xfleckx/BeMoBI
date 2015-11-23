using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using Assets.Paradigms.MultiMazePathRetrieval;
using System.Diagnostics;
using System.Collections;

public enum SubjectControlMode { None, Joystick, PhaseSpace }

[RequireComponent(typeof(LSLMarkerStream))]
public class ParadigmController : MonoBehaviour
{
    #region Constants

    private const string ParadgimConfigDirectoryName = "ParadigmConfig";

    private const string ParadigmConfigNamePattern = "VP_{0}_{1}";

    private const string DateTimeFileNameFormat = "yyyy-MM-dd_hh-mm";

    private const string DetailedDateTimeFileNameFormat = "yyyy-MM-dd_hh-mm-ss-tt";
    
    #endregion

    public VirtualRealityManager VRManager;
    public HUD_Instruction hud;
    public LSLMarkerStream markerStream;
    public StartPoint startingPoint;
    public GameObject objectPresenter;
    public ObjectPool objectPool;
    public Transform objectPositionAtTrialStart;
    public GameObject HidingSpotPrefab;
    public GameObject entrance;
    public ParadigmInstanceDefinition InstanceDefinition;

    public float TimeToWaitTilNewTrialStarts = 5f;

    void Awake()
    {
        if (VRManager == null)
            throw new MissingReferenceException("Reference to VirtualRealityManager is missing");

        if (markerStream == null)
            throw new MissingReferenceException("Reference to a MarkerStream instance is missing");

        if (hud == null)
            throw new MissingReferenceException("No HUD available, you are not able to give visual instructions");
        
    }

    void Start()
    {
        if(InstanceDefinition == null)
        {
            UnityEngine.Debug.Log("No instance definition loaded.");
            return;
        }

        trials = new LinkedList<TrialDefinition>(InstanceDefinition.Trials);
    }

    #region Trials

    private LinkedList<TrialDefinition> trials;
    private LinkedListNode<TrialDefinition> currentDefinition;

    public Training training;
    public Experiment experiment;
    public Pause pause;
    public InstructionTrial instruction;

    public Trial currentTrial;
    
    private Dictionary<ITrial, int> runCounter = new Dictionary<ITrial, int>();

    #endregion

    #region Trial Management

    void NextTrial()
    {
        if (currentDefinition == null) {
            UnityEngine.Debug.Log("First Trial");
            currentDefinition = trials.First;
        }
        else if (currentDefinition.Next == null)
        {
            UnityEngine.Debug.Log("Last Trial ended");
            ParadigmInstanceFinished();
            return;
        }
        else
        {
            currentDefinition = currentDefinition.Next;
        }

        var next = currentDefinition.Value;

        if (next.TrialType.Equals(typeof(Training).Name))
        {
            UnityEngine.Debug.Log("Trainings Trial");
            
            Begin(training, next);
        }
        else if (next.TrialType.Equals(typeof(Experiment).Name))
        {
            UnityEngine.Debug.Log("Experiment Trial");

            Begin(experiment, next);
        }
    }
    
    public void Begin<T>(T trial, TrialDefinition trialDefinition) where T : Trial
    {
        if (!runCounter.ContainsKey(trial)) 
            runCounter.Add(trial, 0);

        currentTrial = trial;

        Prepare(currentTrial);
        
        currentTrial.StartTrial();
    }

    private void Prepare(Trial currentTrial)
    {
        currentTrial.VRManager = this.VRManager;
        currentTrial.marker = this.markerStream;
        currentTrial.hud = this.hud;
        currentTrial.hidingSpotPrefab = this.HidingSpotPrefab;
        currentTrial.objectPool = this.objectPool;
        currentTrial.MazeEntranceDoor = this.entrance;
        currentTrial.positionAtTrialBegin = objectPositionAtTrialStart;
        currentTrial.ObjectDisplaySocket = objectPresenter;
        currentTrial.startPoint = this.startingPoint;

        var def = currentDefinition.Value;

        currentTrial.Initialize(def.MazeName, def.Path, def.Category, def.ObjectName);

        currentTrial.Finished += currentTrial_Finished;
    }
     
    void currentTrial_Finished()
    {
        runCounter[currentTrial]++;
        
        currentTrial.CleanUp();

        // TODO: replace with a more immersive door implementation
        entrance.SetActive(true);

       StartCoroutine( WaitForNextTrial() );
    }

    private IEnumerator WaitForNextTrial()
    {
        yield return new WaitForSeconds(TimeToWaitTilNewTrialStarts);

        NextTrial();
    }

    private void ParadigmInstanceFinished()
    {
        throw new NotImplementedException("TODO");
    }
    
    #endregion
    
    #region Public interface for controlling the paradigm remotely

    public void StartParadigmInstance()
    {
        hud.Clear();

        NextTrial();
    }

    public void InjectPauseTrial()
    {
        var pauseTrial = new TrialDefinition()
        {
            TrialType = typeof(Pause).Name
        };

        trials.AddAfter(currentDefinition, new LinkedListNode<TrialDefinition>(pauseTrial));
    }

    public void SaveCurrentState()
    {
        throw new NotImplementedException("TODO");
    }

    public void LoadState()
    {
        throw new NotImplementedException("TODO");
    }

    #endregion
}

public static class MarkerPattern
{
    public const string BeginTrial = "{0}_{1}_{2}_{3}_BeginTrial";
    public const string L = "L";
    public const string R = "R";
    public const string Turn = "{0}_Turn";
    public const string Correct = "Correct";
    public const string Incorrect = "Incorrect";
    public const string Unit = "{0}_Unit_{1}_{2}";
    public const string Enter = "Entering_{0}_{1}_{2}";
}

#region TODO: Save instance state (SaveGames)
//https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
[Serializable]
public class InstanceState
{
    public int last_subject_id;
    // TODO
}

#endregion

public class ParadigmInstanceDefinition : ScriptableObject //, ISerializationCallbackReceiver
{
    public int Subject;
    public string BodyController;
    public string HeadController;

    [SerializeField]
    public List<TrialDefinition> Trials;
    
    //[SerializeField]
    //private List<string> _keys;
    //[SerializeField]
    //private List<TrialDefinition> _values;

    //public void OnAfterDeserialize()
    //{
    //    for (int i = 0; i < _keys.Count; i++)
    //    {
    //        Trials.Add(_keys[i], _values[i]);
    //    }
    //}

    //public void OnBeforeSerialize()
    //{
    //    _keys = Trials.Keys.ToList();
    //    _values = Trials.Values.ToList();
    //}
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