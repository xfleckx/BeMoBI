﻿using UnityEngine;
using System.Collections;

/// <summary>
/// This singleton should provide an dedicated timestamp for each update call or fixed update LSL sample!
/// So that each sample provided by an Unity3D app has the same timestamp 
/// Important! Make sure that the script is called before the default execution order!
/// </summary>
public class LSLTimeSync : MonoBehaviour {
    
    private static LSLTimeSync instance;
    public static LSLTimeSync Instance
    {
        get {
            return instance;
        }
    }

    private double fixedUpdateTimeStamp;
    public double FixedUpdateTimeStamp
    {
        get
        {
            return fixedUpdateTimeStamp;
        }
    }

    private double updateTimeStamp;
    public double UpdateTimeStamp
    {
        get
        {
            return updateTimeStamp;
        }
    }
    
	void Awake () {
        LSLTimeSync.instance = this;
	}
	
    void FixedUpdate()
    {
        fixedUpdateTimeStamp = LSL.liblsl.local_clock();
    }
	
	void Update () {
	    updateTimeStamp = LSL.liblsl.local_clock();
    }
}
