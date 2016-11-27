
using UnityEngine.Assertions;

/// <summary>
/// This an instance of this class
/// </summary>
public class FreeFlyRiftPSRigid : OWLCustomLRigidController
{
    public OVRCameraRig targetCamRig;

    public override void Start()
    {
        if(targetCamRig == null)
            targetCamRig = FindObjectOfType<OVRCameraRig>();

        Assert.IsNotNull(targetCamRig, "There is no OVRCameraRig Instance in the Scene.");
        
        targetCamRig.UpdatedAnchors += Rig_UpdatedAnchors;
    }

    /// <summary>
    /// This got called after OVR Anchors got all updates from the headset sensors.
    /// </summary>
    /// <param name="rig"></param>
    private void Rig_UpdatedAnchors(OVRCameraRig rig)
    {
        rig.transform.position = prevPos;
    }

    /// <summary>
    /// Message from Unity got called before the camera gets rendered.
    /// </summary>
    void OnPreRender()
    {
        // call right before render as well, to ensure most recent data.
        _Update();
    }
}

