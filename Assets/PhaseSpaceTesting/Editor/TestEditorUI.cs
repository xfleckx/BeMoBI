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
    private int lastRigidID = 0;
    private string lastRBFile;
    private OWLTestUpdater updater;
    private bool autoConnectAndStart = true;
    
    Action performAutoStart;

    private void OnGUI()
    {
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

        if (tracker == null) { 
            EditorGUILayout.HelpBox("No OWLTracker Instance found in current Scene", MessageType.Error);
            return;
        }

        autoConnectAndStart = EditorGUILayout.Toggle("Auto Start on Play", autoConnectAndStart);

        tracker.mode = EditorGUILayout.IntField("Choose Mode before connecting: ", tracker.mode);

        if (GUILayout.Button("Connect"))
        {
            if (tracker != null)
                tracker.Connect();
        }

        if (trackerCanBeConfigured())
        {
            lastRigidID = EditorGUILayout.IntField("Rigid ID", lastRigidID);

            if (isNotStreaming() && GUILayout.Button("Load RB File"))
            {
                lastRBFile = EditorUtility.OpenFilePanel("RB file", Application.dataPath, "rb");
                
            }

            EditorGUILayout.LabelField(lastRBFile);

            if(GUILayout.Button("Create Rigid Body"))
            {

                if (lastRBFile != String.Empty)
                {
                    if (lastRBFile != string.Empty && File.Exists(lastRBFile))
                        tracker.CreateRigidTracker(lastRigidID, lastRBFile);
                }
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

        if(GUILayout.Button("Reset Rift"))
        {
            OVRManager.display.RecenterPose();
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

    }

    private void OnAutoStart()
    {
        if (lastRBFile != string.Empty && File.Exists(lastRBFile))
        {
            Debug.Log("Auto Start with rb File:: " + Path.GetFileName(lastRBFile));
            tracker.Connect();
            tracker.CreateRigidTracker(lastRigidID, lastRBFile);
            tracker.StartStreaming();
        }
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
        return tracker != null && tracker.Connected();
    }

    private void Update()
    {
        if (tracker != null && EditorApplication.isPlayingOrWillChangePlaymode && performAutoStart != null)
        {
            // call whatever is placed in the callback
            performAutoStart();
            // clean the callback so no auto start logic get`s called twice
            performAutoStart = null;
        }
    }



}
