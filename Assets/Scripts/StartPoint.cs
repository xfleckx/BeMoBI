
using System;
using System.Collections;

using UnityEngine;

public class StartPoint : MonoBehaviour {

    public event Action<GameObject> EnterStartPoint;
    public event Action<GameObject> LeaveStartPoint;

    void OnTriggerEnter(Collider c)
    {
        if (EnterStartPoint != null)
            EnterStartPoint(c.gameObject);
    }

    void OnTriggerExit(Collider c)
    {
        if (LeaveStartPoint != null)
            LeaveStartPoint(c.gameObject);
    }

}
