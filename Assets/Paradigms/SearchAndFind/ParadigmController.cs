using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Assets.Paradigms.SearchAndFind;
using System.Diagnostics;
using System.Collections;
using NLog;

using Debug = UnityEngine.Debug;

using Logger = NLog.Logger;

public enum SubjectControlMode { None, Joystick, PhaseSpace }

[RequireComponent(typeof(LSLMarkerStream))]
public class ParadigmController : MonoBehaviour
{
    private static Logger appLog = LogManager.GetLogger("App");

    private static Logger statistic = LogManager.GetLogger("Statistics");

    private bool isRunning = false;

    private ParadigmRunStatistics runStatistic;

    #region Constants

    private const string ParadgimConfigDirectoryName = "ParadigmConfig";

    private const string ParadigmConfigNamePattern = "VP_{0}_{1}";

    private const string DateTimeFileNameFormat = "yyyy-MM-dd_hh-mm";

    private const string DetailedDateTimeFileNameFormat = "yyyy-MM-dd_hh-mm-ss-tt";

    #endregion

    public string SubjectID = String.Empty;

    public ParadigmInstanceDefinition InstanceDefinition;
    public VirtualRealityManager VRManager;
    public StartPoint startingPoint;
    public HUD_Instruction hud;
    public LSLMarkerStream markerStream;
    public GameObject objectPresenter;
    public ObjectPool objectPool;
    public Transform objectPositionAtTrialStart;
    public GameObject HidingSpotPrefab;
    public GameObject entrance;
    public FullScreenFade fading;

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

        GlobalDiagnosticsContext.Set("subject_Id", SubjectID);
        
        appLog.Info("Initializing Paradigm");

        hud.Clear();

