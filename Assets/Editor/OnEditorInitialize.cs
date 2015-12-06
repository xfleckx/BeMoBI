﻿using UnityEngine;
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
        

        LogManager.Configuration = new XmlLoggingConfiguration(Environment.CurrentDirectory + @"\Assets\NLog.config");

        LogManager.ReconfigExistingLoggers();
    }
}
