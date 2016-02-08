using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using NLog;
using NLogger = NLog.Logger;
using Assets.BeMoBI.Paradigms.SearchAndFind;

namespace Assets.Editor.BeMoBI.Paradigms.SearchAndFind
{
    public class ConfigurationControl : EditorWindow
    {
        NLogger log = LogManager.GetCurrentClassLogger();

        private ParadigmController instance;

        private ParadigmModelFactory factory;

        public String PreDefinedSubjectID = "TestSubject";

        private bool previewDefinition;

        private ParadigmModel lastGeneratedInstanceDefinition;

        internal void Initialize(ParadigmController target)
        {
            instance = target;

            titleContent = new GUIContent("Paradigm Control");

            log.Info("Initialize Paradigma Control Window");
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            if (instance == null && (instance = TryGetInstance()) == null)
            {
                EditorGUILayout.HelpBox("No Paradigm Controller available! \n Open another scene or create a paradigm controller instance!", MessageType.Info);
                return;
            }

            if (instance != null && instance.Config == null)
            {
                EditorGUILayout.HelpBox("No Configuration at the paradigm controller available! \n Loard or create one!", MessageType.Info);
                return;
            }


            if (factory == null)
            {
                factory = new ParadigmModelFactory();
                factory.config = instance.Config;
            }

            EditorGUILayout.BeginHorizontal(GUILayout.Width(400));

            EditorGUILayout.BeginVertical();

            RenderRunVariables();

            RenderConfigurationGUI();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            RenderPreviewGUI();

            EditorGUILayout.EndVertical();
        }

        private ParadigmController TryGetInstance()
        {
            return FindObjectOfType<ParadigmController>();
        }

        private string FormatSurveyRequest()
        {
            return @"http:\\localhost\limesurvey\index.php\197498?lang=en" + "?subject=test?pose=bla";
        }

        private void RenderRunVariables()
        {
            EditorGUILayout.BeginVertical();

            GUILayout.Label("Subject ID:");
            PreDefinedSubjectID = EditorGUILayout.TextField(PreDefinedSubjectID);

            EditorGUILayout.EndVertical();
        }

        private void InjectCmdArgs()
        {
            instance.SubjectID = PreDefinedSubjectID;
        }

        private void RenderConfigurationGUI()
        {
            GUILayout.Label("Configuration", EditorStyles.largeLabel);

            if (factory.config == null)
            {
                EditorGUILayout.HelpBox("Please Load or generate a Paradigm configuration first!", MessageType.Info);

                return;
            }

            foreach (var config in factory.config.conditionConfigurations)
            {
                config.useExactOnCategoryPerMaze = EditorGUILayout.Toggle(
                    new GUIContent("Use category exclusive", "A category will never be shared within multiple mazes"),
                    config.useExactOnCategoryPerMaze);

                config.groupByMazes = EditorGUILayout.Toggle(
                    new GUIContent("Group by Mazes and Paths", "Trials are set as tuples of training and experiment trials per Maze and Path"),
                    config.groupByMazes);

                if (lastGeneratedInstanceDefinition == null)
                    EditorGUILayout.HelpBox("Try \"Find Possible Configuration\" ", MessageType.Info);

                if (GUILayout.Button(new GUIContent("Find Possible Configuration", "Search the current Scene for all necessary elements!")))
                    factory.EstimateConfigBasedOnAvailableElements();

                config.mazesToUse = EditorGUILayout.IntField("Mazes", config.mazesToUse);

                //config.atLeastAvailblePathsPerMaze = EditorGUILayout.IntField("Common available paths", factory.atLeastAvailblePathsPerMaze);

                config.pathsToUsePerMaze = EditorGUILayout.IntField("Use Paths per Maze", config.pathsToUsePerMaze);
                 
                if (!config.useExactOnCategoryPerMaze)
                {
                    config.categoriesPerMaze = EditorGUILayout.IntField(
                        new GUIContent("Categories per Maze", "Declares the amount of categories \n from which objects are choosen."),
                        config.categoriesPerMaze);
                }

                EditorGUILayout.HelpBox("Remember that only one path per maze per object is allowed", MessageType.Info);

                EditorGUILayout.LabelField("Count of object visitations");

                 config.objectVisitationsInTraining = EditorGUILayout.IntField("Training",  config.objectVisitationsInTraining);

                 config.objectVisitationsInExperiment = EditorGUILayout.IntField("Experiment", config.objectVisitationsInExperiment);

            }


            if (GUILayout.Button("Generate Instance Definition", GUILayout.Height(35)))
            {
                if (instance.SubjectID == null)
                {
                    instance.SubjectID = this.PreDefinedSubjectID;
                }

                lastGeneratedInstanceDefinition = factory.Generate(this.PreDefinedSubjectID, factory.config.expectedConditions, factory.config.conditionConfigurations);
                lastGeneratedInstanceDefinition.Configuration = instance.Config;
            }

            lastGeneratedInstanceDefinition = EditorGUILayout.ObjectField("Last Generated Definion", lastGeneratedInstanceDefinition, typeof(ParadigmModel), false) as ParadigmModel;

            if (lastGeneratedInstanceDefinition == null)
                return;

            previewDefinition = EditorGUILayout.Toggle("Show definition", previewDefinition);

            if (GUILayout.Button("Save Instance Definition"))
            {
                var fileNameWoExt = string.Format("Assets/Paradigms/SearchAndFind/PreDefinitions/VP_{0}_Definition", lastGeneratedInstanceDefinition.Subject);

                var jsonString = JsonUtility.ToJson(lastGeneratedInstanceDefinition, true);

                AssetDatabase.CreateAsset(lastGeneratedInstanceDefinition, fileNameWoExt + ".asset");

                using (var file = new StreamWriter(fileNameWoExt + ".json"))
                {
                    file.Write(jsonString);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Use this definition"))
            {
                instance.InstanceDefinition = lastGeneratedInstanceDefinition;
            }

        }

        private void RenderPreviewGUI()
        {
            if (lastGeneratedInstanceDefinition == null)
                return;

            EditorGUILayout.LabelField("Preview:");

            configPreviewScrollState = EditorGUILayout.BeginScrollView(configPreviewScrollState);

            foreach (var condDef in lastGeneratedInstanceDefinition.Conditions)
            {
                EditorGUILayout.LabelField(string.Format("Condition {0} with {1} Trials", condDef.Identifier, condDef.Trials));

                if (condDef.Trials != null && previewDefinition)
                {
                    string lastMazeName = string.Empty;
                    foreach (var tdef in condDef.Trials)
                    {
                        if (!lastMazeName.Equals(tdef.MazeName))
                            EditorGUILayout.Space();

                        EditorGUILayout.LabelField(
                            string.Format(DEFINITION_PREVIEW_PATTERN,
                            tdef.TrialType,
                            tdef.MazeName,
                            tdef.Path,
                            tdef.ObjectName,
                            tdef.Category));
                        lastMazeName = tdef.MazeName;
                    }
                } 
            }

            EditorGUILayout.EndScrollView();
        }

        const string DEFINITION_PREVIEW_PATTERN = "{0}: {1} -> {2} = {3} from {4}";
        private Vector2 configPreviewScrollState;

    }
}