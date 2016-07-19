﻿using UnityEngine;
using System.Collections;
using PhaseSpace;

using System;
using System.Linq;
using Assets.BeMoBI.Scripts.PhaseSpaceExtensions;
using Assets.BeMoBI.Paradigms.SearchAndFind;
using UnityEngine.Assertions;

namespace Assets.BeMoBI.Scripts.Controls
{
    public class PSRigidBodyForwardOnlyController : MonoBehaviour, IBodyMovementController, IInputCanCalibrate
    {
        public Transform Head;

        public VRSubjectController subject;

        NLog.Logger appLog = NLog.LogManager.GetLogger("App");

        // string VERTICAL = "FW_Vertical"; // not in use

        string VERTICAL_WiiMote = "FW_WiiM_Vertical";
         
        public float ForwardSpeed = 1;

        OWLTracker tracker;

        public int expectedRigidID;

        public int TimeOffsetTilRBcreation = 1;

        public int AvailableRigidBodies
        {
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

        private OVRCameraRig rig;

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

        [Range(-360, 360)]
        public float yawCorrectionOffset = 0;

        void Awake()
        {
            subject = GetComponent<VRSubjectController>();
            rig = FindObjectOfType<OVRCameraRig>();
            
            Assert.IsNotNull(rig);

            body = subject.GetComponent<CharacterController>();

        }
        
        private void Rig_UpdatedAnchors(OVRCameraRig obj)
        {
            OVRPose pose = rig.centerEyeAnchor.ToOVRPose(true);
            
            pose.orientation = Quaternion.Euler(0, -body.transform.rotation.eulerAngles.y, 0) * pose.Inverse().orientation * pose.orientation;
            
            rig.trackingSpace.FromOVRPose(pose, true);
        }
        
        public void Disable()
        {
            this.enabled = false;


            rig.UpdatedAnchors -= Rig_UpdatedAnchors;
        }

        private void CloseTrackerConnection()
        {
            if (tracker != null && tracker.Connected())
            {
                tracker.Disconnect();
            } 
        }

        public void Enable()
        {
            this.enabled = true;
            TryConnectToTracker();

            rig.UpdatedAnchors += Rig_UpdatedAnchors;
        }

        private void TryConnectToTracker()
        {
            tracker = OWLTracker.Instance;

            var availableServers = tracker.GetServers();

            if (tracker == null)
            {
                var msg = "No phasespace (owl) server found!";

                appLog.Error(msg);

                Debug.Log(msg);

                this.enabled = false;
                return;
            }

            appLog.Error("Automatic use the first OWL server.");

            var firstServer = availableServers.FirstOrDefault();

            if (firstServer != null)
            {
                var connectionAttemptMessage = string.Format("Try connection to OWL with address {0}", firstServer.address);

                appLog.Error(connectionAttemptMessage);
                Debug.Log(connectionAttemptMessage);

                var result = tracker.Connect(firstServer.address, false, false);

                if (result == false)
                {
                    var connectionFailedMessage = string.Format("Establishing connection to OWL with address {0} failed...", firstServer.address);
                    appLog.Error(connectionFailedMessage);
                    Debug.Log(connectionFailedMessage);
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

            if (fileInfo == null)
            {
                var missingFileMessage = string.Format("An expected rigidbody file '{0}' for configuring the phasespace server could not be found!", fileProvider.Config.nameOfRigidBodyDefinition);

                appLog.Fatal(missingFileMessage);

                Debug.LogError(missingFileMessage);

                this.enabled = false;

            }

            try
            {
                if (tracker != null) {
                    var creationMessage = string.Format("Try create rigidbody with ID: {0} from file: {1}", expectedRigidID, fileInfo.Name);
                    appLog.Info(creationMessage);
                    Debug.Log(creationMessage);

                    tracker.CreateRigidTracker(expectedRigidID, fileInfo.FullName);
                }
            }
            catch (OWLException owle)
            {   
                appLog.Fatal(owle.Message);

                this.enabled = false;
            }

        }

        IEnumerator WaitSecondsBeforeCreateRigidbody()
        {
            var info = string.Format("Wait {0} seconds before creating the rigidbody tracker", TimeOffsetTilRBcreation);

            appLog.Info(info);
            Debug.Log(info);

            yield return new WaitForSeconds(TimeOffsetTilRBcreation);

            TryCreateRigidBodyTracker();

            appLog.Info("Rigidbody initialized... Send 'Start Streaming' Message to OWL!");

            tracker.StartStreaming();
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

        // Update is called once per frame
        void Update()
        {
            UpdateFromTracker();

            var y_rotation = prevRot.eulerAngles.y + yawCorrectionOffset;

            //eliminate the pitch and roll rotation - for this use case only!
            Body.transform.rotation = Quaternion.Euler(0, y_rotation, 0);
            
            // forward only - only take positive values and ignoring left or right button!
            var forwardMovement = Math.Abs(Input.GetAxis(VERTICAL_WiiMote));

            var movementVector = Body.transform.forward * forwardMovement * ForwardSpeed * Time.deltaTime;

            //only move on a (x,z) plane
            movementVector = new Vector3(movementVector.x, 0, movementVector.z);

            subject.Move(movementVector);
        }

        public void OnDestroy()
        {
            CloseTrackerConnection();
        }

        public void OnDisable()
        {
            CloseTrackerConnection();
        }

        public void Calibrate()
        {
            var delta = Quaternion.FromToRotation(subject.Head.forward, Body.transform.forward);

            appLog.Info(string.Format("Applying '{0.00}'° as offset on calibration.", delta));

            yawCorrectionOffset += delta.eulerAngles.y;

            appLog.Info(string.Format("Constant Yaw correction is now: '{0.00}'°", yawCorrectionOffset));
        }
    }
}