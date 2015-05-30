﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public enum SubjectControlMode { None, Joystick, PhaseSpace }

[RequireComponent(typeof(LSLMarkerStream))]
public class MultiMazePathRetrieval : MonoBehaviour {

	public VirtualRealityManager environment;
	public HUDInstruction instructions;
	public LSLMarkerStream markerStream;

	public Training training;
	 
	public ITrial currentTrial;

	private ITrial lastTrial;

	void Awake()
	{
		if (environment == null)
			throw new MissingReferenceException("Reference to VirtualRealityManager is missing");
		
		if (markerStream == null)
			throw new MissingReferenceException("Reference to a MarkerStream instance is missing");

		if (instructions == null)
			throw new MissingReferenceException("No HUD available, you are not able to give visual instructions");
		
	}

	public void Begin(Training training)
	{ 
		currentTrial = training;
		training.Initialize(8, 1, SubjectControlMode.Joystick);
		training.StartTrial();
	}
	 
	void currentTrial_Finished()
	{
		currentTrial.CleanUp();

		lastTrial = currentTrial;

		DecideOnNextTrial();
	}

	private void DecideOnNextTrial(){

		if (lastTrial.GetType() == typeof(Training))
		{
			currentTrial = GetNextTrial(training);
		}

	}

	Training GetNextTrial(Training recycle)
	{
		int lastPath = recycle.currentPathID;

		var allPaths = recycle.pathController.GetAvailablePathIDs();

		var allPathsExceptLastPath = allPaths.Except(new int[] { lastPath });

		var rand = new System.Random();
		int randIndex = rand.Next(allPathsExceptLastPath.Count());

		int newPathID = allPathsExceptLastPath.ElementAt(randIndex);

		recycle.Initialize(recycle.mazeID, newPathID, SubjectControlMode.Joystick);

		return recycle;
	}
}


public static class MarkerPattern {

	public const string BeginTrial = "{0}_{1}_{2}_BeginTrial";
	public const string L = "L";
	public const string R = "R";
	public const string Turn = "{0}_Turn";
	public const string Correct = "Correct";
	public const string Incorrect = "Incorrect";
}
