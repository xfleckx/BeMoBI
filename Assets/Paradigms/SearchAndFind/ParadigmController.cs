using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.IO;

using Debug = UnityEngine.Debug;
using Assets.BeMoBI.Scripts;
// A logging framework, mainly used to write the log and statistic files. 
// See also the NLog.config within the asset directory 
// Pittfall: You need to copy the NLog.config file to the *_DATA directory after the build!
using NLog;
using Logger = NLog.Logger; // just aliasing
using Assets.BeMoBI.Paradigms.SearchAndFind.Scripts;
using Assets.BeMoBI.Scripts.PhaseSpaceExtensions;

namespace Assets.BeMoBI.Paradigms.SearchAndFind
{
    public class ParadigmController : MonoBehaviour, IProvideRigidBodyFile
    {
        public const string STD_CONFIG_NAME = "SearchAndFind_Config.json";
        
        private const string COND_MOCAP = "mocap";
        private const string COND_MOCAP_ROT = "mocap_rot";
        private const string COND_DESKTOP = "desktop";
        private const string COND_MONOSCOP = "monoscop";
        private const string COND_STANDING = "standing";
        private const string COND_SITTING = "sitting";

        private static Logger appLog = LogManager.GetLogger("App");

        private static Logger statistic = LogManager.GetLogger("Statistics");

        private bool isRunning = false;

        private ParadigmRunStatistics runStatistic;

        private bool resetTheLastTrial = false;

        #region Constants

        private const string ParadgimConfigDirectoryName = "ParadigmConfig";

        private const string ParadigmConfigNamePattern = "VP_{0}_{1}";

        private const string DateTimeFileNameFormat = "yyyy-MM-dd_hh-mm";

        private const string DetailedDateTimeFileNameFormat = "yyyy-MM-dd_hh-mm-ss-tt";

        #endregion

        #region dependencies

        public string SubjectID = String.Empty;

        public AppInit appInit;

        public FileInfo fileToLoadedConfig;
        
        public ParadigmConfiguration config;

        public ParadigmInstanceDefinition InstanceDefinition;
        
        public ActionWaypoint TrialEndPoint;
        public VirtualRealityManager VRManager;
        public StartPoint startingPoint;
        public HUD_Instruction hud;
        public HUD_DEBUG debug_hud;
        public LSLSearchAndFindMarkerStream marker;
        public GameObject objectPresenter;
        public ObjectPool objectPool;
        public Transform objectPositionAtTrialStart;
        public GameObject HidingSpotPrefab;
        public GameObject entrance;
        public FullScreenFade fading;
        public Teleporting teleporter;
        public VRSubjectController subject;
        public BaseFogControl fogControl;
        public LSLSubjectRelativePositionStream relativePositionStream;
        public Transform FocusPointAtStart;
        
        #endregion

        void Awake()
        {
            if (VRManager == null)
                throw new MissingReferenceException("Reference to VirtualRealityManager is missing");

            if (marker == null)
                throw new MissingReferenceException("Reference to a MarkerStream instance is missing");

            if (hud == null)
                throw new MissingReferenceException("No HUD available, you are not able to give visual instructions");

            if (subject == null)
                subject = FindObjectOfType<VRSubjectController>();
            

        }
        
        void Start()
        {
            First_GetTheSubjectName();
            
            appLog.Info("Initializing Paradigm");

            Second_LoadOrGenerateAConfig();

            Third_LoadInstanceDefinitionAndSupplySubjectIDAndConfig();

            hud.Clear();

            fogControl.DisappeareImmediately();

            fading.StartFadeIn();

            marker.LogAlsoToFile = config.logMarkerToFile;
        }

        private void First_GetTheSubjectName()
        {
            if (SubjectID == string.Empty)
            {
                if (appInit.Options.subjectId != String.Empty)
                    SubjectID = appInit.Options.subjectId;
                else
                    SubjectID = ParadigmUtils.GetRandomSubjectName();
            }
            
            // this is enables access to variables used by the logging framework
            NLog.GlobalDiagnosticsContext.Set("subject_Id", SubjectID);

            appLog.Info(string.Format("Using Subject Id: {0}", SubjectID));
        }

