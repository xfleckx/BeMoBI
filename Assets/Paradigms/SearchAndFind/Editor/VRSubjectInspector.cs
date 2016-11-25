using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

using Assets.BeMoBI.Scripts.Controls;
namespace Assets.EditorExtensions.BeMoBI.Paradigms.SearchAndFind
{
    [CustomEditor(typeof(VRSubjectController))]
    public class VRSubjectInspector : Editor
    {
        private static Mesh SubjectHeadMesh;

        private static Mesh SubjectBodyMesh;
        
        VRSubjectController instance;

        public override void OnInspectorGUI()
        {
            instance = target as VRSubjectController;

            base.OnInspectorGUI();

            GUILayout.BeginVertical();


            if (GUILayout.Button("Toggle Rectile"))
            {
                instance.ToggleRectile();
            }

            if (GUILayout.Button("Toogle Fog"))
            {
                instance.ToggleFog();
            }

            EditorGUILayout.Space();

            var availableBodyController = instance.GetComponents<IBodyMovementController>().Where(c => !(c is CombinedController));
            var availableHeadController = instance.GetComponents<IHeadMovementController>().Where(c => !(c is CombinedController));
            var availableCombinedController = instance.GetComponents<CombinedController>();

            EditorGUILayout.LabelField("Available Combi Controller");

            if (!availableCombinedController.Any())
                EditorGUILayout.HelpBox("No Combined Controller Implementations found! \n Attache them to this GameObject!", MessageType.Info);

            foreach (var combiController in availableCombinedController)
            {
                var nameOfController = combiController.Identifier;

                if (GUILayout.Button(nameOfController))
                {
                    instance.HeadController = nameOfController;
                    instance.BodyController = nameOfController;
                    instance.Change<CombinedController>(nameOfController);
                }
            }

            EditorGUILayout.LabelField("Available Head Controller");

            if (!availableHeadController.Any())
                EditorGUILayout.HelpBox("No Head Controller Implementations found! \n Attache them to this GameObject!", MessageType.Info);

            foreach (var headController in availableHeadController)
            {
                var nameOfController = headController.Identifier;

                if (GUILayout.Button(nameOfController))
                {
                    instance.HeadController = nameOfController;
                    instance.Change<IHeadMovementController>(nameOfController);
                }
            }

            EditorGUILayout.LabelField("Available Body Controller");

            if (!availableBodyController.Any())
                EditorGUILayout.HelpBox("No Body Controller Implementations found! \n Attache them to this GameObject!", MessageType.Info);

            foreach (var bodyController in availableBodyController)
            {
                var nameOfController = bodyController.Identifier;

                if (GUILayout.Button(nameOfController))
                {
                    instance.BodyController = nameOfController;
                    instance.Change<IBodyMovementController>(nameOfController);
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Reset Controller"))
            {
                instance.ResetController();
            }

            GUILayout.EndVertical();

        }


        static void LoadPreviewMesh()
        {
            string path = "Assets/Models/";
            SubjectHeadMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path  + "SubjectHead.fbx");

            SubjectBodyMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path + "SubjectBody.fbx");
            
        }

        [DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.NonSelected)]
        static void OnDrawGizmos(VRSubjectController controller, GizmoType type)
        {
            var characterController = controller.GetComponent<CharacterController>();

            var bodyCenter = controller.Body.transform.localPosition + new Vector3(0, characterController.height / 2, 0);

            var temp = Gizmos.color;

            Gizmos.color = new Color(0.2f, 0.3f, 0.7f);

            Gizmos.DrawWireSphere(controller.Head.position, 0.15f);

            Gizmos.DrawLine(bodyCenter, controller.Head.position);

            if(SubjectBodyMesh == null || SubjectHeadMesh == null)
            {
                LoadPreviewMesh();
            }
            
            var meshMid = (SubjectBodyMesh.bounds.max - SubjectBodyMesh.bounds.min).magnitude / 2;

            Gizmos.color = new Color(0.2f, 0.3f, 0.7f, 0.4f);

            Gizmos.DrawMesh(SubjectBodyMesh, bodyCenter + new Vector3(0,meshMid,0), controller.Body.transform.rotation * Quaternion.Euler(-90, 180, 0), new Vector3(10f, 10f, 10f));

            Gizmos.DrawMesh(SubjectHeadMesh, controller.Head.transform.position, controller.Head.transform.rotation * Quaternion.Euler(-90,0,0));

            Gizmos.color = new Color(0.2f, 0.3f, 0.7f);

            Gizmos.DrawRay(controller.HeadPerspective.transform.position, controller.HeadPerspective.transform.forward);

            Gizmos.DrawRay(bodyCenter, controller.Body.transform.forward * 0.5f);

            Gizmos.DrawSphere(bodyCenter, 0.05f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(controller.Body.transform.localPosition, new Vector3(0.4f, 0.001f, 0.4f));

            Gizmos.color = temp;

            var handleColorTemp = Handles.color;
            Handles.color = Color.yellow;

            var angleBetweenSubjectForwardAndNorth = Vector3.Angle(controller.transform.forward, Vector3.forward);
            Handles.DrawSolidArc(bodyCenter, controller.transform.up, Vector3.forward, angleBetweenSubjectForwardAndNorth, 0.5f);

            Handles.color = Color.magenta;

            var sign = controller.HeadPerspective.transform.rotation.eulerAngles.y;
            var angleBetweenHeadAndBodyForward = Vector3.Angle(controller.Body.transform.forward, controller.HeadPerspective.transform.forward);

            var angleStart = Quaternion.Euler(0, sign * angleBetweenHeadAndBodyForward, 0);
            
            Handles.DrawSolidArc(bodyCenter, 
                                controller.transform.up, 
                                controller.Body.transform.forward,  
                                angleBetweenHeadAndBodyForward, 0.3f);

        }
    }
}
