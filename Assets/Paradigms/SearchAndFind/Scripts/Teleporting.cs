using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Teleporting : MonoBehaviour {

    public GameObject ObjectToTeleport;

    public Transform Target;

    public UnityEvent BeforeTeleporting;

    public UnityEvent AfterTeleporting;
    
    public void Teleport()
    {
        if (BeforeTeleporting != null)
            BeforeTeleporting.Invoke();

        ObjectToTeleport.transform.position = Target.position;

        if (AfterTeleporting != null)
            AfterTeleporting.Invoke();
    }
}
