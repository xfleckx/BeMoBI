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
        private M2PRControl controlWindow;
        private List<ParadigmInstanceDefinition> availableDefinitions;

        public override void OnInspectorGUI()
        {
            instance = target as ParadigmController;

            availableDefinitions = new List<ParadigmInstanceDefinition>();

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