        private void Second_LoadOrGenerateAConfig()
        {
            var pathOfDefaultConfig = new FileInfo(Application.dataPath + @"\" + STD_CONFIG_NAME);

            if (config == null)
            {
                appLog.Info("Load Config or create a new one!");

                if (appInit.HasOptions &&
                    appInit.Options.fileNameOfCustomConfig != String.Empty &&
                    File.Exists(Application.dataPath + @"\" + appInit.Options.fileNameOfCustomConfig))
                {
                    var configFile = new FileInfo(Application.dataPath + @"\" + appInit.Options.fileNameOfCustomConfig);

                    appLog.Info(string.Format("Load specific config: {0}!", configFile.FullName));

                    config = ConfigUtil.LoadConfig<ParadigmConfiguration>(configFile, true, 
                        () => appLog.Error("Loading config failed, using default config + writing a default config"));
                }
                else if (pathOfDefaultConfig.Exists) 
                {
                    appLog.Info(string.Format("Found default config at {0}", pathOfDefaultConfig.Name));

                    config = ConfigUtil.LoadConfig<ParadigmConfiguration>(pathOfDefaultConfig, false, () => {
                        appLog.Error (string.Format("Load default config at {0} failed!", pathOfDefaultConfig.Name));
                    });
                }
                else
                { 
                    config = ScriptableObject.CreateInstance<ParadigmConfiguration>();

                    // TODO if cmd args available use them here

                    appLog.Info(string.Format("New Config created will be saved to: {0}! Reason: No config file found!", pathOfDefaultConfig.FullName));

                    try
                    {
                        ConfigUtil.SaveAsJson<ParadigmConfiguration>(pathOfDefaultConfig, config);
                    }
                    catch (Exception e)
                    {
                        appLog.Info(string.Format("Config could not be saved to: {0}! Reason: {1}", pathOfDefaultConfig.FullName, e.Message));
                    }
                }

            }
            else
            {
                appLog.Info("Config not null!");
            }

        }

        private void Third_LoadInstanceDefinitionAndSupplySubjectIDAndConfig()
        {
            if (InstanceDefinition == null)
            {
                UnityEngine.Debug.Log("No instance definition found.");

                if (appInit.HasOptions && File.Exists(appInit.Options.fileNameOfParadigmDefinition))
                {
                    var logMsg = string.Format("Load instance definition from {0}", appInit.Options.fileNameOfParadigmDefinition);

                    UnityEngine.Debug.Log(logMsg);

                    appLog.Info(logMsg);

                    var fileContainingDefinition = new FileInfo(appInit.Options.fileNameOfParadigmDefinition);

                    LoadInstanceDefinitionFrom(fileContainingDefinition);
                }
                else
                {
                    var factory = new InstanceDefinitionFactory();

                    factory.config = config;
                    
                    factory.EstimateConfigBasedOnAvailableElements();

                    if (!factory.IsAbleToGenerate)
                    {
                        appLog.Fatal("Not able to create an instance definition based on the given configuration! Check the paradigm using the UnityEditor and rebuild the paradigm or change the expected configuration!");
                        Application.Quit();
                    }
                    else
                    {
                        InstanceDefinition = factory.Generate(SubjectID, config.expectedConditions);
                        
                        var fileNameWoExt = string.Format("{0}/PreDefinitions/VP_{1}_Definition", Application.dataPath, InstanceDefinition.Subject);

                        var jsonString = JsonUtility.ToJson(InstanceDefinition, true);

                        var targetFileName = fileNameWoExt + ".json";

                        appLog.Info(string.Format("Saving new definition at: {0}", targetFileName));

                        using (var file = new StreamWriter(targetFileName))
                        {
                            file.Write(jsonString);
                        }
                    }
                }
            }

        }

        void Update()
        {
            if (Input.GetKey(KeyCode.F5) && !IsRunning)
                StartTheExperimentFromBeginning();

            if (Input.GetKeyUp(KeyCode.F1))
                ToogleDebugHUD();

        }

        private void ToogleDebugHUD()
        {
            if (debug_hud != null)
                debug_hud.gameObject.SetActive(!debug_hud.gameObject.activeSelf);
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
        private bool pauseActive;
        private ConditionDefinition currentCondition;

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
            //InitializeCondition(c)

            trials = new LinkedList<TrialDefinition>(currentCondition.Trials);

            appLog.Info(string.Format("Run complete paradigma as defined in {0}!", InstanceDefinition.name));

            runStatistic = new ParadigmRunStatistics();

            statistic.Info(string.Format("Starting new Paradigm Instance: VP_{0}", InstanceDefinition.Subject));

            SetNextTrialPending();
        }

        public void InitializeCondition(string condition)
        {

            currentCondition = InstanceDefinition.Get(condition);
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
                if (!resetTheLastTrial) { 
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

            if(!pauseActive)
                SetNextTrialPending();
        }

        public void AfterTeleportingToEndPoint()
        {
            subject.transform.LookAt(FocusPointAtStart);
            subject.transform.rotation = Quaternion.Euler(0, subject.transform.rotation.eulerAngles.y, 0);
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
        
        /// <summary>
        /// Loads a predefined definition from a file
        /// May override already loaded config!
        /// </summary>
        /// <param name="file"></param>
        public void LoadInstanceDefinitionFrom(FileInfo file)
        {
            using (var reader = new StreamReader(file.FullName))
            {
                var jsonFromFile = reader.ReadToEnd();

                InstanceDefinition = JsonUtility.FromJson<ParadigmInstanceDefinition>(jsonFromFile);

                if (InstanceDefinition == null)
                {
                    appLog.Fatal(string.Format("Loading {0} as Instance Definition failed!", file.FullName));
                    Application.Quit();
                }
            }
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

        public FileInfo GetRigidBodyDefinition()
        {
            var fileName = config.nameOfRigidBodyDefinition;

            var expectedFilePath = Path.Combine(Application.dataPath, fileName);

            if (File.Exists(expectedFilePath))
            {
                return new FileInfo(expectedFilePath);
            }

            return null;
        }

        public void SubjectTriesToSubmit()
        {
            if (currentTrial != null && currentTrial.acceptsASubmit)
            {
                currentTrial.RecieveSubmit();
            }

            if (pauseActive)
            {
                hud.ShowInstruction("Press the Submit Button to continue", "Break");

                SetNextTrialPending();
            }
        }

        public void ForceABreakInstantly()
        {
            if (currentTrial != null) {

                resetTheLastTrial = true;
                pauseActive = true;

                currentTrial.ForceTrialEnd();
            }

            hud.ShowInstruction("Press the Submit Button to continue!\n Close your eyes and talk to the supervisor!", "Break");
        }

        #endregion
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
}