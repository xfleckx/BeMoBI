using UnityEngine;
using System.Collections;

namespace Assets.BeMoBI.Scripts.Controls
{
    public class GamepadHeadAndBodyController : MonoBehaviour, ICombinedControl
    {

        private const string X_AXIS_BODY_NAME = "XBX_Horizontal_Body";
        private const string Y_AXIS_BODY_NAME = "XBX_Vertical_Body";

        private const string X_AXIS_HEAD_NAME = "XBX_Horizontal_Head_X";
        private const string Y_AXIS_HEAD_NAME = "XBX_Vertical_Head_Y";

        [Range(0.1f, 2)]
        public float MaxWalkingSpeed = 1.4f;

        public AnimationCurve accelerationCurve;

        public Transform customHeadReference;

        [Range(10, 50)]
        public float RotationSpeed = 40f;

        public bool RotateSmooth = true;
        public float SmoothTime = 1;

        [HideInInspector]
        public Quaternion targetRotation;

        public string Identifier { get { return "GamePad"; } }

        [HideInInspector]
        public float body_raw_X = 0f;
        [HideInInspector]
        public float body_raw_Y = 0f;

        [HideInInspector]
        public float head_raw_X = 0f;
        [HideInInspector]
        public float head_raw_Y = 0f;

        public bool smooth;
        public float smoothTime = 5f;

        [HideInInspector]
        public Quaternion headTargetRotation = Quaternion.identity;
        
        [HideInInspector]
        public Quaternion sourceLocalRotation = Quaternion.identity;

        [HideInInspector]
        public Quaternion sourceRotation = Quaternion.identity;

        public float X_Sensitivity = 2f;
        public float Y_Sensitivity = 2f;

        public bool clampVerticalRotation = true;
        public bool clampHorizontalRotation = true;

        public float MinimumX = -90F;
        public float MaximumX = 90F;
        

        public void ApplyMovement(Transform head)
        {
            Transform headToUse = null;

            if (customHeadReference != null)
                headToUse = customHeadReference;
            else
                headToUse = head;
            
            head_raw_X = Input.GetAxis(X_AXIS_HEAD_NAME);
            head_raw_Y = Input.GetAxis(Y_AXIS_HEAD_NAME);

            float xRot = head_raw_X * X_Sensitivity;
            float yRot = head_raw_Y * Y_Sensitivity;

            headTargetRotation *= Quaternion.Euler(yRot, xRot, 0f);

            if (clampVerticalRotation)
                headTargetRotation = InputUtils.ClampRotationAroundXAxis(headTargetRotation, MinimumX, MaximumX);

            if (clampHorizontalRotation)
                headTargetRotation = InputUtils.ClampRotationAroundYAxis(headTargetRotation, MinimumX, MaximumX);

            sourceRotation = headToUse.rotation;
            sourceLocalRotation = headToUse.localRotation;

            if (smooth)
            {
                headToUse.localRotation *= Quaternion.Slerp(sourceLocalRotation, headTargetRotation,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                headToUse.rotation *= headTargetRotation;
            }
        }

        public void ApplyMovement(CharacterController character)
        {
            targetRotation = character.transform.rotation;

            body_raw_X = Input.GetAxis(X_AXIS_BODY_NAME); // use as body rotation

            body_raw_Y = Input.GetAxis(Y_AXIS_BODY_NAME); // use as movement to forward / backwards

            Vector3 desiredMove = character.transform.forward * accelerationCurve.Evaluate(body_raw_Y) * Time.deltaTime * MaxWalkingSpeed;

            targetRotation *= Quaternion.Euler(0f, body_raw_X * RotationSpeed, 0f);

            if (RotateSmooth)
            {
                character.transform.rotation = Quaternion.Slerp(character.transform.rotation, targetRotation, SmoothTime * Time.deltaTime);
            }
            else {
                character.transform.rotation = targetRotation;
            }

            character.Move(desiredMove);
        }

    }
}