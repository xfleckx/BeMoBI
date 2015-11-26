using UnityEngine;
using System.Collections;
using LSL;

public class LSLHeadOrientation : MonoBehaviour {

    private liblsl.StreamOutlet outlet;
    private liblsl.StreamInfo streamInfo;
    private float[] currentSample;

    public string StreamName = "BeMoBI.Unity.HeadOrientation";
    public string StreamType = "Unity.Quaternion";
    public int ChannelCount = 4;

    public Transform head;

    void Start()
    {
        currentSample = new float[ChannelCount];

        streamInfo = new liblsl.StreamInfo(StreamName, StreamType, ChannelCount, Time.fixedDeltaTime);

        outlet = new liblsl.StreamOutlet(streamInfo);
    }

    public void FixedUpdate()
    {
        var rotation = head.rotation;

        currentSample[0] = rotation.x;
        currentSample[1] = rotation.y;
        currentSample[2] = rotation.z;
        currentSample[3] = rotation.w;
        outlet.push_sample(currentSample);
    }
}
