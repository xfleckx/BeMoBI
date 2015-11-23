using UnityEngine;
using System.Collections;

public class LSLMarkerStream : AMarkerStream {

	public string lslStreamName = "Unity_Paradigma";
	public string lslStreamType = "LSL_Marker_Strings";

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
				lslChannelFormat);

		lslOutlet = new LSL.liblsl.StreamOutlet(lslStreamInfo);
	}
	  
	public override void Write(string name, float customTimeStamp)
	{
		sample[0] = name;

		lslOutlet.push_sample(sample);
	}

	public override void Write(string name)
	{
		sample[0] = name;

		lslOutlet.push_sample(sample);
	}
}
