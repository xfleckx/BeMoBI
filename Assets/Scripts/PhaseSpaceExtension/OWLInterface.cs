using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System;
using PhaseSpace;
using System.IO;
using System.Linq;
using UnityEngine.Assertions;

namespace Assets.BeMoBI.Scripts.PhaseSpaceExtensions
{
    public enum OWLUpdateStratgy { FixedUpdate, Update, OnPreRender }

    public delegate void OnPostOwlUpdate();
    public delegate void OnOwlConnected();

    /// <summary>
    /// Extension to the OwlTracker example implementation from PhaseSpace Asset Package
    /// </summary>
    public class OWLInterface : MonoBehaviour
    {
        NLog.Logger appLog = NLog.LogManager.GetLogger("App");

        public OWLUpdateStratgy updateMoment = OWLUpdateStratgy.FixedUpdate;


        public bool autoConnectOnStart = false;

        public bool CreateDefaultPointTracker = false;

        public bool showDeprecatedOnGUI = false;

        public string configFilePath;

        public OWLTracker Tracker;

        public PhaseSpaceConfig config;

        private bool connected = false;

        public bool IsConnected { get { return connected; } }

        public OnPostOwlUpdate OwlUpdateCallbacks;

        public OnOwlConnected OwlConnectedCallbacks;

        public Action OwlConnectionFailed;

        public float OWLUpdateTook = 0f;

        protected string message = String.Empty;

        private Stopwatch stopWatch = new Stopwatch();

        public const string DEFAULT_CONFIG_NAME = "phasespace.json";

        //
        void Awake()
        {
            //isSlave = PlayerPrefs.GetInt("owlInSlaveMode", 0) == 1;

            Assert.IsNotNull(Tracker);
        }

        // Use this for initialization
        void Start()
        {
            LoadOrUseDefaultConfig();

            if (!Tracker.Device.Equals(string.Empty) && autoConnectOnStart)
            {
                ConnectToOWLInstance();
                connected = Tracker.Connected();
            }
        }

        public void createDefaultConfigFilePath()
        {
            configFilePath = Path.Combine(Application.dataPath, DEFAULT_CONFIG_NAME);

            configFilePath = configFilePath.Replace(Application.dataPath, "Assets");
        }

        public void LoadOrUseDefaultConfig()
        {
            if (!Application.isEditor)
            {
                var fileName = Path.GetFileName(configFilePath);

                configFilePath = Path.Combine(Application.dataPath, fileName);
            }

            if (configFilePath.Equals("") || !File.Exists(configFilePath))
            {
                appLog.Info(string.Format("Creating default phasespace config '{0}' at {1}", DEFAULT_CONFIG_NAME, Application.dataPath));

                createDefaultConfigFilePath();
            }

            appLog.Info(string.Format("Use phasespace config '{0}'",configFilePath));

            var expectedConfigFileFullPath = new FileInfo(configFilePath);

            config = ConfigUtil.LoadConfig<PhaseSpaceConfig>(expectedConfigFileFullPath, true, () =>
            {
                var message = string.Format("Could not load expected phasespace config: {0}", expectedConfigFileFullPath.FullName);
                appLog.Error(message);
            });

            this.autoConnectOnStart = config.autoConnectOnStart;
            this.updateMoment = config.updateMoment;
            this.CreateDefaultPointTracker = config.CreateDefaultPointTracker;

            this.Tracker.SlaveMode = config.isSlave;
            this.Tracker.BroadcastMode = config.broadcast;
            this.Tracker.Device = config.OWLHost;
        }

        public void SaveConfig()
        {
            var expectedConfigFile = new FileInfo(configFilePath);

            config.autoConnectOnStart = this.autoConnectOnStart;
            config.updateMoment = this.updateMoment;
            config.CreateDefaultPointTracker = this.CreateDefaultPointTracker;

            config.isSlave = Tracker.SlaveMode;
            config.broadcast = Tracker.BroadcastMode;
            config.OWLHost = Tracker.Device;

            ConfigUtil.SaveAsJson(expectedConfigFile, config);
        }

