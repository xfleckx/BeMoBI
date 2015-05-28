using UnityEngine;
using System.Collections;
using System;

public enum ParadigmCondition { Active, Passive }

[RequireComponent(typeof(LSLMarkerStream))]
public class MultiMazePathRetrieval : MonoBehaviour {

	public VirtualRealityManager environment;
	public HUDInstruction instructions;
	public LSLMarkerStream markerStream;

	public ITrial currentTrial;

	[SerializeField]
	public int NumberOfTrainingsTrials;

	[SerializeField]
	public int NumberOfTrialsPerCondition;

	void Awake()
	{
		if (environment == null)
			throw new MissingReferenceException("Reference to VirtualRealityManager is missing");

        markerStream = GetComponent<LSLMarkerStream>();

		if (markerStream == null)
			throw new MissingReferenceException("Reference to a MarkerStream instance is missing");

		if (instructions == null)
			throw new MissingReferenceException("No HUD available, you are not able to give visual instructions");
        
	}

	// Use this for initialization
	void Start () {
		 
		currentTrial = new Training(environment, markerStream, instructions);
		currentTrial.Initialize();

		currentTrial.OnEnd += currentTrial_OnEnd;

		currentTrial.Start();
	}

	void currentTrial_OnEnd()
	{
		

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
