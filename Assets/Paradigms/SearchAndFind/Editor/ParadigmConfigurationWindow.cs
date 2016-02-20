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
using Assets.BeMoBI.Scripts;

namespace Assets.Editor.BeMoBI.Paradigms.SearchAndFind
{
    public class ParadigmModelEditor : EditorWindow
    {
        NLogger log = LogManager.GetCurrentClassLogger();

        private ParadigmController instance;

        private ParadigmModelFactory factory;

        public String PreDefinedSubjectID = "TestSubject";

        public Action OnPostOnGUICommands;

        private bool previewDefinition;

        [SerializeField]
        private ParadigmModel lastGeneratedInstanceDefinition;

        internal void Initialize(ParadigmController target)
        {
            instance = target;

            titleContent = new GUIContent("Paradigm Control");

            log.Info("Initialize Paradigma Control Window");
        }

        private int indexOfSelectedCondition = 0;

        private ConditionConfiguration selectedConfiguration;

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

            EditorGUILayout.LabelField("Configure a condition configuration", EditorStyles.largeLabel);

            EditorGUILayout.BeginHorizontal();

            var conditionNames = instance.Config.conditionConfigurations.Select(cc => cc.ConditionID).ToArray();

            if( conditionNames.Count() > indexOfSelectedCondition) { 

                indexOfSelectedCondition = EditorGUILayout.Popup(indexOfSelectedCondition, conditionNames);

                selectedConfiguration = instance.Config.conditionConfigurations[indexOfSelectedCondition];

                if (GUILayout.Button("Copy"))
                {
                    OnPostOnGUICommands += () =>
                    {
                        var cloneOfSelectedConfig = selectedConfiguration.Clone() as ConditionConfiguration;
                        cloneOfSelectedConfig.ConditionID = cloneOfSelectedConfig.ConditionID + "_copy";
                        instance.Config.conditionConfigurations.Add(cloneOfSelectedConfig); 
                    };
                }

                if (GUILayout.Button("Remove"))
                {
                    OnPostOnGUICommands += () =>
                    {
                        instance.Config.conditionConfigurations.Remove(selectedConfiguration);
                        selectedConfiguration = null;

                        var middleIndex = instance.Config.conditionConfigurations.Count / 2;

                        if (indexOfSelectedCondition > middleIndex)
                            indexOfSelectedCondition -= 1;

                    };
                }

                RenderAddDefaultConfigOption();

                EditorGUILayout.EndHorizontal();
                
                selectedConfiguration.ConditionID = EditorGUILayout.TextField("Name of Condition Configuration:", selectedConfiguration.ConditionID);

                EditorGUILayout.BeginHorizontal(GUILayout.Width(400));

                EditorGUILayout.BeginVertical();

                RenderConfigurationOptions();

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                RenderInstanceDefinitionOptions();

                RenderPreviewGUI();

                EditorGUILayout.EndVertical();
                
            }
            else
            {
                EditorGUILayout.HelpBox("No Conditions available", MessageType.Info);
                RenderAddDefaultConfigOption();
            }

            if(OnPostOnGUICommands != null)
            {
                OnPostOnGUICommands();
                OnPostOnGUICommands = null;
            }
        }

