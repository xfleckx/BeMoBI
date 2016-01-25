using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.SNEED.Unity3D.Editor.HUD
{
    [CustomEditor(typeof(StateDisplay))]
    public class StateDisplayInspector : UnityEditor.Editor
    {
        private string state_id;
        private string message;
        private State_Info_Type state_type;

        public override void OnInspectorGUI()
        {
            var instance = target as StateDisplay; 

            base.OnInspectorGUI();

            EditorGUILayout.BeginVertical();

            state_id = EditorGUILayout.TextField("State:", state_id);

            message = EditorGUILayout.TextField("Description:", message);

            state_type = (State_Info_Type) EditorGUILayout.EnumPopup("Type:", state_type);

            if (GUILayout.Button("Update State"))
            {
                instance.SetState(state_id, message, (State_Info_Type) state_type);
            }

            EditorGUILayout.EndVertical();

        }
    }
}
