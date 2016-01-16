using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.BeMoBI.Scripts.Controls
{
    public class KeyboardCombined : CombinedController
    {
        private const string X_AXIS_NAME = "KBC_Horizontal";
        private const string Y_AXIS_NAME = "KBC_Vertical";

        public override string Identifier
        {
            get
            {
                return "KeyboardCombi";
            }
        }

        void OnEnable()
        {
            targetRotation = Quaternion.identity;
        }


        public AnimationCurve BodyRotationAccelerationCurve;

        void Update()
        {
            body_raw_X = Input.GetAxis(X_AXIS_NAME);

            var sign = Math.Sign(body_raw_X);
            
            var abs_body_raw_X = Math.Abs(body_raw_X);
            
            if (abs_body_raw_X > 0 && !isBodyRotating)
            {
                var args = new RotationEventArgs();
                args.state = RotationEventArgs.State.Begin;
                OnBodyRotation.Invoke(args);
                isBodyRotating = true;
            }

            if (abs_body_raw_X == 0 && isBodyRotating)
            {
                var args = new RotationEventArgs();
                args.state = RotationEventArgs.State.End;
                OnBodyRotation.Invoke(args);
                isBodyRotating = false;
            }
            
            body_raw_Y = Input.GetAxis(Y_AXIS_NAME);

            desiredMove = Body.transform.forward * BodyAccelerationCurve.Evaluate(body_raw_Y) * Time.deltaTime * MaxWalkingSpeed;

            // Problem here... Acceleration for rotation doesn't work :/ always the same direction

            var evaluated = BodyRotationAccelerationCurve.Evaluate(abs_body_raw_X);

            var bodyRotation = evaluated * sign;

            targetRotation *= Quaternion.Euler(0f, bodyRotation * BodyRotationSpeed, 0f);

            if (RotateBodySmooth)
            {
                Body.transform.rotation = Quaternion.Slerp(Body.transform.rotation, targetRotation, SmoothBodyTime * Time.deltaTime);
            }
            else {
                Body.transform.rotation = targetRotation;
            }

            Body.Move(desiredMove);
        }
    }
}
