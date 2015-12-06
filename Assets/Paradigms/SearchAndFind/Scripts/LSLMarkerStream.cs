using UnityEngine;
using System.Collections;
using NLog;

public class LSLMarkerStream : AMarkerStream {

    Logger markerLog = LogManager.GetLogger("MarkerLog");

    private const string unique_source_id = "D3F83BB699EB49AB94A9FA44B88882AB";
    
    public string lslStreamName = "Unity_Paradigma";
	public string lslStreamType = "LSL_Marker_Strings";

    public bool LogAlsoToFile = true;

	private LSL.liblsl.StreamInfo lslStreamInfo;
	private LSL.liblsl.StreamOutlet lslOutlet;
	private int lslChannelCount = 1;
	private double nominalRate = 0;
	private const LSL.liblsl.channel_format_t lslChannelFormat = LSL.liblsl.channel_format_t.cf_string;

	private string[] sample; 

	// Use this for initialization
	void Start () {
		sample = new string[lslChannelCount];

		lslStreamInfo = new LSL.liblsl.StreamInfo(
				lslStreamName,
				lslStreamType,
				lslChannelCount,
				nominalRate,
				lslChannelFormat,
                unique_source_id);

		lslOutlet = new LSL.liblsl.StreamOutlet(lslStreamInfo);
	}
	  
	public override void Write(string name, float customTimeStamp)
	{
		sample[0] = name;

		lslOutlet.push_sample(sample, customTimeStamp);

        if (LogAlsoToFile)
            markerLog.Info(name);
    }

	public override void Write(string name)
	{
		sample[0] = name;

		lslOutlet.push_sample(sample);

        if (LogAlsoToFile)
            markerLog.Info(name);
	}
}