        public void ConnectToOWLInstance()
        {
            if (Tracker == null)
            {
                var msg = "Missing reference to OWL Client instance";

                appLog.Error(msg);

                UnityEngine.Debug.Log(msg);

                return;
            }

            var availableServers = Tracker.GetServers();

            Server serverToUse = null;

            if (!availableServers.Any())
            {
                var msg = "Could not found any PhaseSpace server in the Network!";
                appLog.Fatal(msg);
                UnityEngine.Debug.Log(msg);
                return;
            }

            if (Tracker.Device != String.Empty && !availableServers.Any(server => server.address.Equals(Tracker.Device)))
            {
                var msg = string.Format("Server with the expected address: {0} not found under available servers", Tracker.Device);
                appLog.Error(msg);
                UnityEngine.Debug.Log(msg);

                serverToUse = availableServers.FirstOrDefault();

                var automaticUsage = string.Format("Automatic use the first OWL server. {0}", serverToUse.address);

                appLog.Info(automaticUsage);
                UnityEngine.Debug.Log(automaticUsage);
            }
            else
            {
                serverToUse = availableServers.First(s => s.address.Equals(Tracker.Device));
            }

            if (serverToUse != null)
            {
                var connectionAttemptMessage = string.Format("Try connection to OWL with address {0}", serverToUse.address);

                appLog.Info(connectionAttemptMessage);
                UnityEngine.Debug.Log(connectionAttemptMessage);

                if (Tracker.Connect(serverToUse.address, Tracker.SlaveMode, Tracker.BroadcastMode))
                {
                    UnityEngine.Debug.Log(string.Format("OWL connected to {0}", serverToUse.address), this);

                    if (!Tracker.SlaveMode && CreateDefaultPointTracker)
                    {
                        CreateADefaultPointTracker();
                    }

                    if (OwlConnectedCallbacks != null)
                    {
                        OwlConnectedCallbacks.Invoke();
                    }

                    // Hint:
                    // do not forget to call StartStreaming somewhere when a tracker has been created
                }
                else
                {
                    var connectionFailedMessage = string.Format("Establishing connection to OWL with address {0} failed...", serverToUse.address);
                    appLog.Error(connectionFailedMessage);
                    UnityEngine.Debug.Log(connectionFailedMessage);

                    if (OwlConnectionFailed != null)
                        OwlConnectionFailed.Invoke();
                }
            }

        }

        public void ClearCallbacks()
        {
            OwlUpdateCallbacks = null;

            OwlConnectedCallbacks = null;
        }

        private void CreateADefaultPointTracker()
        {
            int n = 128;
            int[] leds = new int[n];
            for (int i = 0; i < n; i++)
                leds[i] = i;
            Tracker.CreatePointTracker(0, leds);
        }

        public void DisconnectFromOWLInstance()
        {
            Tracker.Disconnect();
        }

        private void PerformOwlUpdate()
        {
            if (OwlUpdateCallbacks == null)
                return;

            stopWatch.Start();

            OwlUpdateCallbacks.Invoke();

            stopWatch.Stop();

            OWLUpdateTook = stopWatch.ElapsedMilliseconds;

            stopWatch.Reset();
        }

        void FixedUpdate()
        {
            if (Tracker.Connected() && updateMoment == OWLUpdateStratgy.FixedUpdate)
            {
                PerformOwlUpdate();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Tracker.Connected() && updateMoment == OWLUpdateStratgy.Update)
                PerformOwlUpdate();
        }

        void OnPreRender()
        {
            if (Tracker.Connected() && updateMoment == OWLUpdateStratgy.OnPreRender)
                PerformOwlUpdate();
        }

        //
        void OnDestroy()
        {
            // disconnect from OWL server
            Tracker.Disconnect();
        }
    }
}