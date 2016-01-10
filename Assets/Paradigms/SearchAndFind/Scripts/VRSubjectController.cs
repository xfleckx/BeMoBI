using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.Assertions;
using Assets.Paradigms.SearchAndFind;
using VRStandardAssets.Utils;
using Assets.Paradigms.SearchAndFind.ImageEffects;
using Assets.BeMoBI.Scripts.Controls;

[RequireComponent(typeof(CharacterController))]
public class VRSubjectController : MonoBehaviour
{

    public CharacterController Body;
    public Transform Head;
    public Camera HeadPerspective;
    public Reticle reticle;

    
    private event Action<CharacterController> ApplyMovementToBody;
    private event Action<Transform> ApplyMovementToHead;
    
    public event Action<float> ItemValueRequested;

    [SerializeField]
    public string HeadController = string.Empty;
    [SerializeField]
    public string BodyController = string.Empty;
    
    public void Start()
    {
        Assert.IsNotNull<Camera>(HeadPerspective);

        Body = GetComponent<CharacterController>();

        ApplyMovementToBody = null;
        ApplyMovementToHead = null;

        var bodyMovementController = GetComponents<IBodyMovementController>();

        Func<IInputController, bool> withTheExpectedName = controller => controller.Identifier.Equals(BodyController);

        if (bodyMovementController.Any(withTheExpectedName))
        {
            ApplyMovementToBody += (bodyMovementController.FirstOrDefault(withTheExpectedName) as IBodyMovementController ).ApplyMovement;
        }

        var headMovementController = GetComponents<IHeadMovementController>();

        withTheExpectedName = controller => controller.Identifier.Equals(HeadController);

        if (headMovementController.Any(withTheExpectedName))
        {
            ApplyMovementToHead += ( headMovementController.FirstOrDefault(withTheExpectedName) as IHeadMovementController ).ApplyMovement;
        }
    }

    void Update()
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

    #region Options

    public void ToggleRectile()
    {
        if (reticle == null)
            return;

        var state = reticle.ReticleTransform.gameObject.activeSelf;

        reticle.ReticleTransform.gameObject.SetActive(!state);
    }

    public void ToggleFog()
    {
        var fog = GetComponentInChildren<CustomGlobalFog>();

        if (fog == null) { 
            Debug.Log("No CustomGlobalFog instance found");
            return;
        }

        fog.enabled = !fog.enabled;

    }

    public void RecenterHeading()
    {
        InputTracking.Recenter();
    }
    
    #endregion

    public void ChangeBodyController(IBodyMovementController bodyController)
    {
        BodyController = bodyController.Identifier;
        ApplyMovementToBody += bodyController.ApplyMovement;
    }

    public void ResetController()
    {
        BodyController = String.Empty;
        ApplyMovementToBody = null;

        HeadController = String.Empty;
        ApplyMovementToHead = null;

        Head.rotation = Quaternion.identity;
        Body.transform.rotation = Quaternion.identity;
    }

    public void ChangeHeadController(IHeadMovementController headController)
    {
        HeadController = headController.Identifier;
        ApplyMovementToHead += headController.ApplyMovement;
    }


} 
