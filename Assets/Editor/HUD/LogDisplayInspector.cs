using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.SNEED.Unity3D.Editor.HUD
{
    [CustomEditor(typeof(LOGDisplay))]
    public class LogDisplayInspector : UnityEditor.Editor
    {
        private string logMessage;

        public override void OnInspectorGUI()
        {
            var instance = target as LOGDisplay; 

            base.OnInspectorGUI();

            logMessage = EditorGUILayout.TextField("Log:", logMessage);

            if (GUILayout.Button("Write Log"))
            {
                instance.WriteLog(logMessage);
            }
        }
    }
}
