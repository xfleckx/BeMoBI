using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace Assets.BeMoBI.Paradigms.SearchAndFind
{
    public class Training : Trial
    {
        public float WaitOnInstructionInSeconds = 2;

        /// <summary>
        /// Ready means the trial is waiting for an event which causes the actual start of the trial. 
        /// </summary>
        public override void SetReady()
        {
            base.SetReady();

            SetLightningOn(path, mazeInstance);

        }

        protected override void ShowObjectAtStart()
        {
            StartCoroutine(DisplayInstruction());
        }

        IEnumerator DisplayInstruction()
        {

            if (conditionConfig.UseTextInstructions) {

                paradigm.hud.Clear();

                paradigm.hud.ShowInstruction("Merke dir das Objekt und den Pfad durch das Labyrinth!", "Aufgabe");

                yield return new WaitForSeconds(WaitOnInstructionInSeconds);
            }

            paradigm.hud.Clear();

            base.ShowObjectAtStart();

            yield return null;
        }
    }
}
