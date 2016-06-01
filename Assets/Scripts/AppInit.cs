using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;
using NLog;
using NLog.Config;
using Debug = UnityEngine.Debug;
using CommandLine;
using System.IO;
using Assets.BeMoBI.Scripts;

public class AppInit : MonoBehaviour {

    NLog.Logger appLog = NLog.LogManager.GetLogger("App");

    public const string DEFINTION_DIR_NAME = "PreDefinitions";
    
    public DirectoryInfo DirectoryForInstanceDefinitions;

    public AppConfig appConfig;

    public Action<AppConfig> OnAppConfigLoaded;

    // Use this for initialization
    void Start () {

        var pathToInstanceDefinitions = Application.dataPath + @"\" + DEFINTION_DIR_NAME;

        if (!Directory.Exists(pathToInstanceDefinitions))
        {
            DirectoryForInstanceDefinitions = Directory.CreateDirectory(pathToInstanceDefinitions);
        }
        else
        {
            DirectoryForInstanceDefinitions = new DirectoryInfo(pathToInstanceDefinitions);
        }
        
        var args = Environment.GetCommandLineArgs();

        options = new StartUpOptions();
        
        hasOptions = Parser.Default.ParseArguments( args, options );
        
        var stopWatch = new Stopwatch();

        stopWatch.Start();

        LogManager.Configuration = new XmlLoggingConfiguration(Application.dataPath + @"\NLog.config");

        UpdateLoggingConfiguration();

        stopWatch.Stop();

        Debug.Log(string.Format("### Runtime ### Nlog config lookup took: {0}", stopWatch.Elapsed));

        appLog.Info(string.Format("Starting with Args: {0} {1} {2} {3}", options.subjectId, options.fileNameOfCustomConfig, options.fileNameOfParadigmDefinition, options.condition));

        var expectedAppConfig = Path.Combine(Application.dataPath, options.fileNameAppConfig);

        appConfig = ConfigUtil.LoadConfig<AppConfig>(new FileInfo(expectedAppConfig), true, () => {
            appLog.Fatal("Something is wrong with the AppConfig. Was not found and I was not able to create one!");
        });
        
        if (OnAppConfigLoaded != null)
            OnAppConfigLoaded(appConfig);

        var currentLevelIndex = QualitySettings.GetQualityLevel();
        var allLevels = QualitySettings.names;
        var currentLevel = allLevels[currentLevelIndex];

        appLog.Info("Using quality level " + currentLevel);
    }
    
    private bool hasOptions;
    public bool HasOptions
    {
        get
        {
            return hasOptions;
        }
    }
    
    private StartUpOptions options;
    public StartUpOptions Options
    {
        get
        {
            return options;
        }
    }

    public void UpdateLoggingConfiguration()
    {
        LogManager.ReconfigExistingLoggers();
    }
}

/// <summary>
/// See usage scenarios
/// https://github.com/gsscoder/commandline/wiki/Latest-Version
/// </summary>
public class StartUpOptions
{
    [Option('s', "subject", DefaultValue = "", Required = true, HelpText = "Subject Identification - should be a unique string!")]
    public string subjectId { get; set; }
    
    [Option('t', "condition", DefaultValue = "", HelpText = "A short description or name of a global condition for this instance", Required = false)]
    public string condition { get; set; }
    
    [Option('d', "paradigmDefinition", DefaultValue = "", HelpText = "A file name of a paradigm definition" , Required = false)]
    public string fileNameOfParadigmDefinition { get; set; }

    [Option('c', "config", DefaultValue = "", HelpText = "A file name of customn config file", Required = false)]
    public string fileNameOfCustomConfig { get; set; }


    [Option('a', "appConfig", DefaultValue = "AppConfig.json", HelpText = "A file name of app config file", Required = false)]
    public string fileNameAppConfig { get; set; }
    
}