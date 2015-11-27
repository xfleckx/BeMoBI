using UnityEngine;
using System;
using UnityEditor;
using NLog;
using NLog.Config;
using System.IO;

[InitializeOnLoad]
public static class OnEditorInitialize   {

    [InitializeOnLoadMethod]
    private static void LookRuntimeInfos()
    {
        Debug.Log("EditorApplication.applicationPath :" + EditorApplication.applicationPath);
        Debug.Log("EditorApplication.applicationsContentsPath: " + EditorApplication.applicationContentsPath);
        Debug.Log("Application.dataPath: " + Application.dataPath);
        Debug.Log("Application.dataPersistentPath: " + Application.persistentDataPath);
        Debug.Log("Environment.CurrentDirectory: " + Environment.CurrentDirectory);

        var nlog_expected_log_directory = Path.Combine(Environment.CurrentDirectory, "logs");
        var nlog_expected_statistics_directory = Path.Combine(Environment.CurrentDirectory, "statistics");

        if (!Directory.Exists(nlog_expected_log_directory))
        {
            Directory.CreateDirectory(nlog_expected_log_directory);
        }

        if (!Directory.Exists(nlog_expected_statistics_directory))
        {
            Directory.CreateDirectory(nlog_expected_statistics_directory);
        }

        LogManager.Configuration = new XmlLoggingConfiguration(Environment.CurrentDirectory + @"\Assets\NLog.config");

        LogManager.ReconfigExistingLoggers();
    }
}
