using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;
using NLog;
using NLog.Config;
using Debug = UnityEngine.Debug;
using CommandLine;

public class AppInit : MonoBehaviour {
    
	// Use this for initialization
	void Start () {

        var args = Environment.GetCommandLineArgs();

        options = new Options();

        hasOptions = Parser.Default.ParseArguments(args, options );

        var stopWatch = new Stopwatch();

        stopWatch.Start();

        LogManager.Configuration = new XmlLoggingConfiguration(Application.dataPath + @"\NLog.config");

        LogManager.ReconfigExistingLoggers();

        stopWatch.Stop();

        Debug.Log(string.Format("### Runtime ### Nlog config lookup took: {0}", stopWatch.Elapsed));
         
    }

    private bool hasOptions;
    public bool HasOptions
    {
        get
        {
            return hasOptions;
        }
    }
    
    private Options options;
    public Options Options
    {
        get
        {
            return options;
        }
    }

}

public class Options
{
    [Option('s', "subject", Required = true, HelpText = "Subject Identification - should be a unique string!")]
    public string subjectId { get; set; }
}