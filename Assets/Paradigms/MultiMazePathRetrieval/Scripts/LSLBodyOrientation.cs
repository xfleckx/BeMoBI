using UnityEngine;
using System.Collections;
using LSL;

public class LSLBodyOrientation : MonoBehaviour {


    private liblsl.StreamOutlet outlet;
    private liblsl.StreamInfo streamInfo;
    private float[] currentSample;

    public string StreamName = "BeMoBI.Unity.BodyOrientation";
    public string StreamType = "Unity.Quarternion";
    public int ChannelCount = 4;

    public Transform body;

    void Start()
    {
        currentSample = new float[ChannelCount];

        streamInfo = new liblsl.StreamInfo(StreamName, StreamType, ChannelCount, Time.fixedDeltaTime);

        outlet = new liblsl.StreamOutlet(streamInfo);
    }

    public void FixedUpdate()
    {
        var rotation = body.rotation;

        currentSample[0] = rotation.x;
        currentSample[1] = rotation.y;
        currentSample[2] = rotation.z;
        currentSample[3] = rotation.w;
        outlet.push_sample(currentSample);
    }
}
