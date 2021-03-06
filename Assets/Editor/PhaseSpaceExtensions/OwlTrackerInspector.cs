﻿using System;
using UnityEngine;
using UnityEditor;
using Assets.BeMoBI.Scripts.PhaseSpaceExtensions;
using System.IO;

namespace Assets.BeMoBI.EditorExtensions.PhaseSpaceExtensions
{
    [CustomEditor(typeof(OWLInterface))]
    public class OwlTrackerInspector : Editor
    {
        private OWLInterface instance;

        private Ping ping;
        private float lastPingTime;

        public override void OnInspectorGUI()
        {
            instance = target as OWLInterface;

            var currentConfigPathNotValid = !File.Exists(instance.configFilePath);

            if (instance.configFilePath == null || instance.configFilePath.Equals("") || currentConfigPathNotValid)
            {
                instance.createDefaultConfigFilePath();
            }

            base.OnInspectorGUI();


            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Load Config"))
            {
                var choosenFilePath = EditorUtility.OpenFilePanel("Load Config", Application.dataPath, "json");

                if(choosenFilePath != null || choosenFilePath == "") {

                    instance.configFilePath = String.Empty; 
                    
                    DestroyImmediate(instance.config);

                    instance.configFilePath = choosenFilePath.Replace(Application.dataPath, @"Assets");

                    instance.LoadOrUseDefaultConfig();
                }
            }

            if (GUILayout.Button("Save Config"))
            {
                var fullPath = EditorUtility.SaveFilePanel("Save Config", Application.dataPath, OWLInterface.DEFAULT_CONFIG_NAME, "json");

                instance.configFilePath = fullPath.Replace(Application.dataPath, "Assets");
                
                instance.SaveConfig();

                AssetDatabase.Refresh();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Connect"))
            {
                instance.ConnectToOWLInstance();
            }

            if (GUILayout.Button("Disconnect"))
            {
                instance.DisconnectFromOWLInstance();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Ping"))
            {
                ping = new Ping(instance.Tracker.Device);
            }

            GUILayout.Label(string.Format("Ping: {0} ms", lastPingTime), EditorStyles.boldLabel);

            if (ping != null && ping.isDone)
            {
                lastPingTime = ping.time;
                ping.DestroyPing();
            }

            EditorGUILayout.EndVertical();



            // need to be merged into the CustomInspector
            //void OnGUI()
            //{
            //    if (!showDeprecatedOnGUI)
            //        return;

            //    GUILayout.BeginArea(new Rect(8, 8, Screen.width - 16, Screen.height / 4 + 4));
            //    GUILayout.BeginHorizontal();
            //    GUILayout.Label("Device", GUILayout.ExpandWidth(false));

            //    // disable controls if connected already
            //    if (connected) GUI.enabled = false;

            //    // reenable controls
            //    GUI.enabled = true;

            //    if (connected)
            //    {
            //        if (GUILayout.Button("Disconnect", GUILayout.ExpandWidth(false)))
            //            OWL.Disconnect();
            //    }
            //    else
            //    {
            //        OWLHost = GUILayout.TextField(OWLHost, GUILayout.Width(250));

            //        if (GUILayout.Button("Connect", GUILayout.ExpandWidth(false)))
            //        {
            //            ConnectToOWLInstance();

            //            connected = OWL.Connected();

            //            if (connected)
            //                PlayerPrefs.SetString("OWLHost", OWLHost);
            //        }
            //    }
            //    GUILayout.EndHorizontal();

            //    // display avgCondition message or current frame number
            //    if (OWL.error != 0)
            //    {
            //        message = String.Format("owl message: 0x{0,0:X}", OWL.error);
            //    }
            //    else
            //    {
            //        message = String.Format("frame = {0}, m = {1}, r = {2}, c = {3}", OWL.frame, OWL.NumMarkers, OWL.NumRigids, OWL.NumCameras);
            //    }

            //    GUILayout.Label(message);

            //    GUILayout.EndArea();
            //}
        }

    }
}