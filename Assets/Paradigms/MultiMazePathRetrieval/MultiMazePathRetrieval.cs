using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum SubjectControlMode { None, Joystick, PhaseSpace }

[RequireComponent(typeof(LSLMarkerStream))]
public class MultiMazePathRetrieval : MonoBehaviour {

	public VirtualRealityManager environment;
	public HUDInstruction instructions;
	public LSLMarkerStream markerStream;

    public Training training;

    //public Experiment experiment;

    public HashSet<ITrial> TrialTypes;

	public ITrial currentTrial;

	[SerializeField]
	public int NumberOfTrainingsTrials;

	[SerializeField]
	public int NumberOfTrialsPerCondition;

	void Awake()
	{
        TrialTypes = new HashSet<ITrial>();

		if (environment == null)
			throw new MissingReferenceException("Reference to VirtualRealityManager is missing");
		
		if (markerStream == null)
			throw new MissingReferenceException("Reference to a MarkerStream instance is missing");

		if (instructions == null)
			throw new MissingReferenceException("No HUD available, you are not able to give visual instructions");
		
	}

	public void BeginTraining()
	{
        currentTrial = training;
        training.Initialize(1, 0, SubjectControlMode.Joystick);
        training.StartTrial();
	}

    public void BeginExperiment()
    {
        
    }

	void currentTrial_Finished()
	{
		currentTrial.CleanUp();

	}
	 
    ITrial GetRandomTrial()
    { 

        return null;
    }
}


public static class MarkerPattern {

	public const string BeginTrial = "{0}_{1}_{2}_BeginTrial";

}
