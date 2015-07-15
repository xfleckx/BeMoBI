using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PhaseSpaceController))]
public class LSLOwlStream : MonoBehaviour {

    private LSL.liblsl.StreamOutlet lslOutlet;
    private LSL.liblsl.StreamInfo lslStreamInfo;

    private const int lslChannelCount = 6; // PositionVector, Speed, Acceleration, OWL update time

    private float[] lslSample = new float[lslChannelCount];

    private const string lslStreamName = "Unity_PhaseSpace_Client";
    private const string lslStreamType = "PhaseSpace_Coordinates";

    public bool WriteToLSL = true;
    PhaseSpaceController controller;

    public List<PSMarker> markerList = new List<PSMarker>();
    public List<Vector3> markerVectorList = new List<Vector3>();

    void Start()
    {
        lslStreamInfo = new LSL.liblsl.StreamInfo(
            lslStreamName,
            lslStreamType,
            lslChannelCount,
            Time.fixedTime,
            LSL.liblsl.channel_format_t.cf_float32);

        lslOutlet = new LSL.liblsl.StreamOutlet(lslStreamInfo);

        EnableLSLWriting();
    }

    public void EnableLSLWriting()
    {
        controller.tracker.OwlUpdateCallbacks += ProcessOwlUpdate;
    }

    public void DisableLSLWriting()
    {
       controller.tracker.OwlUpdateCallbacks -= ProcessOwlUpdate;
    }

    void ProcessOwlUpdate()
    {
        var chunk = GenerateChunkFromMarkerList(controller.Marker, controller.AvailableMarker);
        lslOutlet.push_chunk(chunk);
    }

    private float[,] GenerateChunkFromMarkerList(IEnumerable<PSMarker> newMarker, int count)
    { 
        float[,] chunk = new float[count, lslChannelCount];

        for (int i = 0; i < count; i++)
        {
            var marker = markerList[i];

            chunk[i, 0] = marker.position.x;
            chunk[i, 1] = marker.position.y;
            chunk[i, 2] = marker.position.z;

            chunk[i, 3] = controller.tracker.OWLUpdateTook;
        }

        return chunk;
    }

}
