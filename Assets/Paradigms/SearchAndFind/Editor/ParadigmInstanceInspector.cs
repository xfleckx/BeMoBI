using UnityEngine;
using UnityEditor;
using System.Linq;

using System.Collections;
using System.Collections.Generic;

namespace Assets.Paradigms.SearchAndFind
{
    [CustomEditor(typeof(ParadigmController))]
    public class ParadigmInstanceInspector : Editor
    {
        private ParadigmController instance;
        private ParadigmControlWindow controlWindow;
        private List<ParadigmInstanceDefinition> availableDefinitions;

        public override void OnInspectorGUI()
        {
            instance = target as ParadigmController;

            availableDefinitions = new List<ParadigmInstanceDefinition>();

            if (GUILayout.Button("Open Control Window", GUILayout.Height(40)))
            {
                var existingWindow = EditorWindow.GetWindow<ParadigmControlWindow>();

                if (existingWindow != null)
                    controlWindow = existingWindow;
                else
                    controlWindow = CreateInstance<ParadigmControlWindow>();

                controlWindow.Initialize(instance);

                controlWindow.Show();
            }

            if (GUILayout.Button("Open Configuration Window", GUILayout.Height(30)))
            {
                var window = EditorWindow.GetWindow<ConfigurationControl>();

                window.Initialize(instance);

                window.Show();
            }

            if (GUILayout.Button("Lookup Instance definitions"))
            {
                availableDefinitions = Resources.FindObjectsOfTypeAll<ParadigmInstanceDefinition>().ToList();
            }

            if (availableDefinitions.Any())
            {
                foreach (var item in availableDefinitions)
                {
                    if (GUILayout.Button(item.name)) {

                        instance.InstanceDefinition = item;
                    }

                }
            }

            base.OnInspectorGUI();


        }
    }
}
