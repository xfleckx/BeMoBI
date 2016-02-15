using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Events;

namespace Assets.BeMoBI.Scripts
{
    public class CommandReciever : MonoBehaviour
    {
        public IParadigmControl paradigm;

        // Use this for initialization
        void Start()
        {
            paradigm = GetComponent<IParadigmControl>();
        }

        public void RecieveAndApply(string command)
        {
            if (command.Contains("config"))
            {
                var parts = command.Split(' ');
                var conditionName = parts[1];
                paradigm.InitializeCondition(conditionName);
            }

            if (command.Contains("start"))
            {
                paradigm.StartExperimentWithCurrentPendingCondition();
            }

            if (command.Contains("restart"))
            {
                paradigm.Restart();
            }
        }

    }

}

