
using System;
using System.Collections;

using UnityEngine;

public class StartPoint : MonoBehaviour {

    public void ClearSubscriptions()
    {
        EnterStartPoint = null;
        LeaveStartPoint = null;
    }

    public event Action<Collider> EnterStartPoint;
    public event Action<Collider> LeaveStartPoint;

    void OnTriggerEnter(Collider c)
    {
        if (EnterStartPoint != null)
            EnterStartPoint(c);
    }

    void OnTriggerExit(Collider c)
    {
        if (LeaveStartPoint != null)
            LeaveStartPoint(c);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, transform.forward * 3);
    }
}
