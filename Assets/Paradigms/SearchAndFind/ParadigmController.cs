﻿using UnityEngine;
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
using UnityEngine.Assertions;

namespace Assets.BeMoBI.Paradigms.SearchAndFind
{
    public class ParadigmController : MonoBehaviour, IParadigmControl, IProvideRigidBodyFile
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
        
        public ParadigmRunStatistics runStatistic;

        public string PathToLoadedConfig = String.Empty;

        #region Constants

        private const string ParadgimConfigDirectoryName = "ParadigmConfig";

        private const string ParadigmConfigNamePattern = "VP_{0}_{1}";

        private const string DateTimeFileNameFormat = "yyyy-MM-dd_hh-mm";

        private const string DetailedDateTimeFileNameFormat = "yyyy-MM-dd_hh-mm-ss-tt";

        #endregion

        #region dependencies

        public string SubjectID = String.Empty;

        public AppInit appInit;

        public ConditionController conditionController;
        
        public FileInfo fileToLoadedConfig;
        
        public ParadigmConfiguration Config;

        public ParadigmModel InstanceDefinition;
        
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
        
        public Training training;
        public Experiment experiment;
        public Pause pause;
        public InstructionTrial instruction;
        
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

            Assert.IsNotNull<ConditionController>(conditionController);

