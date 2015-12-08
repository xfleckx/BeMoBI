using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.Assertions;
using Assets.Paradigms.SearchAndFind;

[RequireComponent(typeof(CharacterController))]
public class VRSubjectController : MonoBehaviour
{
    public Camera HeadPerspective;

    public Transform Head;
    public CharacterController Body;
    
    private event Action<CharacterController> ApplyMovementToBody;
    private event Action<Transform> ApplyMovementToHead;
    
    public event Action<float> ItemValueRequested;

    [SerializeField]
    public string CurrentHeadController = string.Empty;
    [SerializeField]
    public string CurrentBodyController = string.Empty;
    
    public void Start()
    {
        Assert.IsNotNull<Camera>(HeadPerspective);

        Body = GetComponent<CharacterController>();

        var bodyMovementController = GetComponents<IBodyMovementController>();

        ApplyMovementToBody = null;
        ApplyMovementToHead = null;

        if (bodyMovementController.Any())
        {
            ApplyMovementToBody += bodyMovementController.FirstOrDefault(
                controller => controller.Identifier.Equals(CurrentBodyController)
                ).ApplyMovement;
        }

        var headMovementController = GetComponents<IHeadMovementController>();

        if (headMovementController.Any())
        {
            ApplyMovementToHead += headMovementController.FirstOrDefault(
                controller => controller.Identifier.Equals(CurrentHeadController)
                ).ApplyMovement;
        }
    }

    public void ChangeBodyController(IBodyMovementController bodyController)
    {
        CurrentBodyController = bodyController.Identifier;
        ApplyMovementToBody += bodyController.ApplyMovement;
    }

    public void ResetController()
    {
        CurrentBodyController = String.Empty;
        ApplyMovementToBody = null;

        CurrentHeadController = String.Empty;
        ApplyMovementToHead = null;
    }
    
    public void SetMonoscopic(bool useMonoscopigRendering)
    {
        OVRManager.instance.monoscopic = useMonoscopigRendering;
    }

    public void ChangeHeadController(IHeadMovementController headController)
    {
        CurrentHeadController = headController.Identifier;
        ApplyMovementToHead += headController.ApplyMovement;
    }

    public void RecenterHeading()
    {
        InputTracking.Recenter();
    }

    public void FixedUpdate()
    {   
        if (ApplyMovementToBody != null)
            ApplyMovementToBody(Body);

        if (ApplyMovementToHead != null)
            ApplyMovementToHead(Head);
    }

    void OnDrawGizmos()
    {
        var bodyCenter = transform.position + new Vector3(0, 1, 0);

        Gizmos.DrawWireCube(transform.position, new Vector3(0.4f, 0.001f, 0.4f));
        Gizmos.DrawRay(bodyCenter, transform.forward * 0.5f);
        Gizmos.DrawCube(bodyCenter, new Vector3(0.1f, 0.4f, 0.1f));
        
    }
} 
