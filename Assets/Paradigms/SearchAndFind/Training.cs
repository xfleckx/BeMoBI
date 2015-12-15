using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace Assets.Paradigms.SearchAndFind
{
    public class Training : Trial
    {
        /// <summary>
        /// A Trial Start may caused from external source (e.g. a key press)
        /// </summary>
        public override void SetReady()
        {
            base.SetReady();

            SetLightningOn(path, mazeInstance);

        }

        protected override void ShowObject()
        {
            hud.Clear();

            hud.ShowInstruction("Remember the given path for this object!");

            base.ShowObject();
        }
    }
}
