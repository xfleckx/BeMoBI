using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.BeMoBI.Scripts.PhaseSpaceExtensions
{
    public class PhaseSpaceConfig : ScriptableObject
    {
        public string OWLHost;

        public bool isSlave = false;

        public bool broadcast = false;

        public bool autoConnectOnStart = false;

        public bool CreateDefaultPointTracker = false;
        
        public OWLUpdateStratgy updateMoment = OWLUpdateStratgy.FixedUpdate;

    }
}
