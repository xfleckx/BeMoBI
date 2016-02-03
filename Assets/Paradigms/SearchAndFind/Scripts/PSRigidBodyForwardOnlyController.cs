using UnityEngine;
using System.Collections;
using PhaseSpace;

using System;
using System.Linq;

namespace Assets.BeMoBI.Scripts.Controls
{
    public class PSRigidBodyForwardOnlyController : MonoBehaviour, IBodyMovementController
    {
        string VERTICAL = "FW_Vertical";

        public float ForwardSpeed = 1;

        OWLTracker tracker;

        public int expectedRigidID;
        
        private CharacterController body;
        public CharacterController Body
        {
            get
            {
                return body;
            }

            set
            {
                body = value;
            }
        }

        public string Identifier
        {
            get
            {
                return "PS_RigidBody";
            }
        }

        public void Disable()
        {
            if(tracker != null)
               tracker.Disconnect();
        }

        public void Enable()
        {
            TryConnectToTracker();
        }

        private void TryConnectToTracker()
        {
            tracker = OWLTracker.Instance;

            var availableServers = tracker.GetServers();

            if (tracker == null) {
                Debug.Log("No tracker found!");
                this.enabled = false;
                return;
            }

            var firstServer = availableServers.FirstOrDefault();

            if (firstServer != null)
                tracker.Connect(firstServer.address, false, false);

            this.enabled = true;
        }

        // Update is called once per frame
        void Update()
        {

            var rb = tracker.GetRigid(expectedRigidID);

            if (rb != null)
            { 
                var y_rotation = rb.rotation.eulerAngles.y;

                Body.transform.rotation = Quaternion.Euler(0, y_rotation, 0);
            }

                                  // forward only - only take positive value ignoring left or right button!
            var forwardMovement = Math.Abs(Input.GetAxis(VERTICAL));

            var movementVector = Body.transform.forward * forwardMovement * ForwardSpeed * Time.deltaTime;

            Body.Move(movementVector);

        }
    }
}