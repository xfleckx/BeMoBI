using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.IO;

using Debug = UnityEngine.Debug;

// A logging framework, mainly used to write the log and statistic files. 
// See also the NLog.config within the asset directory 
using NLog;
using Logger = NLog.Logger; // just aliasing


namespace Assets.Paradigms.SearchAndFind
{

    [RequireComponent(typeof(LSLMarkerStream))]
    public class ParadigmController : MonoBehaviour
    {
        private static Logger appLog = LogManager.GetLogger("App");

        private static Logger statistic = LogManager.GetLogger("Statistics");

        private bool isRunning = false;

        private ParadigmRunStatistics runStatistic;

        #region Constants

        private const string ParadgimConfigDirectoryName = "ParadigmConfig";

        private const string ParadigmConfigNamePattern = "VP_{0}_{1}";

        private const string DateTimeFileNameFormat = "yyyy-MM-dd_hh-mm";

        private const string DetailedDateTimeFileNameFormat = "yyyy-MM-dd_hh-mm-ss-tt";

        #endregion

        public string SubjectID = String.Empty;
        
        public ParadigmConfiguration config;

        public ActionWaypoint TrialEndPoint;
        public ParadigmInstanceDefinition InstanceDefinition;
        public VirtualRealityManager VRManager;
        public StartPoint startingPoint;
        public HUD_Instruction hud;
        public LSLMarkerStream markerStream;
        public GameObject objectPresenter;
        public ObjectPool objectPool;
        public Transform objectPositionAtTrialStart;
        public GameObject HidingSpotPrefab;
        public GameObject entrance;
        public FullScreenFade fading;
        
        void Awake()
        {
            if (VRManager == null)
                throw new MissingReferenceException("Reference to VirtualRealityManager is missing");

            if (markerStream == null)
                throw new MissingReferenceException("Reference to a MarkerStream instance is missing");

            if (hud == null)
                throw new MissingReferenceException("No HUD available, you are not able to give visual instructions");
        }

        public void SaveConfig()
        {
            var configAsJson = JsonUtility.ToJson(config);

            using (var streamWriter = new StreamWriter(GetPathToConfig()))
            {
                streamWriter.Write(configAsJson);
            }

        }

        public void LoadInstanceDefinitionFrom(FileInfo file)
        {

        }

        public string GetPathToConfig()
        {
            return Application.dataPath + @"\Resources\SearchAndFind_Config.json";
        }

        void Start()
        {

            if(config == null)
            {
                LoadConfig(true);
            }

            if (InstanceDefinition == null)
            {
                UnityEngine.Debug.Log("No instance definition loaded.");

                //! TODO check if instance definition is available if not generate one!

                return;
            }

            // this is enables access to variables used by the logging framework
            NLog.GlobalDiagnosticsContext.Set("subject_Id", SubjectID);

            appLog.Info("Initializing Paradigm");

            hud.Clear();

            fading.StartFadeIn();
        }


        void Update()
        {
            if (Input.GetKey(KeyCode.F5) && !IsRunning)
                StartTheExperimentFromBeginning();
        }

        #region Trials

        private LinkedList<TrialDefinition> trials;
        private LinkedListNode<TrialDefinition> currentDefinition;

        public Training training;
        public Experiment experiment;
        public Pause pause;
        public InstructionTrial instruction;

        public Trial currentTrial;

        private Dictionary<ITrial, int> runCounter = new Dictionary<ITrial, int>();

        private bool currentRunShouldEndAfterTrialFinished;

        public bool IsRunning
        {
            get
            {
                return isRunning;
            }
        }

        #endregion

        #region Trial Management

        public void StartTheExperimentFromBeginning()
        {
            trials = new LinkedList<TrialDefinition>(InstanceDefinition.Trials);

            appLog.Info(string.Format("Run complete paradigma as defined in {0}!", InstanceDefinition.name));

            runStatistic = new ParadigmRunStatistics();

            statistic.Info(string.Format("Starting new Paradigm Instance: VP_{0}", InstanceDefinition.Subject));

            SetNextTrialPending();
        }