        fading.StartFadeIn();
    }


    void Update()
    {
        if (Input.GetKey(KeyCode.F5) && !IsRunning)
            RunAll();
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

    private bool shouldEnd;
    
    public bool IsRunning
    {
        get
        {
            return isRunning;
        }
    }

    #endregion

    #region Trial Management

    void NextTrial()
    {
        isRunning = true;

        if (currentDefinition == null) {

            currentDefinition = trials.First;
        }
        else if (shouldEnd || currentDefinition.Next == null)
        {
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

        }else if (next.TrialType.Equals(typeof(Pause).Name))
        {
            UnityEngine.Debug.Log("Pause Trial");

            Begin(pause, next);
        }
    }


    public void RunAll()
    {
        trials = new LinkedList<TrialDefinition>(InstanceDefinition.Trials);

        appLog.Info(string.Format("Run complete paradigma as defined in {0}!", InstanceDefinition.name));

        runStatistic = new ParadigmRunStatistics();

        statistic.Info(string.Format("Starting new Paradigm Instance: VP_{0}", InstanceDefinition.Subject));

        NextTrial();
    }

    public void RunOnly<T>() where T : Trial
    {
        var nameOfTrialsToSelect = typeof(T).Name;

        appLog.Info(string.Format("Run only {0} trials!", nameOfTrialsToSelect));

        var selectedTrials = InstanceDefinition.Trials.Where((def) => def.TrialType.Equals(nameOfTrialsToSelect));

        trials = new LinkedList<TrialDefinition>(selectedTrials);

        runStatistic = new ParadigmRunStatistics();

        NextTrial();
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
        currentTrial.fading = this.fading;
        
        var def = currentDefinition.Value;

        currentTrial.Initialize(def.MazeName, def.Path, def.Category, def.ObjectName);

        currentTrial.Finished += currentTrial_Finished;
    }
     
    void currentTrial_Finished(Trial trial, TrialResult result)
    {
        runCounter[trial]++;

        var trialType = trial.GetType().Name;

        statistic.Trace(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", trialType, trial.currentMazeName, trial.currentPathID, trial.objectToRemember.name, result.Duration.TotalMinutes));

        runStatistic.Add(trialType, trial.currentMazeName, trial.currentPathID, result);

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

    public void ForceSaveEnd()
    {
        this.shouldEnd = true;
    }

    private void ParadigmInstanceFinished()
    {
        isRunning = false;

        hud.ShowInstruction("You made it!\nThx for participation!","Experiment finished!");
        var completeTime = runStatistic.Trials.Sum(t => t.DurationInSeconds) / 60;

        double averageTimePerTraining = 0;

        if (runStatistic.Trials.Any(t => t.TrialType.Equals(typeof(Training).Name)))
            averageTimePerTraining = runStatistic.Trials.Where(t => t.TrialType.Equals(typeof(Training).Name)).Average(t => t.DurationInSeconds) / 60;

        double averageTimePerExperiment = 0;

        if (runStatistic.Trials.Any(t => t.TrialType.Equals(typeof(Experiment).Name)))
          averageTimePerExperiment = runStatistic.Trials.Where(t => t.TrialType.Equals(typeof(Experiment).Name)).Average(t => t.DurationInSeconds) / 60;
        
        statistic.Info(string.Format("Run took: {0} minutes, Avg Training: {1}     Avg Experiment {2}", completeTime, averageTimePerTraining, averageTimePerExperiment));

        appLog.Info("Paradigma run finished");
    }
    
    #endregion
    
    #region Public interface for controlling the paradigm remotely
    
    public void InjectPauseTrial()
    {
        var pauseTrial = new TrialDefinition()
        {
            TrialType = typeof(Pause).Name
        };

        trials.AddAfter(currentDefinition, new LinkedListNode<TrialDefinition>(pauseTrial));
    }

    public void ReturnFromPauseTrial()
    {
        if (currentTrial.Equals(pause))
            currentTrial.ForceTrialEnd();
    }

    public void PerformSaveInterupt()
    {

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
    public const string BeginTrial = "BeginTrial#{0}#{1}#{2}#{3}#{4}";

    public const string Unit = "{0}#Unit#{1}#{2}";

    public const string Enter = "Entering#{0}#{1}#{2}";

    public const string ObjectFound = "ObjectFound#{0}#{1}#{2}#{3}";
    
    // TODO: Grid ID's should not be renderered as float values
    /// <summary>
    /// From {0} to {1} TurnType {2} UnitType {3}
    /// </summary>
    public const string Turn = "Turn#From:{0}#To:{1}#{2}#{3}";

    /// <summary>
    /// From {0} to {1} expected {2} TurnType {3} UnitType {4}
    /// </summary>
    public const string WrongTurn = "Incorrect#From:{0}#To:{1}#Exp:{2}#{3}#{4}";

    public const string EndTrial = "EndTrial#{0}#{1}#{2}#{3}#{4}";

    public static string FormatBeginTrial(string trialTypeName, string mazeName, int pathId, string objectName, string categoryName)
    {
        return string.Format(BeginTrial, trialTypeName, mazeName, pathId, objectName, categoryName);
    }

    public static string FormatCorrectTurn(PathElement lastPathElement, PathElement currentPathElement)
    {
        var lastGridId = lastPathElement.Unit.GridID;

        var currentGridId = currentPathElement.Unit.GridID;

        return string.Format(Turn, lastGridId, currentGridId, lastPathElement.Type, lastPathElement.Turn);
    }

    public static string FormatIncorrectTurn(MazeUnit wrongUnitEntered, PathElement lastPathElement, PathElement expectedUnit)
    {
        var wrongGridId = wrongUnitEntered.GridID;

        var lastGridId = lastPathElement.Unit.GridID;

        var expectedGridId = expectedUnit.Unit.GridID;

        return string.Format(WrongTurn, lastGridId, wrongGridId, expectedGridId, lastPathElement.Type, lastPathElement.Turn);
    }

    public static string FormatFoundObject(string currentMazeName, int iD, string objectName, string categoryName)
    {
        return string.Format(ObjectFound, currentMazeName, iD, objectName, categoryName);
    }

    public static string FormatEndTrial(string trialTypeName, string mazeName, int pathId, string objectName, string categoryName)
    {
        return string.Format(EndTrial, trialTypeName, mazeName, pathId, objectName, categoryName);
    }
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

public class ParadigmInstanceDefinition : ScriptableObject
{ 
    public string Subject;
    public string BodyController;
    public string HeadController;

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

public class ParadigmRunStatistics
{
    private string subjectId = String.Empty;
    
    public class TrialStatistic
    {
        private string mazeName;
        private int path;
        private double seconds;
        private string trialType;

        public TrialStatistic(string trialType, string mazeName, int path, double seconds)
        {
            this.trialType = trialType;
            this.mazeName = mazeName;
            this.path = path;
            this.seconds = seconds;
        }

        public string MazeName
        {
            get
            {
                return mazeName;
            }
        }

        public int Path
        {
            get
            {
                return path;
            }
        }

        public double DurationInSeconds
        {
            get
            {
                return seconds;
            }
        }

        public string TrialType
        {
            get
            {
                return trialType;
            }
        }
    }

    public List<TrialStatistic> Trials = new List<TrialStatistic>();

    public string SubjectId
    {
        get
        {
            return subjectId;
        }
    }

    public void Add(string trialType, string mazeName, int pathId, TrialResult result)
    {
        Trials.Add(new TrialStatistic(trialType, mazeName, pathId, result.Duration.TotalSeconds));
    }
}