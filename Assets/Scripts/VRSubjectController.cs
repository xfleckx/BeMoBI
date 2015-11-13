using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.Assertions;

public class VRSubjectController : MonoBehaviour
{
    public Camera HeadPerspective;

    private event Action ProcessInput;

    public event Action SubmitPressed;
    public event Action<float> ItemValueRequested; 

    public void Start()
    {
        Assert.IsNotNull<Camera>(HeadPerspective);
        m_CharacterController = GetComponent<CharacterController>();
    }

    public void RecenterHeading()
    {
        InputTracking.Recenter();
    }

    public void FixedUpdate()
    {
        if (ProcessInput != null)
            ProcessInput.Invoke();
    }

    public void SetInputMethod(Action initMethod, Action continousMethod)
    {
        initMethod.Invoke();

        ProcessInput = null;
        ProcessInput += continousMethod;
    }

    public void SetInitializeMethodsForMultipleInstance(params Action[] initMethods) {
        foreach (var m in initMethods)
        {
            m.Invoke();
        }
    }

    public void SetMultipleInputMethods(params Action[] continousMethods)
    {
        ProcessInput = null;
        foreach (var m in continousMethods)
        {
            ProcessInput += m;
        }
    }

    #region Rift only

    public void RiftOnlyInput()
    {
        HeadPerspective.transform.localRotation = InputTracking.GetLocalRotation(VRNode.Head);
        
        if(!useTestHeight)
            HeadPerspective.transform.localPosition = InputTracking.GetLocalPosition(VRNode.Head);
    }

    #endregion

    #region XBox Ctrl

    public void XBoxActions()
    {
        if (Input.GetButton("ItemSubmit") && SubmitPressed != null)
            SubmitPressed.Invoke();

        if (ItemValueRequested != null)
            ItemValueRequested(Input.GetAxis("ItemValue"));
    }

    #endregion

    #region Keyboard Rift Input

    public void KeyboardRiftInput()
    {
        HeadPerspective.transform.localRotation = InputTracking.GetLocalRotation(VRNode.Head);

        if(!useTestHeight)
            HeadPerspective.transform.localPosition = InputTracking.GetLocalPosition(VRNode.Head);

        KeyboardMovement();
    }

    #endregion

    #region Mouse Keyboard Input

    public float XSensitivity = 2f;
    public float YSensitivity = 2f;
    public bool clampVerticalRotation = true;
    public float MinimumX = -90F;
    public float MaximumX = 90F;
    public bool smooth;
    public float smoothTime = 5f;
    
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_IsWalking;
    public  float m_WalkSpeed;
    public float m_RunSpeed;

    private Quaternion m_CharacterTargetRot;
    private Quaternion m_CameraTargetRot;

    public bool useTestHeight = true;

    public void InitMouseKeyboardInput()
    {
        m_CharacterTargetRot = transform.rotation;
        m_CameraTargetRot = HeadPerspective.transform.localRotation;
    }

    public void MouseKeyboardInput()
    {
        LookRotation();
        KeyboardMovement();
    }
    
    private void LookRotation()
    {
        float yRot = Input.GetAxis("Mouse X") * XSensitivity;
        float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

        m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
        m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

        if (clampVerticalRotation)
            m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

        if (smooth)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, m_CharacterTargetRot,
                smoothTime * Time.deltaTime);
            HeadPerspective.transform.localRotation = Quaternion.Slerp(HeadPerspective.transform.localRotation, m_CameraTargetRot,
                smoothTime * Time.deltaTime);
        }
        else
        {
            transform.rotation = m_CharacterTargetRot;
            HeadPerspective.transform.localRotation = m_CameraTargetRot;
        }
    }

    private void KeyboardMovement()
    {
        float speed;
        GetInput(out speed);

        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                            m_CharacterController.height / 2f);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;

        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

    }

    private void GetInput(out float speed)
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
        // On standalone builds, walk/run speed is modified by a key press.
        // keep track of whether or not the character is walking or running
        m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
        // set the desired speed to be walking or running
        speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
        m_Input = new Vector2(h, v);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        if (m_IsWalking != waswalking && m_CharacterController.velocity.sqrMagnitude > 0)
        {
            StopAllCoroutines();
        }
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    #endregion

    #region Joystick Input

    public void InitJoystick()
    {

    }

    public void JoystickInput()
    {

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
