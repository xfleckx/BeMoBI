using UnityEngine;
using System;
using UnityEditor;
using NLog;
using NLog.Config;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public static class OnEditorInitialize   {

    [InitializeOnLoadMethod]
    private static void LookRuntimeInfos()
    {
        if(LogManager.Configuration== null) {
            var stopWatch = new Stopwatch();

            stopWatch.Start();

            LogManager.Configuration = new XmlLoggingConfiguration(Application.dataPath + @"\NLog.config");

            stopWatch.Stop();

            Debug.Log(string.Format("Nlog config lookup took: {0}", stopWatch.Elapsed));
        }
        

        LogManager.ReconfigExistingLoggers();
    }
}