        public void StartASubsetOfTrials<T>() where T : Trial
        {
            var nameOfTrialsToSelect = typeof(T).Name;

            appLog.Info(string.Format("Run only {0} trials!", nameOfTrialsToSelect));

            var selectedTrials = InstanceDefinition.Trials.Where((def) => def.TrialType.Equals(nameOfTrialsToSelect));

            trials = new LinkedList<TrialDefinition>(selectedTrials);

            runStatistic = new ParadigmRunStatistics();

            SetNextTrialPending();
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
                ParadigmInstanceFinished();
                return;
            }
            else
            {
                // normal case the next trial is the follower of the current trial due to the definition
                currentDefinition = currentDefinition.Next;
            }

            var definitionForNextTrial = currentDefinition.Value;

            if (definitionForNextTrial.TrialType.Equals(typeof(Training).Name))
            {
                Begin(training, definitionForNextTrial);
            }
            else if (definitionForNextTrial.TrialType.Equals(typeof(Experiment).Name))
            {
                Begin(experiment, definitionForNextTrial);

            }
            else if (definitionForNextTrial.TrialType.Equals(typeof(Pause).Name))
            {
                Begin(pause, definitionForNextTrial);
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
            currentTrial.gameObject.SetActive(true);

            currentTrial.enabled = true;

            currentTrial.paradigm = this;

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

            if (config.writeStatistics)
                statistic.Trace(string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}", "\t", trialType, trial.currentMazeName, trial.currentPathID, trial.objectToRemember.name, result.Duration.TotalMinutes));

            runStatistic.Add(trialType, trial.currentMazeName, trial.currentPathID, result);

            currentTrial.CleanUp();

            currentTrial.enabled = false;

            currentTrial.gameObject.SetActive(false);

            // TODO: replace with a more immersive door implementation
            entrance.SetActive(true);

            SetNextTrialPending();
        }

        public void ForceSaveEnd()
        {
            this.currentRunShouldEndAfterTrialFinished = true;
        }

        private void ParadigmInstanceFinished()
        {
            isRunning = false;

            hud.ShowInstruction("You made it!\nThx for participation!", "Experiment finished!");
            var completeTime = runStatistic.Trials.Sum(t => t.DurationInSeconds) / 60;

            double averageTimePerTraining = 0;

            if (runStatistic.Trials.Any(t => t.TrialType.Equals(typeof(Training).Name)))
                averageTimePerTraining = runStatistic.Trials.Where(t => t.TrialType.Equals(typeof(Training).Name)).Average(t => t.DurationInSeconds) / 60;

            double averageTimePerExperiment = 0;

            if (runStatistic.Trials.Any(t => t.TrialType.Equals(typeof(Experiment).Name)))
                averageTimePerExperiment = runStatistic.Trials.Where(t => t.TrialType.Equals(typeof(Experiment).Name)).Average(t => t.DurationInSeconds) / 60;

            statistic.Info(string.Format("Run took: {0} minutes, Avg Training: {1}     Avg Experiment {2}", completeTime, averageTimePerTraining, averageTimePerExperiment));

            appLog.Info("Paradigma run finished");
        }

        #endregion

        #region Public interface for controlling the paradigm remotely

        public void LoadConfig(bool writeNewWhenNotFound)
        {
            string jsonAsText = String.Empty;

            try
            { 
                using (var fileStream = new StreamReader(GetPathToConfig()))
                {
                    jsonAsText = fileStream.ReadToEnd();
                }
            }
            catch (FileNotFoundException)
            {
                Debug.Log("No Config found");
                appLog.Error(string.Format("No config file found at {0}! Using default values and write new config!", GetPathToConfig()));

                if (writeNewWhenNotFound) { 
                    config = new ParadigmConfiguration();
                }
            }

            config = JsonUtility.FromJson<ParadigmConfiguration>(jsonAsText);
        }

        public void InjectPauseTrial()
        {
            var pauseTrial = new TrialDefinition()
            {
                TrialType = typeof(Pause).Name
            };

            trials.AddAfter(currentDefinition, new LinkedListNode<TrialDefinition>(pauseTrial));
        }

        public void ReturnFromPauseTrial()
        {
            if (currentTrial.Equals(pause))
                currentTrial.ForceTrialEnd();
        }

        public void PerformSaveInterupt()
        {

        }

        public void SaveCurrentState()
        {
            // TODO serialize to JSON string...  JsonUtility.ToJson()
            throw new NotImplementedException("TODO");
        }

        public void LoadState()
        {
            throw new NotImplementedException("TODO");
        }

        #endregion
    }

    public static class MarkerPattern
    {
        public const string BeginTrial = "BeginTrial\t{0}\t{1}\t{2}\t{3}\t{4}";

