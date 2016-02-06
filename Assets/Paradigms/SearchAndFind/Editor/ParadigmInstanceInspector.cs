using UnityEngine;
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
        private List<ParadigmInstanceDefinition> availableDefinitions;
        private string configFilePathToLoad = string.Empty;
        private string configName = string.Empty;

        private bool showDependencies = false;

        public override void OnInspectorGUI()
        {
            instance = target as ParadigmController;

            availableDefinitions = new List<ParadigmInstanceDefinition>();

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

            if (instance.config == null)
            {
                EditorGUILayout.HelpBox("To Generate Instance definitions please load or generate a paradigm config!", MessageType.Info);
            }
            else
            {
                if (GUILayout.Button("Open Configuration Window", GUILayout.Height(30)))
                {
                    var window = EditorWindow.GetWindow<ConfigurationControl>();

                    window.Initialize(instance);

                    window.Show();
                }
            }


            if (GUILayout.Button("Lookup Instance definitions"))
            {
                availableDefinitions = Resources.FindObjectsOfTypeAll<ParadigmInstanceDefinition>().ToList();
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


            var pathToConfigFile = Application.dataPath + @"/" + configName + ".json";

            EditorGUILayout.BeginVertical();
            
            configFilePathToLoad = EditorGUILayout.TextField(configFilePathToLoad);

            if (GUILayout.Button("Load"))
            {
                configFilePathToLoad = EditorUtility.OpenFilePanel("Load Config", Application.dataPath, "json");

                if (configFilePathToLoad != null && configFilePathToLoad != string.Empty) { 

                    instance.config = ConfigUtil.LoadConfig<ParadigmConfiguration>(
                        new FileInfo(configFilePathToLoad), 
                        false, 
                        () => { EditorUtility.DisplayDialog("Error", "Config could not be loaded!", "Ok"); });

                    configName = Path.GetFileNameWithoutExtension(new FileInfo(configFilePathToLoad).Name);
                }
            }

            EditorGUILayout.LabelField("Config Name:");

            configName = EditorGUILayout.TextField(configName);

            EditorGUILayout.LabelField("Path to Config");
            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField(pathToConfigFile);

            var fileInfoForConfig = new FileInfo(pathToConfigFile);

            if (GUILayout.Button("Clear Config"))
            {
                DestroyImmediate(instance.config);
                instance.config = null;
                configName = string.Empty;
                GC.Collect();
            }

            if (instance.config == null &&
                File.Exists(pathToConfigFile) &&
                GUILayout.Button("Load config"))
            {
                instance.config = ConfigUtil.LoadConfig<ParadigmConfiguration>(fileInfoForConfig, true, () => { EditorUtility.DisplayDialog("Error", "Config could not be loaded!", "Ok"); });
            }

            if (GUILayout.Button("Create plain config"))
            {
                instance.config = CreateInstance<ParadigmConfiguration>();
            }

            if (instance.config != null)
            {
                instance.config.useTeleportation = EditorGUILayout.Toggle(new GUIContent("Use Teleportation", "Teleport the subject to the startpoint on trial finished."), instance.config.useTeleportation);

                instance.config.writeStatistics = EditorGUILayout.Toggle(new GUIContent("Write Statistics", "Writes a statistics file for the experiment per subject."), instance.config.writeStatistics);

                if (GUILayout.Button("Save Config"))
                {
                    ConfigUtil.SaveAsJson<ParadigmConfiguration>(fileInfoForConfig, instance.config);
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Load a Config to see available config values!", MessageType.Info);
            }


            showDependencies = EditorGUILayout.Foldout(showDependencies, new GUIContent("Dependencies", "Shows all dependencies of this ParadigmController"));

            if (showDependencies)
                base.OnInspectorGUI();

        }
    }
}