        private void RenderAddDefaultConfigOption()
        {
            if (GUILayout.Button("Add default"))
            {
                OnPostOnGUICommands += () =>
                {
                    var newDefaultCondition = ConditionConfiguration.GetDefault();
                    instance.Config.conditionConfigurations.Add(newDefaultCondition);
                };
            }
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

        private void RenderConfigurationOptions()
        {
            if (factory.config == null)
            {
                EditorGUILayout.HelpBox("Please Load or generate a Paradigm configuration first!", MessageType.Info);

                return;
            }

            EditorGUILayout.LabelField(new GUIContent("Expetected Controller Names", "See VR Subject Controller for possible options"), EditorStyles.largeLabel);

            selectedConfiguration.HeadControllerName = EditorGUILayout.TextField("Head Controller", selectedConfiguration.HeadControllerName);

            selectedConfiguration.BodyControllerName = EditorGUILayout.TextField("Body Controller", selectedConfiguration.BodyControllerName);

            selectedConfiguration.useExactOnCategoryPerMaze = EditorGUILayout.Toggle(
                new GUIContent("Use category exclusive", "A category will never be shared within multiple mazes"),
                selectedConfiguration.useExactOnCategoryPerMaze);

            selectedConfiguration.groupByMazes = EditorGUILayout.Toggle(
                new GUIContent("Group by Mazes and Paths", "Trials are set as tuples of training and experiment trials per Maze and Path"),
                selectedConfiguration.groupByMazes);

            selectedConfiguration.useTeleportation = EditorGUILayout.Toggle(
                new GUIContent("use Teleportaiton","Use Teleportation to bring the subject back to start after Trial ends"),
                selectedConfiguration.useTeleportation);

            if (lastGeneratedInstanceDefinition == null)
                EditorGUILayout.HelpBox("Try \"Find Possible Configuration\" ", MessageType.Info);

            if (GUILayout.Button(new GUIContent("Find Possible Configuration", "Search the current Scene for all necessary elements!")))
                factory.EstimateConfigBasedOnAvailableElements();

            selectedConfiguration.mazesToUse = EditorGUILayout.IntField("Mazes", selectedConfiguration.mazesToUse);

            //config.atLeastAvailblePathsPerMaze = EditorGUILayout.IntField("Common available paths", factory.atLeastAvailblePathsPerMaze);

            selectedConfiguration.pathsToUsePerMaze = EditorGUILayout.IntField("Use Paths per Maze", selectedConfiguration.pathsToUsePerMaze);

            if (!selectedConfiguration.useExactOnCategoryPerMaze)
            {
                selectedConfiguration.categoriesPerMaze = EditorGUILayout.IntField(
                    new GUIContent("Categories per Maze", "Declares the amount of categories \n from which objects are choosen."),
                    selectedConfiguration.categoriesPerMaze);
            }

            EditorGUILayout.HelpBox("Remember that only one path per maze per object is allowed", MessageType.Info);

            EditorGUILayout.LabelField("Count of object visitations");

            selectedConfiguration.objectVisitationsInTraining = EditorGUILayout.IntField("Training", selectedConfiguration.objectVisitationsInTraining);

            selectedConfiguration.objectVisitationsInExperiment = EditorGUILayout.IntField("Experiment", selectedConfiguration.objectVisitationsInExperiment);

            //if(GUILayout.Button("Save Configuration"))
            //{
            //    ConfigUtil.SaveAsJson<ParadigmConfiguration>(new FileInfo(instance.PathToLoadedConfig), instance.Config);
            //}

        }

        private void RenderInstanceDefinitionOptions()
        {

            EditorGUILayout.LabelField("Predefine a Instance Definition:", EditorStyles.boldLabel);

            RenderRunVariables();

            if (GUILayout.Button("Generate Instance Definition", GUILayout.Height(35)))
            {
                if (instance.SubjectID == null)
                {
                    instance.SubjectID = this.PreDefinedSubjectID;
                }

                lastGeneratedInstanceDefinition = factory.Generate(this.PreDefinedSubjectID, factory.config.conditionConfigurations);
                lastGeneratedInstanceDefinition.Configuration = instance.Config;
            }

            lastGeneratedInstanceDefinition = EditorGUILayout.ObjectField("Last Generated Definion", lastGeneratedInstanceDefinition, typeof(ParadigmModel), false) as ParadigmModel;

            if (lastGeneratedInstanceDefinition == null)
            {
                lastGeneratedInstanceDefinition = instance.InstanceDefinition;
            }

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

        int indexOfSelectedDefintionPreview = 0;
        ConditionDefinition selectedConditionForPreview;

        private void RenderPreviewGUI()
        {
            if (lastGeneratedInstanceDefinition == null)
                return;

            previewDefinition = EditorGUILayout.Toggle("Show definition", previewDefinition);

            EditorGUILayout.LabelField("Preview:");
            
            var definitionNames = lastGeneratedInstanceDefinition.Conditions.Select(cc => cc.Identifier).ToArray();

            indexOfSelectedDefintionPreview = EditorGUILayout.Popup(indexOfSelectedDefintionPreview, definitionNames);

            selectedConditionForPreview = lastGeneratedInstanceDefinition.Conditions[indexOfSelectedDefintionPreview];
            
            EditorGUILayout.LabelField(string.Format("Condition {0} with {1} Trials", selectedConditionForPreview.Identifier, selectedConditionForPreview.Trials.Count));
            
            configPreviewScrollState = EditorGUILayout.BeginScrollView(configPreviewScrollState);

            if (selectedConditionForPreview.Trials != null && previewDefinition)
            {
                string lastMazeName = string.Empty;

                foreach (var tdef in selectedConditionForPreview.Trials)
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

            EditorGUILayout.EndScrollView();
        }

        const string DEFINITION_PREVIEW_PATTERN = "{0}: {1} -> {2} = {3} from {4}";
        private Vector2 configPreviewScrollState;

    }
}