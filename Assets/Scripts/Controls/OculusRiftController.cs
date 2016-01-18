﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using UnityEngine.VR;

namespace Assets.BeMoBI.Scripts.Controls
{
    public class OculusRiftController : MonoBehaviour, IHeadMovementController
    {
        [SerializeField]
        private Transform head;
        public Transform Head
        {
            get
            {
                return head;
            }

            set
            {
                head = value;
            }
        }

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
        private float originalIpd = 0;

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

        public float IPD {
            get { return OVRPlugin.ipd; }
        }
        
        public void ChangeIPDValue(float value)
        {
            originalIpd = OVRPlugin.ipd;

            OVRPlugin.ipd = value;
        }

        public void RestoreOriginalIpd()
        {
            OVRPlugin.ipd = originalIpd;
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
         
        void OnDisable()
        {
            ovrRig.enabled = false;
            ovrManager.enabled = false;
        }

        public void Enable()
        {
            enabled = true;
        }

        public void Disable()
        {
            enabled = false;
        }
    }
}