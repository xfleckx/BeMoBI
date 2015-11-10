using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Xml.Serialization;
using System.IO;

public enum SubjectControlMode { None, Joystick, PhaseSpace }

[RequireComponent(typeof(LSLMarkerStream))]
public class MultiMazePathRetrieval : MonoBehaviour {

    private const string ParadgimConfigDirectoryName = "ParadigmConfig";
    private const string ParadigmConfigNamePattern = "VP_{0}_{1}";

    private const string DateTimeFileNameFormat = "yyyy-MM-dd_hh-mm";

    private const string DetailedDateTimeFileNameFormat = "yyyy-MM-dd_hh-mm-ss-tt";

    public VirtualRealityManager environment;
	public HUDInstruction instructions;
	public LSLMarkerStream markerStream;
	public StartPoint startingPoint;
    public GameObject objectPresenter;
    public ObjectPool objectPool;

    #region Trials
    public Training training;
    public Experiment experiment;
    public Pause pause;
    public InstructionTrial instruction;
    #endregion
    public ITrial currentTrial;

	private ITrial lastTrial;

	private Dictionary<ITrial, int> runCounter = new Dictionary<ITrial,int>();

    private ParadigmConfiguration ParadigmConfiguration;

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
        
        FileInfo ParadigmInstanceConfigFile = new FileInfo(FullPathToParadigmInstanzConfig);

        if (ParadigmInstanceConfigFile.Exists) {
            
            Debug.Log(string.Format("Instance config file found at {0}", ParadigmInstanceConfigFile.FullName));

            var serializer = new XmlSerializer(typeof(ParadigmConfiguration));

            var fileStream = new FileStream(ParadigmInstanceConfigFile.FullName, FileMode.Open);

            ParadigmConfiguration = serializer.Deserialize(fileStream) as ParadigmConfiguration;

            // TODO: Serialization
            if (ParadigmConfiguration == null)
            {
                throw new InvalidOperationException("Missing valid Paradigm config... Generate one!");
            }
        }
        else
        {
            Debug.LogWarning("No Instance Configuration found");
        }

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
		if (Input.GetKey(KeyCode.Space))
			instructions.StopDisplaying();

		if(Input.GetKey(KeyCode.S))
			Begin(training);
	}

	private void DecideOnNextTrial(){

        if(ParadigmConfiguration == null)
        {
            currentTrial = GetRandomTrial();

            return;
        }

        if(lastTrial is InstructionTrial)
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

    Trial GetNextTrial(TrialConfiguration config)
    {
        throw new NotImplementedException();
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

public static class MarkerPattern {

	public const string BeginTrial = "{0}_{1}_{2}_BeginTrial";
	public const string L = "L";
	public const string R = "R";
	public const string Turn = "{0}_Turn";
	public const string Correct = "Correct";
	public const string Incorrect = "Incorrect";
	public const string Unit = "{0}_Unit_{1}_{2}";
	public const string Enter = "Entering_{0}_{1}_{2}";
}

[XmlRoot("XmlDocRoot")]
class ParadigmConfiguration
{
    [XmlArray("Trials")]
    public List<TrialConfiguration> Trials; 
}

class TrialConfiguration
{
    [XmlAttribute("Type")]
    public TrialType Type;

    [XmlAttribute("Maze")]
    public int MazeNumber;

    [XmlAttribute("Categorie")]
    public int CategorieNumber;

    [XmlAttribute("Object")]
    public int ObjectNumber;

    [XmlAttribute("ControlMode")]
    public SubjectControlMode ControlMode;
}