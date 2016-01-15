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
    
    public event Action<float> ItemValueRequested;

    [SerializeField]
    public string HeadController;
    [SerializeField]
    public string BodyController;

    public void Start()
    {
        Assert.IsNotNull<Camera>(HeadPerspective);

        Body = GetComponent<CharacterController>(); 
    }
    
    public void EnableSubjectBehaviorControl()
    {
        Change<IHeadMovementController>(HeadController);
        Change<IBodyMovementController>(BodyController);
    }
    
    private void Enable<C>(string ControllerName) where C : IInputController
    {
        var possibleController = GetComponents<C>();

        Func<C, bool> withTheExpectedName = controller => controller.Identifier.Equals(ControllerName);

        if (possibleController.Any(withTheExpectedName))
        {
            possibleController.FirstOrDefault(withTheExpectedName).Enable();
        }
        
    }

    public void Change<C>(string controllerName) where C : IInputController
    {
        DisableAll<C>();
        Enable<C>(controllerName);
    }

    private void DisableAll<C>() where C : IInputController
    {
        var controller = GetComponents<C>();

        foreach (var c in controller)
        {
            c.Disable();
        }

    }
     
    public void ResetController()
    {
        BodyController = String.Empty; 
        HeadController = String.Empty;

        DisableAll<IInputController>();

        Head.rotation = Quaternion.identity;
        Body.transform.rotation = Quaternion.identity;
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

        if (fog == null)
        {
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
    
    void OnDrawGizmos()
    {
        var bodyCenter = transform.position + new Vector3(0, 1, 0);

        Gizmos.DrawWireCube(transform.position, new Vector3(0.4f, 0.001f, 0.4f));
        Gizmos.DrawRay(bodyCenter, transform.forward * 0.5f);
        Gizmos.DrawCube(bodyCenter, new Vector3(0.1f, 0.4f, 0.1f));

    }
}