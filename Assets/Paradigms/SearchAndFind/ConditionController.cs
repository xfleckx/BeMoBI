﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

using NLog;
using Assets.BeMoBI.Scripts.Controls;

namespace Assets.BeMoBI.Paradigms.SearchAndFind
{
    public class ConditionController : MonoBehaviour
    {
        private static NLog.Logger statistic = LogManager.GetLogger("Statistics");
        
        public ParadigmController paradigm;

        public Action OnLastConditionFinished;

        #region Condition state

        private LinkedList<TrialDefinition> trials;
        public LinkedListNode<TrialDefinition> currentTrialDefinition;
        public Trial currentTrial;

        public ConditionDefinition currentCondition;
        public ConditionConfiguration conditionConfig;

        private Dictionary<ITrial, int> runCounter = new Dictionary<ITrial, int>();

        private bool currentRunShouldEndAfterTrialFinished;
        private bool pauseActive;

        private bool isTrialRunning = false;
        public bool IsRunning
        {
            get
            {
                return isTrialRunning;
            }
        }

        public List<ConditionDefinition> PendingConditions { get; internal set; }
        public List<ConditionDefinition> FinishedConditions { get; internal set; }

        public bool IsConditionRunning
        {
            get
            {
                return isConditionRunning;
            }
        }

        public bool PendingForNextCondition { get; private set; }

        private bool resetTheLastTrial = false;
        private bool isConditionRunning;

        #endregion

        public void Initialize(ConditionDefinition requestedCondition)
        {
            if (IsConditionRunning)
                throw new InvalidOperationException("A condition is already running!");
            
            if (PendingConditions.Any(c => c.Equals(requestedCondition))){
                currentCondition = requestedCondition;
                PendingConditions.Remove(requestedCondition);
            }
            else
            {
                throw new ArgumentException("Requested Condition not available!");
            }

            ApplyConditionSpecificConfiguration(currentCondition.Config);

            trials = new LinkedList<TrialDefinition>(currentCondition.Trials);

            isConditionRunning = true;

            SetNextTrialPending();
        }

        public void ResetCurrentCondition()
        {
            PendingConditions.Add(currentCondition);
        }

        #region Trial Management


        private void ApplyConditionSpecificConfiguration(ConditionConfiguration config)
        {
            conditionConfig = config;

            paradigm.subject.Change<IBodyMovementController>(config.BodyControllerName);
            paradigm.subject.Change<IHeadMovementController>(config.HeadControllerName);
        }

        /// <summary>
        /// Use this for regular breaks
        /// </summary>
        public void InjectPauseTrialAfterCurrentTrial()
        {
            var pauseTrial = new TrialDefinition()
            {
                TrialType = typeof(Pause).Name
            };

            trials.AddAfter(currentTrialDefinition, new LinkedListNode<TrialDefinition>(pauseTrial));
        }

        /// <summary>
        /// use this for instant breaks
        /// </summary>
        public void InjectPauseTrialBeforeCurrentTrial()
        {
            var pauseTrial = new TrialDefinition()
            {
                TrialType = typeof(Pause).Name
            };

            trials.AddBefore(currentTrialDefinition, new LinkedListNode<TrialDefinition>(pauseTrial));
        }
        
        public void ReturnFromPauseTrial()
        {
            if (currentTrial.Equals(paradigm.pause))
                currentTrial.ForceTrialEnd();
        }

        /// <summary>
        /// Setup the next trial but wait until subject enters startpoint
        /// 
        /// Take into a account that the paradigm definition is a linked list!
        /// </summary>
        void SetNextTrialPending()
        {
            isTrialRunning = true;

            if (currentTrialDefinition == null)
            {
                // Special case: First Trial after experiment start
                currentTrialDefinition = trials.First;
            }
            else if (currentRunShouldEndAfterTrialFinished || currentTrialDefinition.Next == null)
            {
                // Special case: Last Trial either the run was canceld or all trials done
                ConditionFinished();
                return;
            }
            else
            {
                if (!resetTheLastTrial)
                {
                    // normal case the next trial is the follower of the current trial due to the definition
                    currentTrialDefinition = currentTrialDefinition.Next;
                }
                else
                {
                    // current definition stays - e.g. when a pause was requested
                    resetTheLastTrial = false;
                }
            }

            var definitionForNextTrial = currentTrialDefinition.Value;

            if (definitionForNextTrial.TrialType.Equals(typeof(Training).Name))
            {
                Begin(paradigm.training, definitionForNextTrial);
            }
            else if (definitionForNextTrial.TrialType.Equals(typeof(Experiment).Name))
            {
                Begin(paradigm.experiment, definitionForNextTrial);

            }
            else if (definitionForNextTrial.TrialType.Equals(typeof(Pause).Name))
            {
                Begin(paradigm.pause, definitionForNextTrial);
            }
        }
        
        public void Begin<T>(T trial, TrialDefinition trialDefinition) where T : Trial
        {
            if (!runCounter.ContainsKey(trial))
                runCounter.Add(trial, 0);

            currentTrial = trial;

            Prepare(currentTrial);

            currentTrial.SetReady();
        }

        private void Prepare(Trial currentTrial)
        {
            currentTrial.conditionConfig = currentCondition.Config;

            currentTrial.gameObject.SetActive(true);

            currentTrial.enabled = true;

            currentTrial.paradigm = this.paradigm;

            var def = currentTrialDefinition.Value;

            currentTrial.Initialize(def.MazeName, def.Path, def.Category, def.ObjectName);

            // this sets a callback to Trials Finished event (it's an pointer to a function)
            currentTrial.Finished += CallWhenCurrentTrialFinished;
        }

        /// <summary>
        /// Callback method, should be called by the trial itself - cause only the trial knows when it's finished
        /// </summary>
        /// <param name="trial">the trial instance which has finished</param>
        /// <param name="result">A collection of informations on the trial run</param>
        void CallWhenCurrentTrialFinished(Trial trial, TrialResult result)
        {
            runCounter[trial]++;

            var trialType = trial.GetType().Name;

            if (this.paradigm.Config.writeStatistics)
                statistic.Trace(string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}", "\t", trialType, trial.currentMazeName, trial.currentPathID, trial.objectToRemember.name, result.Duration.TotalMinutes));

            paradigm.runStatistic.Add(trialType, trial.currentMazeName, trial.currentPathID, result);

            currentTrial.CleanUp();

            currentTrial.enabled = false;

            currentTrial.gameObject.SetActive(false);

            // TODO: replace with a more immersive door implementation
            paradigm.entrance.SetActive(true);

            if (!pauseActive)
                SetNextTrialPending();
        }

        internal void ResetCurrentTrial()
        {
            if (currentTrial != null)
            {
                currentTrial.ForceTrialEnd();
            }
        }

        public void SetNextConditionPending()
        {
            PendingForNextCondition = false;

            if (PendingConditions.Count > 0)
                Initialize(PendingConditions.First());
            else
                OnLastConditionFinished();
        }

        public void SetSpecificConditionPending(string conditionName)
        {
            if(PendingConditions.Any(c => c.Identifier.Equals(conditionName)))
            {
                var requestedCondition = PendingConditions.First();

                PendingForNextCondition = false;

                Initialize(requestedCondition);
            }
                

        }


        private void ConditionFinished()
        {
            isConditionRunning = false;

            currentTrialDefinition = null;

            GC.Collect();

            if (paradigm.Config.waitForCommandOnConditionEnd)
            {
                PendingForNextCondition = true;
                return;
            }

            SetNextConditionPending();
        }

        #endregion

    }
}