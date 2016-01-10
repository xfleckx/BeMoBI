using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using UnityEngine.VR;

namespace Assets.BeMoBI.Scripts.Controls
{
    public class OculusRiftController : MonoBehaviour, IHeadMovementController
    {
        public OVRManager ovrManager;

        public OVRCameraRig ovrRig;
        
        public string Identifier
        {
            get
            {
               return "Oculus Headset";
            }
        }

        private bool monoscopicRendering = false;

        public void Recenter()
        {
            InputTracking.Recenter();
        }

        public bool UseMonoscopigRendering {

            get {
                return monoscopicRendering;
            }
            set {

                if (value != monoscopicRendering) {
                    monoscopicRendering = value;
                    OnMonoscopicRenderingChanged();
                }
            }
        }

        private void OnMonoscopicRenderingChanged()
        {
            ovrManager.monoscopic = true;
        }

        void Awake()
        {
            ovrManager = OVRManager.instance;
            ovrRig = gameObject.GetComponentInChildren<OVRCameraRig>();
        }

        void Start()
        {
            ovrRig.enabled = true;
            ovrManager.enabled = true;
        }

        void OnEnable()
        {
            ovrRig.enabled = true;
            ovrManager.enabled = true;
        }

        public void ApplyMovement(Transform head)
        {
            // Actual Controlled by the Oculus Components!
        }

        void OnDisable()
        {
            ovrRig.enabled = true;
            ovrManager.enabled = true;
        }
    }
}
