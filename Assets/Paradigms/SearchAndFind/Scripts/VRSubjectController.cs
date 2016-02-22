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

public class VRSubjectController : MonoBehaviour
{
    public CharacterController Body;
    public Transform Head;
    public Camera HeadPerspective;
    public Reticle reticle;
    public HUD_DEBUG debug_hud;
    public HUD_Instruction instruction_hud;

    public float feetToHead = 1.76f;

    public event Action<float> ItemValueRequested;

    [SerializeField]
    public string HeadController;
    [SerializeField]
    public string BodyController;

    public void Start()
    {
       Assert.IsNotNull<Camera>(HeadPerspective);
       //Body = GetComponentInChildren<CharacterController>();
       AdjustSubjectProperties();
    }
    
    public void AdjustSubjectProperties()
    {
        Body.height = feetToHead;
        Body.center = new Vector3(0, feetToHead / 2, 0);
        instruction_hud.transform.localPosition = new Vector3(0, feetToHead, 0);

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

        var controller = Get<C>(controllerName);

        if (controller == null)
            Debug.Log(string.Format("Expected {0} not found!", controllerName));

        if(controller is IBodyMovementController)
        {
            var bodyController = controller as IBodyMovementController;

            if (bodyController == null)
                Debug.Log(string.Format("Expected {0} not found as BodyMovementController!", controllerName));

            bodyController.Body = this.Body;
        }

        if(controller is IHeadMovementController)
        {
            var headController = controller as IHeadMovementController;

            if (headController == null)
                Debug.Log(string.Format("Expected {0} not found as HeadMovementController!", controllerName));

            headController.Head = this.Head;
        }
        
    }
    
    public IInputController Get<C>(string ControllerName) where C : IInputController
    {
        var possibleController = GetComponents<C>();

        IInputController expectedController = null;

        Func<C, bool> withTheExpectedName = controller => controller.Identifier.Equals(ControllerName);

        if (possibleController.Any(withTheExpectedName))
        {
            expectedController = possibleController.FirstOrDefault(withTheExpectedName);
        } 

        return expectedController;
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
        var bodyCenter = Body.transform.localPosition + new Vector3(0, 1, 0);

        Gizmos.DrawWireCube(Body.transform.localPosition, new Vector3(0.4f, 0.001f, 0.4f));
        Gizmos.DrawRay(bodyCenter, Body.transform.forward * 0.5f);
        Gizmos.DrawCube(bodyCenter, new Vector3(0.1f, 0.4f, 0.1f));

    }

    public void Move(Vector3 movementVector)
    {
        Body.Move(movementVector);
        UpdateHeadPosition();
    }

    public void SetPosition(Transform target)
    {
        Body.transform.position = target.position;
        UpdateHeadPosition();
    }

    public void Rotate(Quaternion resultRotation)
    {
        Body.transform.rotation = resultRotation;
        Head.transform.rotation = resultRotation;
    }

    private void UpdateHeadPosition()
    {
        Head.transform.position = new Vector3(Body.transform.position.x, feetToHead, Body.transform.position.z);
    }
}