            conditionController.OnLastConditionFinished += ParadigmInstanceFinished;
        }

        void Start()
        {
            First_GetTheSubjectName();
            
            appLog.Info("Initializing Paradigm");

            Second_LoadOrGenerateAConfig();
            
            Third_LoadOrGenerateInstanceDefinition();

            Fourth_InitializeFirstOrDefaultCondition();

            hud.Clear();

            fogControl.DisappeareImmediately();

            fading.StartFadeIn();

            marker.LogAlsoToFile = Config.logMarkerToFile;
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
            var pathOfDefaultConfig = new FileInfo(Application.dataPath + Path.AltDirectorySeparatorChar + STD_CONFIG_NAME);

            if (Config == null)
            {
                appLog.Info("Load Config or create a new one!");

                if (appInit.HasOptions &&
                    appInit.Options.fileNameOfCustomConfig != String.Empty &&
                    File.Exists(Application.dataPath + Path.AltDirectorySeparatorChar + appInit.Options.fileNameOfCustomConfig))
                {
                    var configFile = new FileInfo(Application.dataPath + Path.AltDirectorySeparatorChar + appInit.Options.fileNameOfCustomConfig);

                    appLog.Info(string.Format("Load specific config: {0}!", configFile.FullName));

                    Config = ConfigUtil.LoadConfig<ParadigmConfiguration>(configFile, true, 
                        () => appLog.Error("Loading config failed, using default config + writing a default config"));

                    PathToLoadedConfig = configFile.FullName;
                }
                else if (pathOfDefaultConfig.Exists) 
                {
                    appLog.Info(string.Format("Found default config at {0}", pathOfDefaultConfig.Name));

                    Config = ConfigUtil.LoadConfig<ParadigmConfiguration>(pathOfDefaultConfig, false, () => {
                        appLog.Error (string.Format("Load default config at {0} failed!", pathOfDefaultConfig.Name));
                    });

                    PathToLoadedConfig = pathOfDefaultConfig.FullName;
                }
                else
                {
                    Config = ParadigmConfiguration.GetDefault();

                    var customPath = pathOfDefaultConfig;

                    if (appInit.HasOptions && appInit.Options.fileNameOfCustomConfig != string.Empty)
                    {
                        customPath = new FileInfo(Application.dataPath + Path.AltDirectorySeparatorChar + appInit.Options.fileNameOfCustomConfig);
                    }
                    
                    appLog.Info(string.Format("New Config created will be saved to: {0}! Reason: No config file found!", customPath.FullName));

                    try
                    {
                        ConfigUtil.SaveAsJson<ParadigmConfiguration>(pathOfDefaultConfig, Config);
                    }
                    catch (Exception e)
                    {
                        appLog.Info(string.Format("Config could not be saved to: {0}! Reason: {1}", pathOfDefaultConfig.FullName, e.Message));
                    }

                    PathToLoadedConfig = customPath.FullName;
                }

            }
            else
            {
                appLog.Info("Config not null!");
            }

        }
        
        private void Third_LoadOrGenerateInstanceDefinition()
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
                    UnityEngine.Debug.Log("Create instance definition.");

                    var factory = new ParadigmModelFactory();

                    factory.config = Config;

                    try
                    {
                        factory.EstimateConfigBasedOnAvailableElements();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);

                        appLog.Fatal(e, "Incorrect configuration!");

                        appLog.Fatal("Not able to create an instance definition based on the given configuration! Check the paradigm using the UnityEditor and rebuild the paradigm or change the expected configuration!");

                        Application.Quit();

                        return;
                    }
                     
                    InstanceDefinition = factory.Generate(SubjectID, Config.conditionConfigurations);

                    Save(InstanceDefinition);
                }
            }

        }

        private void Fourth_InitializeFirstOrDefaultCondition()
        {
            conditionController.PendingConditions = InstanceDefinition.Conditions;
            conditionController.FinishedConditions = new List<ConditionDefinition>();
        }

        private void Save(ParadigmModel instanceDefinition)
        {
            var fileNameWoExt = string.Format("{1}{0}PreDefinitions{0}VP_{2}_Definition", Path.AltDirectorySeparatorChar, Application.dataPath, instanceDefinition.Subject);

            var jsonString = JsonUtility.ToJson(InstanceDefinition, true);

            var targetFileName = fileNameWoExt + ".json";

            appLog.Info(string.Format("Saving new definition at: {0}", targetFileName));

            using (var file = new StreamWriter(targetFileName))
            {
                file.Write(jsonString);
            }
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.F5))
            {
                if( !conditionController.IsRunning )
                    StartExperimentFromBeginning();

                if (conditionController.IsRunning && conditionController.PendingForNextCondition)
                    conditionController.SetNextConditionPending();


            }

            if (Input.GetKeyUp(KeyCode.F1))
                ToogleDebugHUD();
        }

        private void ToogleDebugHUD()
        {
            if (debug_hud != null)
                debug_hud.gameObject.SetActive(!debug_hud.gameObject.activeSelf);
        }
        public void AfterTeleportingToEndPoint()
        {
            subject.transform.LookAt(FocusPointAtStart);
            subject.transform.rotation = Quaternion.Euler(0, subject.transform.rotation.eulerAngles.y, 0);
        }

        private void ParadigmInstanceFinished()
        {
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

        public void OnRotationEvent(RotationEventArgs args)
        {
            if (args.state == RotationEventArgs.State.Begin)
                marker.Write("Begin Rotation", LSLTimeSync.Instance.UpdateTimeStamp);
            
            if (args.state == RotationEventArgs.State.End)
                marker.Write("End Rotation", LSLTimeSync.Instance.UpdateTimeStamp);
        }

        #region Public interface for controlling the paradigm remotely
        
        public void StartExperimentFromBeginning()
        {
            appLog.Info(string.Format("Run complete paradigma as defined in {0}!", InstanceDefinition.name));

            runStatistic = new ParadigmRunStatistics();

            statistic.Info(string.Format("Starting new Paradigm Instance: VP_{0}", InstanceDefinition.Subject));
            
            conditionController.SetNextConditionPending();
            conditionController.StartTheConditionWithFirstTrial();
        }
        
        public void InitializeCondition(string condition)
        {
            try
            {
                var requestedCondition =  InstanceDefinition.Get(condition);

                conditionController.Initialize(requestedCondition);
            }
            catch (ArgumentException e)
            {
                appLog.Error(e, "Expected Condition could not be started implemented!");
            }

        }
        
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

                InstanceDefinition = JsonUtility.FromJson<ParadigmModel>(jsonFromFile);

                if (InstanceDefinition == null)
                {
                    appLog.Fatal(string.Format("Loading {0} as Instance Definition failed!", file.FullName));
                    Application.Quit();
                }
            }
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
            var fileName = Config.nameOfRigidBodyDefinition;

            var expectedFilePath = Path.Combine(Application.dataPath, fileName);

            if (File.Exists(expectedFilePath))
            {
                return new FileInfo(expectedFilePath);
            }

            return null;
        }

        public void SubjectTriesToSubmit()
        {
            if (conditionController.currentTrial != null && conditionController.currentTrial.acceptsASubmit)
            {
                conditionController.currentTrial.RecieveSubmit();
            }

        }

        public void ForceABreakInstantly()
        {
            //conditionController.InjectPauseTrial();
            conditionController.ResetCurrentTrial();
            hud.ShowInstruction("Press the Submit Button to continue!\n Close your eyes and talk to the supervisor!", "Break");
        }
          
        public void StartExperimentWithCurrentPendingCondition()
        {
            conditionController.StartTheConditionWithFirstTrial();
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