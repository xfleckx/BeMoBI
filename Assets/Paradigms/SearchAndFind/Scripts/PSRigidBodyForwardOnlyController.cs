using UnityEngine;
using System.Collections;
using PhaseSpace;

using System;
using System.Linq;
using Assets.BeMoBI.Scripts.PhaseSpaceExtensions;
using Assets.BeMoBI.Paradigms.SearchAndFind;

namespace Assets.BeMoBI.Scripts.Controls
{
    public class PSRigidBodyForwardOnlyController : MonoBehaviour, IBodyMovementController
    {
        string VERTICAL = "FW_Vertical";

        public float ForwardSpeed = 1;

        OWLTracker tracker;

        public int expectedRigidID;

        public int TimeOffsetTilRBcreation = 1;

        public int AvailableRigidBodies {
            get { return tracker.NumRigids; }
        }
        /// <summary>
        /// The number of frames to interpolate the rigid body over.  Values over 8 are clamped.
        /// </summary>
        public int Interp = 0;

        /// <summary>
        /// The most recent rigid acquired from Tracker
        /// </summary>
        public PhaseSpace.Rigid lastRigid = new PhaseSpace.Rigid();


        protected Vector3 prevPos = new Vector3(0, 0, 0);
        protected Quaternion prevRot = new Quaternion(0, 0, 0, 1);

        // interpolation
        protected int interp_index = 0;
        protected Quaternion[] quaternions = new Quaternion[8];
        protected Vector3[] rotations = new Vector3[8];

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

            if (firstServer != null) {

                var result = tracker.Connect(firstServer.address, false, false);

                if(result == false)
                {
                    Debug.Log("Establishing connection failed...");
                }
            }
            this.enabled = true;

            StartCoroutine(WaitSecondsBeforeCreateRigidbody());

        }

        private void TryCreateRigidBodyTracker()
        {
            Debug.Log("Try create rigid body");

            var fileProvider = FindObjectOfType<ParadigmController>();

            var fileInfo = fileProvider.GetRigidBodyDefinition();

            if(fileInfo == null)
            {
                Debug.Log("Missing RigidBodyFile!");
                this.enabled = false;
                
            }

            tracker.CreateRigidTracker(expectedRigidID, fileInfo.FullName);
        }
        
        IEnumerator WaitSecondsBeforeCreateRigidbody()
        {
            yield return new WaitForSeconds(TimeOffsetTilRBcreation);
            TryCreateRigidBodyTracker();
            tracker.StartStreaming();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateFromTracker();
            
            var y_rotation = prevRot.eulerAngles.y;

            Body.transform.rotation = Quaternion.Euler(0, y_rotation, 0);
            
                                  // forward only - only take positive value ignoring left or right button!
            var forwardMovement = Math.Abs(Input.GetAxis(VERTICAL));

            var movementVector = Body.transform.forward * forwardMovement * ForwardSpeed * Time.deltaTime;

            Body.Move(movementVector);

        }

        protected void UpdateFromTracker()
        {
            // get data from tracker
            lastRigid = tracker.GetRigid(expectedRigidID);

            if (lastRigid == null || lastRigid.cond < 0)
                return;

            prevRot = lastRigid.rotation;
            prevPos = lastRigid.position;

            // interpolate if required
            if (Interp > 1)
            {
                quaternions[interp_index] = prevRot;
                rotations[interp_index] = prevPos;

                int n = Math.Min(Interp, quaternions.Length);
                float f = 1.0f / n;
                Quaternion q = quaternions[interp_index];
                Vector3 r = rotations[interp_index];
                for (int i = 1; i < n; i++)
                {
                    int index = ((interp_index - i) + quaternions.Length) % quaternions.Length;
                    q = Quaternion.Slerp(q, quaternions[index], f);
                    r += rotations[index];
                }
                r *= f;

                prevRot = q;
                prevPos = r;

                interp_index = (interp_index + 1) % quaternions.Length;
            }
        }
        
    }
}