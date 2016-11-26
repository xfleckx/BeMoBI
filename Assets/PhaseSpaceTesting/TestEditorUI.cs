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

    private void OnGUI()
    {
        if(tracker == null)
            tracker = FindObjectOfType<OWLTracker>();

        if (tracker == null) { 
            EditorGUILayout.HelpBox("No OWLTracker Instance found in current Scene", MessageType.Error);
            return;
        }

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
                EditorGUILayout.LabelField(lastRBFile);
                if (lastRBFile != string.Empty && File.Exists(lastRBFile))
                    tracker.CreateRigidTracker(lastRigidID, lastRBFile);
            }

            if (isNotStreaming() && GUILayout.Button("Start Streaming"))
            {
                tracker.StartStreaming();
            }
            
            if (GUILayout.Button("Disconnect"))
            {
                if (tracker != null)
                    tracker.Disconnect();
            }
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
}
