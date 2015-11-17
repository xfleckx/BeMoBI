using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(VRSubjectController))]
public class MouseController : MonoBehaviour, IHeadMovementController
{
    private CharacterController body;

    private Quaternion m_CharacterTargetRot = Quaternion.identity;
    private Quaternion m_CameraTargetRot = Quaternion.identity;

    public float X_Sensitivity = 2f;
    public float Y_Sensitivity = 2f;

    public bool clampVerticalRotation = true;
    public float MinimumX = -90F;
    public float MaximumX = 90F;

    public bool smooth;
    public float smoothTime = 5f;

    public string Identifier
    {
        get
        {
            return "Mouse Head Controller";
        }
    }

    // Use this for initialization
    void Start()
    {
        var subject = GetComponent<VRSubjectController>();
        body = subject.GetComponent<CharacterController>();
        m_CharacterTargetRot = body.transform.rotation;

    }

    public void ApplyMovement(Transform head)
    {
        float yRot = Input.GetAxis("Mouse X") * X_Sensitivity;
        float xRot = Input.GetAxis("Mouse Y") * Y_Sensitivity;
        
        m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
        m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

        if (clampVerticalRotation)
            m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

        if (smooth)
        {
            body.transform.rotation = Quaternion.Slerp(body.transform.rotation, m_CharacterTargetRot,
                smoothTime * Time.deltaTime);
            head.localRotation = Quaternion.Slerp(head.localRotation, m_CameraTargetRot,
                smoothTime * Time.deltaTime);
        }
        else
        {
            body.transform.rotation = m_CharacterTargetRot;
            head.localRotation = m_CameraTargetRot;
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

}
