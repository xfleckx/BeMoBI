using UnityEngine;
using System.Collections.Generic;
using PhaseSpace;

//
// Class for drawing markers, rigids, and cameras
//
public class OWLUpdater : MonoBehaviour
{  
		public OWLTracker tracker;
		protected List<GameObject> Markers = new List<GameObject> ();
		protected List<GameObject> Rigids = new List<GameObject> ();
		protected List<GameObject> Cameras = new List<GameObject> ();
		public GameObject MarkerPrefab;
		public GameObject RigidPrefab;
		public GameObject CameraPrefab;
		//
		void Update ()
		{	
				PhaseSpace.Marker [] markers = tracker.GetMarkers ();
				PhaseSpace.Rigid [] rigids = tracker.GetRigids ();
				PhaseSpace.Camera [] cameras = tracker.GetCameras ();

				while (Markers.Count < markers.Length) {
						print (System.String.Format ("new marker: {0}", Markers.Count));
						Markers.Add (GameObject.Instantiate (MarkerPrefab) as GameObject);
				}
				while (Rigids.Count < rigids.Length) {
						print (System.String.Format ("new rigid: {0}", Rigids.Count));						
						Rigids.Add (GameObject.Instantiate (RigidPrefab) as GameObject);
				}
				while (Cameras.Count < cameras.Length) {
						print (System.String.Format ("new camera: {0}", Cameras.Count));												
						Cameras.Add (GameObject.Instantiate (CameraPrefab) as GameObject);
				}

				for (int i = 0; i < markers.Length; i++) {
						((GameObject)Markers [i]).transform.position = markers [i].position;
				}
				for (int i = 0; i < rigids.Length; i++) {
						Transform t = ((GameObject)Rigids [i]).transform;
						t.position = rigids [i].position;
						t.rotation = rigids [i].rotation;
				}
				for (int i = 0; i < cameras.Length; i++) {
						Transform t = ((GameObject)Cameras [i]).transform;
						t.position = cameras [i].position;
						t.rotation = cameras [i].rotation;
				
				}
		}
}