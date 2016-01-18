//
// PhaseSpace, Inc. 2015
//
using UnityEngine;
using System.Collections;
using PhaseSpace;

public class OWLMarkerController : MonoBehaviour
{
	/// <summary>
	/// The OWLTracker to use.
	/// </summary>
	public OWLTracker Tracker;
	
	/// <summary>
	/// The OWL tracker id
	/// </summary>
	public int TrackerID = 0;
	
	/// <summary>
	/// The OWL marker index.
	
	/// </summary>
	public int MarkerID = 0;
	
	/// <summary>
	/// Update during OnPreRender to reduce latency between data acquisition and rendering.
	/// </summary>
	public bool UpdateOnPreRender = false;
	
	/// <summary>
	/// The most recent marker acquired from Tracker
	/// </summary>
	public Marker lastMarker = new PhaseSpace.Marker ();
	protected Marker accumulator = new PhaseSpace.Marker ();
		
		
	// Use this for initialization
	void Start ()
	{
	
	}
	
	void OnPreRender ()
	{		
		if (UpdateOnPreRender)
			_Update ();
	}
	
	// Update is called once per frame
	void Update ()
	{	
		if (UpdateOnPreRender)
			return;
		_Update ();
	}
	
	void _Update ()
	{
		try {
			if (!(enabled && Tracker.Connected ()))
				return;
		} catch (System.NullReferenceException) {
			Debug.LogError ("Tracker is null");
			return;
		}    
				
		Marker m = Tracker.GetMarker (TrackerID, MarkerID);
		if (m != null && m.cond > 0) {
			lastMarker = m;
			transform.localPosition = lastMarker.position;
		}		
	}
}
