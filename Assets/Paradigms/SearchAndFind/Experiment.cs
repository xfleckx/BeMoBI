﻿using UnityEngine;
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
        public override void SetReady()
        {
            SwitchAllLightsOff(mazeInstance);

            base.SetReady();
        }

        protected override void ShowObject()
        {
            hud.Clear();

            hud.ShowInstruction("Retrieve the path to this object","Task");

            base.ShowObject();
        }



        
    }
}
