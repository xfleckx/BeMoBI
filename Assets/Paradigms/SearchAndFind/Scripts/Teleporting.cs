using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public class Teleporting : MonoBehaviour {
    
    public float ExpectedDuration = 2f;

    public GameObject ObjectToTeleport;

    public Transform Target;

    public Transform postTeleportLookAt;

    public TeleportingDurationEvent BeforeTeleporting;

    public TeleportingDurationEvent AfterTeleporting;
    
    public void Teleport()
    {
        if (BeforeTeleporting != null)
            BeforeTeleporting.Invoke(ExpectedDuration);

        StartCoroutine(TeleportingProcess()); 
    }

    IEnumerator TeleportingProcess()
    {
        yield return new WaitForSeconds(ExpectedDuration);
        
        ObjectToTeleport.transform.position = Target.position;

        ObjectToTeleport.transform.LookAt( postTeleportLookAt );
        
        if (AfterTeleporting != null)
            AfterTeleporting.Invoke(ExpectedDuration);

        yield return null;
    }
}

[Serializable]
public class TeleportingDurationEvent : UnityEvent<float>
{
    // does nothing
}
