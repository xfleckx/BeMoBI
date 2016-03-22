﻿using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.Assertions;

namespace Assets.BeMoBI.Scripts
{
    public class CommandReciever : MonoBehaviour
    {
        public IParadigmControl paradigm;
        public VRSubjectController subject;

        // Use this for initialization
        void Start()
        {
            paradigm = GetComponent<IParadigmControl>();
            subject = FindObjectOfType<VRSubjectController>();

            Assert.IsNotNull(subject);
        }

        public void RecieveAndApply(string command)
        {
            if (command.Contains("config"))
            {
                var parts = command.Split(' ');
                var conditionName = parts[1].Replace("\n","").Trim();
                paradigm.InitializeCondition(conditionName);
            }

            if (command.Contains("start"))
            {
                paradigm.StartExperimentWithCurrentPendingCondition();
            }

            if (command.Equals("pause"))
            {
                paradigm.ConditionController.InjectPauseTrialAfterCurrentTrial();
            }

            if(command.Equals("pause end"))
            {
                paradigm.ConditionController.ReturnFromPauseTrial();
            }

            if (command.Equals("recalibrate_SubjectsOrientation"))
            {
                if (subject != null)
                    subject.Recalibrate();
            }

            if (command.Equals("force_end_of_experiment"))
            {
                paradigm.ForceSaveEndOfExperiment();
            }

            if (command.Contains("restart"))
            {
                paradigm.Restart();
            }
        }

    }

}

