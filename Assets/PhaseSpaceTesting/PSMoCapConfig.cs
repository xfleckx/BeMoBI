using UnityEngine;
using System.Collections;

public class PSMoCapConfig : ScriptableObject {
    
    public Vector3 trackingSpaceCorrection = Vector3.zero;

    public string description;

    public int Mode;

    public int lastRigidID = 0;
    public string lastRBFile;
}
