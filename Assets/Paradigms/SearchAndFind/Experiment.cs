using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace Assets.Paradigms.SearchAndFind
{
    public class Experiment : Trial
    {
        /// <summary>
        /// A Trial Start may caused from external source (e.g. a key press)
        /// </summary>
        public override void StartTrial()
        {
            SwitchAllLightsOff(mazeInstance);

            base.StartTrial();
             
            hud.ShowInstruction("Retrieve the path to this object");
            
        }
    }
}
