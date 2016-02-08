﻿using UnityEngine;
using UnityEditor;
using System.Linq;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Assets.BeMoBI.Paradigms.SearchAndFind;
using Assets.BeMoBI.Scripts;

namespace Assets.Editor.BeMoBI.Paradigms.SearchAndFind
{
    [CustomEditor(typeof(ParadigmController))]
    public class ParadigmInstanceInspector : UnityEditor.Editor
    {
        private ParadigmController instance;
        private ParadigmControlWindow controlWindow;
        private List<ParadigmModel> availableDefinitions;
        private string configFilePathToLoad = string.Empty;
        private string configName = string.Empty;

        private bool showDependencies = false;

        public override void OnInspectorGUI()
        {
            instance = target as ParadigmController;

            availableDefinitions = new List<ParadigmModel>();

            if (GUILayout.Button("Open Control Window", GUILayout.Height(40)))
            {
                var existingWindow = EditorWindow.GetWindow<ParadigmControlWindow>();

                if (existingWindow != null)
                    controlWindow = existingWindow;
                else
                    controlWindow = CreateInstance<ParadigmControlWindow>();

                controlWindow.Initialize(instance);

                controlWindow.Show();
            }


            if (instance.InstanceDefinition != null && GUILayout.Button("Delete Instance Definition"))
            {
                //DestroyImmediate(instance.InstanceDefinition);
                instance.InstanceDefinition = null;
            }


            if (instance.Config == null)
            {
                EditorGUILayout.HelpBox("To Generate Instance definitions please load or generate a paradigm config!", MessageType.Info);
            }
            else
            {
                if (GUILayout.Button("Open Configuration Window", GUILayout.Height(30)))
                {
                    var window = EditorWindow.GetWindow<ParadigmModelEditor>();

                    window.Initialize(instance);

                    window.Show();
                }
            }
            
            if (GUILayout.Button("Create new default config"))
            {
                instance.PathToLoadedConfig = String.Empty;
                instance.Config = ParadigmConfiguration.GetDefault();
            }

            if (instance.Config == null)
            {
                RenderLoadConfigOption();
                return;
            }

            if (GUILayout.Button("Lookup Instance definitions"))
            {
                availableDefinitions = Resources.FindObjectsOfTypeAll<ParadigmModel>().ToList();
            }

            if (availableDefinitions.Any())
            {
                foreach (var item in availableDefinitions)
                {
                    if (GUILayout.Button(item.name))
                    {

                        instance.InstanceDefinition = item;
                    }

                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical();
           
            EditorGUILayout.LabelField("Config Name:");

            if (configName == string.Empty && instance.PathToLoadedConfig != string.Empty)
                configName = Path.GetFileNameWithoutExtension(instance.PathToLoadedConfig);

            configName = EditorGUILayout.TextField(configName);

            EditorGUILayout.LabelField("Path to Config");

            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField(configFilePathToLoad);

            EditorGUILayout.BeginHorizontal();

            if(configName != String.Empty)
            {
                EditorGUILayout.BeginVertical();
                RenderLoadAndSaveOptions();
                EditorGUILayout.EndVertical();
            }

            if (instance.PathToLoadedConfig != String.Empty)
            {
                EditorGUILayout.BeginVertical();
                RenderOverrideAndReloadOptions();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Clear Config"))
            {
                DestroyImmediate(instance.Config);
                instance.Config = null;
                instance.PathToLoadedConfig = string.Empty;
                configName = string.Empty;
                GC.Collect();
            }
                         
            if (instance.Config == null)
            {
                EditorGUILayout.HelpBox("Load a Config to see available config values!", MessageType.Info);
            }
            else
            {
                instance.Config.writeStatistics = EditorGUILayout.Toggle(new GUIContent("Write Statistics", "Writes a statistics file for the experiment per subject."), instance.Config.writeStatistics);
            }
             
            showDependencies = EditorGUILayout.Foldout(showDependencies, new GUIContent("Dependencies", "Shows all dependencies of this ParadigmController"));

            if (showDependencies)
                base.OnInspectorGUI();

        }

        private void RenderLoadAndSaveOptions()
        {
            var pathToConfigFile = Application.dataPath + @"/" + configName + ".json";

            var fileInfoForConfig = new FileInfo(pathToConfigFile);

            if (fileInfoForConfig.Exists &&
                GUILayout.Button("Load config"))
            {
                instance.Config = ConfigUtil.LoadConfig<ParadigmConfiguration>(fileInfoForConfig, true, () => { EditorUtility.DisplayDialog("Error", "Config could not be loaded!", "Ok"); });

                if (instance.Config != null)
                    instance.PathToLoadedConfig = pathToConfigFile;
            }

            if (GUILayout.Button("Save Config"))
            {
                ConfigUtil.SaveAsJson<ParadigmConfiguration>(fileInfoForConfig, instance.Config);

                instance.PathToLoadedConfig = fileInfoForConfig.FullName;

                AssetDatabase.Refresh();
            }
        }

        private void RenderOverrideAndReloadOptions()
        {
            var fileInfoForConfig = new FileInfo(instance.PathToLoadedConfig);

            if (fileInfoForConfig.Exists &&
                GUILayout.Button("Reload config"))
            {
                instance.Config = ConfigUtil.LoadConfig<ParadigmConfiguration>(fileInfoForConfig, true, () => { EditorUtility.DisplayDialog("Error", "Config could not be loaded!", "Ok"); });

                if (instance.Config != null)
                    instance.PathToLoadedConfig = fileInfoForConfig.FullName;
            }

            if (fileInfoForConfig.Exists && GUILayout.Button("Override Config"))
            {
                ConfigUtil.SaveAsJson<ParadigmConfiguration>(fileInfoForConfig, instance.Config);
                AssetDatabase.Refresh();
            }
        }

        private void RenderLoadConfigOption()
        {
            configFilePathToLoad = EditorGUILayout.TextField(configFilePathToLoad);

            if (GUILayout.Button("Load"))
            {
                configFilePathToLoad = EditorUtility.OpenFilePanel("Load Config", Application.dataPath, "json");

                if (configFilePathToLoad != null && configFilePathToLoad != string.Empty)
                {

                    instance.Config = ConfigUtil.LoadConfig<ParadigmConfiguration>(
                        new FileInfo(configFilePathToLoad),
                        false,
                        () => { EditorUtility.DisplayDialog("Error", "Config could not be loaded!", "Ok"); });

                    configName = Path.GetFileNameWithoutExtension(new FileInfo(configFilePathToLoad).Name);
                }
            }

        }
    }
}
