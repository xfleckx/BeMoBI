using UnityEngine;
using System.Collections;
using UnityEditor;
using PhaseSpace;
using System.IO;
using System;

/// <summary>
/// Throw away code... just for experiments
/// </summary>
public class TestEditorUI : EditorWindow {


    [MenuItem("TESTING/OWL")]
    public static void ShowWindow()
    {
        var window = GetWindow<TestEditorUI>();

            window.Show();
    }

    private OWLTracker tracker;

    private OWLTestUpdater updater;
    private bool autoConnectAndStart = true;
    public PSMoCapConfig config;

    Action performAutoStart;

    private void OnDestroy()
    { 
        EditorApplication.playmodeStateChanged -= OnPlayModeChange;
    }

    private void OnDisable()
    {
        EditorApplication.playmodeStateChanged -= OnPlayModeChange;
    }

    private void OnEnable()
    {
        EditorApplication.playmodeStateChanged += OnPlayModeChange;
    }
    private Vector2 scrollPosition;

    private void OnGUI()
    {
        if(config == null)
        {
            CreateConfig();
        }

        if (autoConnectAndStart && performAutoStart == null)
        {
            performAutoStart += new Action(OnAutoStart);
            Debug.Log("Auto Connect procedure attached...");
        }

        if(!autoConnectAndStart && performAutoStart != null)
        {
            performAutoStart = null;
            Debug.Log("Auto Connect procedure detached");
        }

        if(tracker == null)
            tracker = FindObjectOfType<OWLTracker>();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (tracker == null) { 
            EditorGUILayout.HelpBox("No OWLTracker Instance found in current Scene", MessageType.Error);
            return;
        }

        autoConnectAndStart = EditorGUILayout.Toggle("Auto Start on Play", autoConnectAndStart);

        config.Mode = EditorGUILayout.IntField("Set Mode before connecting: ", config.Mode);

        if (GUILayout.Button("Connect"))
        {
            if (tracker != null)
                ConnectToTracker();
        }
         
        if (GUILayout.Button("Load RB File"))
        {
           config.lastRBFile = EditorUtility.OpenFilePanel("RB file", Application.dataPath, "rb");
        }

        EditorGUILayout.LabelField(config.lastRBFile);

        if (trackerCanBeConfigured())
        {
            config.lastRigidID = EditorGUILayout.IntField("Rigid ID", config.lastRigidID);
            if (GUILayout.Button("Create Rigid Body"))
            {
                if (rbFileIsValid()) 
                        tracker.CreateRigidTracker(config.lastRigidID, config.lastRBFile); 
            }
            
            if (isNotStreaming()&& GUILayout.Button("Start Streaming"))
            {
                tracker.StartStreaming();
            }
            
            if (GUILayout.Button("Disconnect"))
            {
                if (tracker != null)
                    tracker.Disconnect();
            }
        }
        else if(isStreaming())
        {
            EditorGUILayout.HelpBox("OWL is streaming. Config not allowed!", MessageType.Info);
        }

        EditorGUILayout.LabelField("Rotation Offsets for Tracking correction:");
        config.trackingSpaceCorrection = EditorGUILayout.Vector3Field("", config.trackingSpaceCorrection);

        if(GUILayout.Button("Set Tracking Space Correction (Euler Angles)"))
        {
            SetTrackingCorrection();
        }

        EditorGUILayout.Space();

        if (isStreaming())
        {
            if(updater == null) 
                updater = FindObjectOfType<OWLTestUpdater>();
            
            if(updater != null)
            {
                updater.showCameras =  GUILayout.Toggle(updater.showCameras, "Show Cameras", "Button");
                updater.showMarker =  GUILayout.Toggle(updater.showMarker, "Show Markers", "Button");
                updater.showRigids =  GUILayout.Toggle(updater.showRigids, "Show Rigids", "Button");
            }
        }

        EditorGUILayout.Space();

        if (OVRManager.instance != null)
        {
            if(GUILayout.Button("Reset Rift"))
            {
                OVRManager.display.RecenterPose();
            }
        }else
        {
            EditorGUILayout.HelpBox("No Oculus component found in the Scene!", MessageType.Info);
        }

        config = EditorGUILayout.ObjectField(config, typeof(PSMoCapConfig), false) as PSMoCapConfig;

        config.name = EditorGUILayout.TextField("Config Name", config.name);
        EditorGUILayout.LabelField("Description");

        config.description = EditorGUILayout.TextArea(config.description);

        if (GUILayout.Button("Save Config as Asset"))
        {
            var path = EditorUtility.SaveFilePanelInProject("Save MoCapConfig", config.name, "asset", "Save the current MoCap config as asset");
            AssetDatabase.CreateAsset(config, path);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        if (GUILayout.Button("Delete Config"))
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(config));
            ScriptableObject.DestroyImmediate(config, true);
            config = null;
            AssetDatabase.Refresh();
        }
        EditorGUILayout.EndScrollView();
    }

    private void SetTrackingCorrection()
    {
        var ovrCamRig = FindObjectOfType<OVRCameraRig>();
        ovrCamRig.transform.Rotate(config.trackingSpaceCorrection);
    }

    private void ConnectToTracker()
    {
        tracker.mode = config.Mode;
        tracker.Connect();
    }

    private void CreateConfig()
    {
       config = PSMoCapConfig.CreateInstance<PSMoCapConfig>();
        config.name = "mocap_" + DateTime.Now.ToString("MM-dd-yy_hh-mm-ss");
    }

    private void OnPlayModeChange()
    { 
        if (EditorApplication.isPlayingOrWillChangePlaymode && performAutoStart != null)
        {
            EditorApplication.delayCall += () =>
            {
                // call whatever is placed in the callback
                performAutoStart();
                // clean the callback so no auto start logic get`s called twice
                performAutoStart = null;
            };
        }

        Repaint();
    }

    private void OnAutoStart()
    {
        if (rbFileIsValid())
        {
            Debug.Log("Auto Start with rb File: " + Path.GetFileName(config.lastRBFile));
            ConnectToTracker();
            tracker.CreateRigidTracker(config.lastRigidID, config.lastRBFile);
            tracker.StartStreaming();
            SetTrackingCorrection();
        }
    }

    private bool rbFileIsValid()
    {
        return config.lastRBFile != string.Empty && File.Exists(config.lastRBFile);
    }

    private bool isStreaming()
    {
        return tracker != null &&  tracker.CurrentFPS != 0;
    }

    private bool isNotStreaming()
    {
        return tracker != null && tracker.Connected() && tracker.CurrentFPS == 0;
    }

    private bool trackerCanBeConfigured()
    {
        return tracker != null && isNotStreaming();
    }
    
    [DrawGizmo(GizmoType.Active | GizmoType.NotInSelectionHierarchy, typeof(FreeFlyRiftPSRigid))]
    private static void DrawGizmosForFreeFly(FreeFlyRiftPSRigid instance, GizmoType type)
    {
        var cam = instance.GetComponent<UnityEngine.Camera>();
        var temp = Gizmos.matrix;
        Gizmos.matrix = instance.transform.localToWorldMatrix;
        Gizmos.DrawFrustum(instance.transform.position, cam.fieldOfView, cam.farClipPlane* 0.001f, cam.nearClipPlane, cam.aspect);
        Gizmos.matrix = temp;
        Gizmos.DrawRay(instance.transform.position, instance.transform.forward);
    } 
}