        public const string Unit = "{0}\tUnit\t{1}\t{2}";

        public const string Enter = "Entering\t{0}\t{1}\t{2}";

        public const string ShowObject = "ShowObject\t{0}\t{1}";

        public const string ObjectFound = "ObjectFound\t{0}\t{1}\t{2}\t{3}";

        // TODO: Grid ID's should not be renderered as float values
        /// <summary>
        /// From {0} to {1} TurnType {2} UnitType {3}
        /// </summary>
        public const string Turn = "Turn\tFrom:{0}\tTo:{1}\t{2}\t{3}";

        /// <summary>
        /// From {0} to {1} expected {2} TurnType {3} UnitType {4}
        /// </summary>
        public const string WrongTurn = "Incorrect\tFrom:{0}\tTo:{1}\tExp:{2}\t{3}\t{4}";

        public const string EndTrial = "EndTrial\t{0}\t{1}\t{2}\t{3}\t{4}";

        public static string FormatBeginTrial(string trialTypeName, string mazeName, int pathId, string objectName, string categoryName)
        {
            return string.Format(BeginTrial, trialTypeName, mazeName, pathId, objectName, categoryName);
        }

        public static string FormatCorrectTurn(PathElement lastPathElement, PathElement currentPathElement)
        {
            var lastGridId = lastPathElement.Unit.GridID;

            var currentGridId = currentPathElement.Unit.GridID;

            return string.Format(Turn, lastGridId, currentGridId, lastPathElement.Type, lastPathElement.Turn);
        }

        public static string FormatIncorrectTurn(MazeUnit wrongUnitEntered, PathElement lastPathElement, PathElement expectedUnit)
        {
            var wrongGridId = wrongUnitEntered.GridID;

            var lastGridId = lastPathElement.Unit.GridID;

            var expectedGridId = expectedUnit.Unit.GridID;

            return string.Format(WrongTurn, lastGridId, wrongGridId, expectedGridId, lastPathElement.Type, lastPathElement.Turn);
        }

        public static string FormatFoundObject(string currentMazeName, int iD, string objectName, string categoryName)
        {
            return string.Format(ObjectFound, currentMazeName, iD, objectName, categoryName);
        }

        public static string FormatEndTrial(string trialTypeName, string mazeName, int pathId, string objectName, string categoryName)
        {
            return string.Format(EndTrial, trialTypeName, mazeName, pathId, objectName, categoryName);
        }

        public static string FormatDisplayObject(string objectName, string categoryName)
        {
            return string.Format(ShowObject, objectName, categoryName);
        }
    }

    #region TODO: Save instance state (SaveGames)
    //https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
    [Serializable]
    public class InstanceState
    {
        public string Subject;

        public string InstanceDefinition;

        public TrialDefinition LastFinishedTrial;
    }

    #endregion

    public class ParadigmRunStatistics
    {
        private string subjectId = String.Empty;

        public class TrialStatistic
        {
            private string mazeName;
            private int path;
            private double seconds;
            private string trialType;

            public TrialStatistic(string trialType, string mazeName, int path, double seconds)
            {
                this.trialType = trialType;
                this.mazeName = mazeName;
                this.path = path;
                this.seconds = seconds;
            }

            public string MazeName
            {
                get
                {
                    return mazeName;
                }
            }

            public int Path
            {
                get
                {
                    return path;
                }
            }

            public double DurationInSeconds
            {
                get
                {
                    return seconds;
                }
            }

            public string TrialType
            {
                get
                {
                    return trialType;
                }
            }
        }

        public List<TrialStatistic> Trials = new List<TrialStatistic>();

        public string SubjectId
        {
            get
            {
                return subjectId;
            }
        }

        public void Add(string trialType, string mazeName, int pathId, TrialResult result)
        {
            Trials.Add(new TrialStatistic(trialType, mazeName, pathId, result.Duration.TotalSeconds));
        }
    }

    [Serializable]
    public class ParadigmConfiguration
    {
        [SerializeField]
        public bool useTeleportation = false;

        [SerializeField]
        public bool writeStatistics = false;

        [SerializeField]
        public bool ifNoInstanceDefinitionCreateOne = false;

        [SerializeField]
        public float TimeToDisplayObjectToRememberInSeconds = 3;
        
        [SerializeField]
        public float TimeToDisplayObjectWhenFoundInSeconds = 2;
    }
}