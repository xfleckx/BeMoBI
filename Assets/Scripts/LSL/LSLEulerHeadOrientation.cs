﻿using UnityEngine;
using System.Collections;
using LSL;
using Assets.LSL4Unity.Scripts;

public class LSLEulerHeadOrientation : MonoBehaviour {

    private const string unique_source_id = "26833393C12A45ACB1EB1FBEFA8B696C";

    private liblsl.StreamOutlet outlet;
    private liblsl.StreamInfo streamInfo;
    private float[] currentSample;

    public string StreamName = "BeMoBI.Unity3D.HeadOrientation";
    public string StreamType = "Unity3D.Euler";
    public int ChannelCount = 3;

    public Transform head;

    void Start()
    {
        currentSample = new float[ChannelCount];

        streamInfo = new liblsl.StreamInfo(StreamName, StreamType, ChannelCount, Time.fixedDeltaTime, liblsl.channel_format_t.cf_float32, unique_source_id);

        try
        {
            outlet = new liblsl.StreamOutlet(streamInfo);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void LateUpdate()
    {
        if (outlet == null)
            return;

        var rotation = head.rotation.eulerAngles;

        currentSample[0] = rotation.x;
        currentSample[1] = rotation.y;
        currentSample[2] = rotation.z;
        outlet.push_sample(currentSample, LSLTimeSync.Instance.UpdateTimeStamp);
    }
}
