using UnityEngine;
using System.Collections;
using System;

public class ArtefactController : MonoBehaviour {

    public Light activationLight;

    public void ActivateArtefact()
    {
        var anim = GetComponentInChildren<Animation>();
        anim.wrapMode = WrapMode.Loop;
        anim.Play();
    }

    public void DeactivateArtefact()
    {
        var anim = GetComponentInChildren<Animation>();
        anim.Stop();
    }

    void OnTriggerEnter(Collider c)
    {
        if (Approached != null)
            Approached(c);
    }

    public void ActivateLight()
    {
        activationLight.enabled = true;
        ActivateArtefact();
    }

    public void DeactivateLight()
    {
        activationLight.enabled = false;
        DeactivateArtefact();
    }

    public event Action<Collider> Approached;
}
