//
// PhaseSpace, Inc. 2014
//
using UnityEngine;
using System;
using System.Collections;

//
// Note: Disable Orientation in OVRCameraController if using Oculus Rift
//

// attach to a camera instead of generic game object
public class OWLRigidCameraController : OWLRigidController
{
	//
	void OnPreRender ()
	{
		// call right before render as well, to ensure most recent data.
		_Update ();
	}
}
