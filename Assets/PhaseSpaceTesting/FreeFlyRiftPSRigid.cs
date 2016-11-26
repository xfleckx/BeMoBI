using UnityEngine;
using System.Collections;

public class FreeFlyRiftPSRigid : OWLRigidController
{
    public override void Start()
    {
        OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();

        rig.UpdatedAnchors += Rig_UpdatedAnchors;
    }

    private void Rig_UpdatedAnchors(OVRCameraRig obj)
    {
        obj.transform.position = prevPos;

    }

    //
    void OnPreRender()
    {
        // call right before render as well, to ensure most recent data.
        _Update();
    }
}

