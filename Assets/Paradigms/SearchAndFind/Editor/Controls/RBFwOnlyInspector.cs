﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using Assets.BeMoBI.Scripts.Controls;

namespace Assets.EditorExtensions.BeMoBI.Paradigms.SearchAndFind
{

    [CustomEditor(typeof(PSRigidBodyForwardOnlyController))]
    public class RBFwOnlyInspector : UnityEditor.Editor {

        PSRigidBodyForwardOnlyController instance;

        public override void OnInspectorGUI()
        {
           //instance = target as PSRigidBodyForwardOnlyController;

            base.OnInspectorGUI();

        }

    }
}