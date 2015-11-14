﻿using UnityEngine;
using UnityEditor;

using System.Collections;

namespace Assets.Paradigms.MultiMazePathRetrieval
{
    [CustomEditor(typeof(ParadigmController))]
    public class MMPREditor : Editor
    {

        private ParadigmController instance;

        public override void OnInspectorGUI()
        {
            instance = target as ParadigmController;

            if (GUILayout.Button("Open Control & \n Configuration Window", GUILayout.Height(40)))
            {
                var controlWindow = EditorWindow.CreateInstance<M2PRControl>();

                controlWindow.Initialize(instance);

                controlWindow.Show();
            }

            base.OnInspectorGUI();


        }
    }
}
