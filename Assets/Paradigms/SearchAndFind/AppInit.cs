﻿using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;
using NLog;
using NLog.Config;
using Debug = UnityEngine.Debug;
using CommandLine;

public class AppInit : MonoBehaviour {

    NLog.Logger log = NLog.LogManager.GetLogger("App");

	// Use this for initialization
	void Start () {

        var args = Environment.GetCommandLineArgs();

        options = new StartUpOptions();

        hasOptions = Parser.Default.ParseArguments( args, options );

        var stopWatch = new Stopwatch();

        stopWatch.Start();

        LogManager.Configuration = new XmlLoggingConfiguration(Application.dataPath + @"\NLog.config");

        LogManager.ReconfigExistingLoggers();

        stopWatch.Stop();

        Debug.Log(string.Format("### Runtime ### Nlog config lookup took: {0}", stopWatch.Elapsed));

        log.Info(string.Format("Starting with Args: {0} {1} {2} {3}", options.subjectId, options.fileNameOfCustomConfig, options.fileNameOfParadigmDefinition, options.condition));
         
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



}