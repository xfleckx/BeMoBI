using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public class Teleporting : MonoBehaviour {
    
    public float ExpectedDuration = 2f;

    public GameObject ObjectToTeleport;

    public Transform Target;
    
    public TeleportingDurationEvent BeforeTeleporting;

    public TeleportingDurationEvent AfterTeleporting;
    public void Teleport()
    {
        if (BeforeTeleporting.GetPersistentEventCount() > 0)
            BeforeTeleporting.Invoke(ExpectedDuration);

        StartCoroutine(TeleportingProcess()); 
    }

    IEnumerator TeleportingProcess()
    {
        yield return new WaitForSeconds(ExpectedDuration);

        yield return new WaitForEndOfFrame();

        ObjectToTeleport.transform.position = Target.position;
        
        if (AfterTeleporting.GetPersistentEventCount() > 0)
            AfterTeleporting.Invoke(ExpectedDuration);

        yield return null;
    }
    
}

[Serializable]
public class TeleportingDurationEvent : UnityEvent<float>
{
    // does nothing
}
