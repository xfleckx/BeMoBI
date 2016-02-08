using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

using NLog;

namespace Assets.BeMoBI.Paradigms.SearchAndFind
{
    public class ConditionController : MonoBehaviour
    {
        private static NLog.Logger statistic = LogManager.GetLogger("Statistics");
        
        public ParadigmController paradigm;

        public Action OnLastConditionFinished;

        #region Condition state

        private LinkedList<TrialDefinition> trials;
        private LinkedListNode<TrialDefinition> currentDefinition;
        public Trial currentTrial;

        private ConditionDefinition currentCondition;
        public ConditionConfiguration conditionConfig;

        private Dictionary<ITrial, int> runCounter = new Dictionary<ITrial, int>();

        private bool currentRunShouldEndAfterTrialFinished;
        private bool pauseActive;

        private bool isRunning = false;
        public bool IsRunning
        {
            get
            {
                return isRunning;
            }
        }

        public List<ConditionDefinition> PendingConditions { get; internal set; }
        public List<ConditionDefinition> FinishedConditions { get; internal set; }

        private bool resetTheLastTrial = false;

        #endregion
         
        public void Initialize(ConditionDefinition requestedCondition)
        {
            if (isRunning)
                throw new InvalidOperationException("A condition is already running!");

            if(PendingConditions.Any(c => c.Equals(requestedCondition))){
                currentCondition = requestedCondition;
                PendingConditions.Remove(requestedCondition);
            }
            else
            {
                throw new ArgumentException("Requested Condition not available!");
            }

            ApplyConditionSpecificConfiguration(currentCondition.Config);

            trials = new LinkedList<TrialDefinition>(currentCondition.Trials);
        }

        public void ResetCurrentCondition()
        {
            PendingConditions.Add(currentCondition);
        }

        #region Trial Management


        private void ApplyConditionSpecificConfiguration(ConditionConfiguration config)
        {
            conditionConfig = config;
            // TODO Head Controller
            // TODO Body Controller

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

            trials.AddAfter(currentDefinition, new LinkedListNode<TrialDefinition>(pauseTrial));
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

            trials.AddBefore(currentDefinition, new LinkedListNode<TrialDefinition>(pauseTrial));
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
            isRunning = true;

            if (currentDefinition == null)
            {
                // Special case: First Trial after experiment start
                currentDefinition = trials.First;
            }
            else if (currentRunShouldEndAfterTrialFinished || currentDefinition.Next == null)
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
                    currentDefinition = currentDefinition.Next;
                }
                else
                {
                    // current definition stays - e.g. when a pause was requested
                    resetTheLastTrial = false;
                }
            }

            var definitionForNextTrial = currentDefinition.Value;

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

            var def = currentDefinition.Value;

            currentTrial.Initialize(def.MazeName, def.Path, def.Category, def.ObjectName);

            // this sets a callback to Trials Finished event (it's an pointer to a function)
            currentTrial.Finished += currentTrial_Finished;
        }

        /// <summary>
        /// Callback method, should be called by the trial itself - cause only the trial knows when it's finished
        /// </summary>
        /// <param name="trial">the trial instance which has finished</param>
        /// <param name="result">A collection of informations on the trial run</param>
        void currentTrial_Finished(Trial trial, TrialResult result)
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

        internal void SetNextConditionPending()
        {
            if (PendingConditions.Count > 0)
                Initialize(PendingConditions.First());
            else
                OnLastConditionFinished();
        }

        private void ConditionFinished()
        {
            SetNextConditionPending();
        }

        #endregion

    }
}