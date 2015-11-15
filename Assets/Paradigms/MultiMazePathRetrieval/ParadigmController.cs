using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using Assets.Paradigms.MultiMazePathRetrieval;
using System.Diagnostics;

public enum SubjectControlMode { None, Joystick, PhaseSpace }

[RequireComponent(typeof(LSLMarkerStream))]
public class ParadigmController : MonoBehaviour
{

    private const string ParadgimConfigDirectoryName = "ParadigmConfig";

    private const string ParadigmConfigNamePattern = "VP_{0}_{1}";

    private const string DateTimeFileNameFormat = "yyyy-MM-dd_hh-mm";

    private const string DetailedDateTimeFileNameFormat = "yyyy-MM-dd_hh-mm-ss-tt";

    public VirtualRealityManager environment;
    public HUD_Instruction instructions;
    public LSLMarkerStream markerStream;
    public StartPoint startingPoint;
    public GameObject objectPresenter;
    public ObjectPool objectPool;

    public ParadigmInstanceDefinition InstanceDefinition;

    #region Trials

    public Training training;
    public Experiment experiment;
    public Pause pause;
    public InstructionTrial instruction;

    #endregion

    public ITrial currentTrial;

    private ITrial lastTrial;

    private Dictionary<ITrial, int> runCounter = new Dictionary<ITrial, int>();
    
    void Awake()
    {
        if (environment == null)
            throw new MissingReferenceException("Reference to VirtualRealityManager is missing");

        if (markerStream == null)
            throw new MissingReferenceException("Reference to a MarkerStream instance is missing");

        if (instructions == null)
            throw new MissingReferenceException("No HUD available, you are not able to give visual instructions");

    }

    void Start()
    {
        int vpId = 0; // for prototyping reasons

        string paradigmInstanceConfigFileName = string.Format(ParadigmConfigNamePattern, vpId, DateTime.Now.ToString(DateTimeFileNameFormat));

        string pathToConfigDirectory = Path.Combine(Application.dataPath, ParadgimConfigDirectoryName);

        string FullPathToParadigmInstanzConfig = Path.Combine(pathToConfigDirectory, paradigmInstanceConfigFileName);
        
    }

    public void Begin(InstructionTrial trial)
    {

    }

    public void Begin(Training training)
    {
        if (runCounter.ContainsKey(training))
            runCounter[training]++;
        else
            runCounter.Add(training, 0);

        training.marker = markerStream;
        currentTrial = training;
        training.Initialize(8, 1, SubjectControlMode.Joystick);
        training.StartTrial();
    }

    public void Begin(Pause trial)
    {

    }

    public void Begin(Experiment experiment)
    {
        if (runCounter.ContainsKey(experiment))
            runCounter[experiment]++;
        else
            runCounter.Add(experiment, 0);

        experiment.marker = markerStream;
        currentTrial = experiment;
        experiment.Initialize(8, 1, SubjectControlMode.Joystick);
        experiment.StartTrial();
    }

    void currentTrial_Finished()
    {
        runCounter[currentTrial]++;

        currentTrial.CleanUp();

        lastTrial = currentTrial;

        DecideOnNextTrial();
    }

    void Update()
    {
        //if (Input.GetKey(KeyCode.Space))
        //	instructions.StopDisplaying();

        //if(Input.GetKey(KeyCode.S))
        //	Begin(training);
    }

    private void DecideOnNextTrial()
    {

        if (InstanceDefinition == null)
        {
            currentTrial = GetRandomTrial();

            return;
        }

        //if (config.TrialType.Equals(typeof(Instruction).Name))
        //{
        //    return instruction;
        //}

        //if (config.TrialType.Equals(typeof(Pause).Name))
        //{
        //    return pause;
        //}

        //if (config.TrialType.Equals(typeof(Training).Name))
        //{
        //    return training;
        //}

        //if (config.TrialType.Equals(typeof(Experiment).Name))
        //{
        //    return experiment;
        //}

        // throw new ArgumentException(string.Format("Expected a Trial Type \"{0}\" which seems to be not implemented!", config.TrialType));


        if (lastTrial is InstructionTrial)
        {
            throw new NotImplementedException("TODO: Implement logic to get into the the Training!");
        }

        if (lastTrial is Pause)
        {
            throw new NotImplementedException("TODO: Implement logic to get back Pause into the Training or Experiment!");
        }


        if (lastTrial.GetType() == typeof(Training))
        {
            currentTrial = GetNextTrial(training);
        }
    }

    Training GetNextTrial(Training lastTrainingTrial)
    {
        int lastPath = lastTrainingTrial.currentPathID;

        var allPaths = lastTrainingTrial.pathController.GetAvailablePathIDs();

        var allPathsExceptLastPath = allPaths.Except(new int[] { lastPath });

        var rand = new System.Random();
        int randIndex = rand.Next(allPathsExceptLastPath.Count());

        int newPathID = allPathsExceptLastPath.ElementAt(randIndex);

        lastTrainingTrial.Initialize(lastTrainingTrial.mazeID, newPathID, SubjectControlMode.Joystick);
        lastTrainingTrial.RunCount = runCounter[lastTrainingTrial];
        return lastTrainingTrial;
    }

    Trial GetRandomTrial()
    {
        Trial result;

        var rand = new System.Random().Next(0, 1);

        if (rand > 0)
            result = experiment;
        else
            result = training;

        return result;
    }

}

public static class MarkerPattern
{

    public const string BeginTrial = "{0}_{1}_{2}_BeginTrial";
    public const string L = "L";
    public const string R = "R";
    public const string Turn = "{0}_Turn";
    public const string Correct = "Correct";
    public const string Incorrect = "Incorrect";
    public const string Unit = "{0}_Unit_{1}_{2}";
    public const string Enter = "Entering_{0}_{1}_{2}";

}


public class ParadigmInstanceDefinition : ScriptableObject //, ISerializationCallbackReceiver
{
    public string BodyController;
    public string HeadController;

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
public class TrialDefinition
{
    public string TrialType;
    public string MazeName;
    public int Path;
    public string Category;
    public string ObjectName;
}

/// <summary>
/// A temporary configuration of values describing the configuration of a trial
/// </summary>
/// 
[DebuggerDisplay("{MazeName} {Path} {Category} {ObjectName}")]
internal struct TrialConfig : ICloneable
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