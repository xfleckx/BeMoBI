
using System;
using System.Collections;

using UnityEngine;

public class StartPoint : MonoBehaviour {

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

}
