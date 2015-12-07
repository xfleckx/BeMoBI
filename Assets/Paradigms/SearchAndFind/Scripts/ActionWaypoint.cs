using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider))]
public class ActionWaypoint : MonoBehaviour
{
    public int WaypointId = 0;

    public Transform InfoText;

    public Transform InfoTextLookAtTarget;

    public IdEvent Entered;

    public IdEvent Leaved;
    
    void Update() {

        if (InfoText != null && InfoTextLookAtTarget != null)
            InfoText.LookAt(InfoTextLookAtTarget);
    }

    public void OnTriggerEnter(Collider other)
    {
        var subject = other.GetComponent<VRSubjectController>();

        if (subject != null)
            Entered.Invoke(WaypointId);
    }

    public void OnTriggerExit(Collider other)
    {
        var subject = other.GetComponent<VRSubjectController>();

        if (subject != null)
            Leaved.Invoke(WaypointId);
    }
}

[Serializable]
public class IdEvent : UnityEvent<int> {}