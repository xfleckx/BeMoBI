using UnityEngine;
using System.Collections;
using System;


namespace Assets.BeMoBI.Scripts.Controls
{

    [RequireComponent(typeof(VRSubjectController))]
    public class KeyboardController : MonoBehaviour, IBodyMovementController
    {
        private Transform heading;

        void Start()
        {
            var subject = GetComponent<VRSubjectController>();
            heading = subject.Head;
        }

        public string Identifier
        {
            get
            {
                return "Keyboard";
            }
        }

        public float speed = 2.0f;

        public void ApplyMovement(CharacterController controller)
        {
            var inputVector = Vector3.zero;

            if (Input.GetKey(KeyCode.RightArrow))
            {
                inputVector += Vector3.right * speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                inputVector += Vector3.left * speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                inputVector += Vector3.forward * speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                inputVector += Vector3.back * speed * Time.deltaTime;
            }

            var m_Input = new Vector2(inputVector.x, inputVector.z);

            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

            heading.transform.TransformDirection(desiredMove);

            controller.Move(desiredMove);
        }

    }
}