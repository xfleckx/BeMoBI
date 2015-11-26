using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Paradigms.MultiMazePathRetrieval.Scripts.Controls
{
    public class OculusRift : MonoBehaviour, IHeadMovementController
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

        void Awake()
        {
            ovrManager = gameObject.GetComponentInChildren<OVRManager>();
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
