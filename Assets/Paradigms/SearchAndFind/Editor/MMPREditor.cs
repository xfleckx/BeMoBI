using UnityEngine;
using UnityEditor;

using System.Collections;

namespace Assets.Paradigms.SearchAndFind
{
    [CustomEditor(typeof(ParadigmController))]
    public class MMPREditor : Editor
    {
        private ParadigmController instance;
        private M2PRControl controlWindow;


        public override void OnInspectorGUI()
        {
            instance = target as ParadigmController;

            if (GUILayout.Button("Open Control & \n Configuration Window", GUILayout.Height(40)))
            {
                var existingWindow = EditorWindow.GetWindow<M2PRControl>();

                if (existingWindow != null)
                    controlWindow = existingWindow;
                else
                    controlWindow = CreateInstance<M2PRControl>();

                controlWindow.Initialize(instance);

                controlWindow.Show();
            }

            base.OnInspectorGUI();


        }
    }
}
