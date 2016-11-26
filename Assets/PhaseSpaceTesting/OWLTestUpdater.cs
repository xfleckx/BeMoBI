using UnityEngine;
using System.Collections.Generic;
using PhaseSpace;
using System.Linq;

//
// Class for drawing markers, rigids, and cameras
//
public class OWLTestUpdater : MonoBehaviour
{
    public OWLTracker tracker;
    protected List<GameObject> Markers = new List<GameObject>();
    protected List<GameObject> Rigids = new List<GameObject>();
    protected List<GameObject> Cameras = new List<GameObject>();
    public GameObject MarkerPrefab;
    public GameObject RigidPrefab;
    public GameObject CameraPrefab;

    public bool showCameras = false;
    public bool showRigids = true;
    public bool showMarker = true;
    //
    void Update()
    {
        PhaseSpace.Marker[] markers = tracker.GetMarkers();
        PhaseSpace.Rigid[] rigids = tracker.GetRigids();
        PhaseSpace.Camera[] cameras = tracker.GetCameras();

        while (showMarker && Markers.Count < markers.Length)
        {
            print(System.String.Format("new marker: {0}", Markers.Count));
            Markers.Add(GameObject.Instantiate(MarkerPrefab) as GameObject);
        }

        if (!showMarker && Markers.Any())
        {
            foreach (var item in Markers)
            {
                Destroy(item);
            }
            Markers.Clear();
        }

        while (showRigids && Rigids.Count < rigids.Length)
        {
            print(System.String.Format("new rigid: {0}", Rigids.Count));
            Rigids.Add(GameObject.Instantiate(RigidPrefab) as GameObject);
        }
        
        if (!showRigids && Rigids.Any())
        {
            foreach (var item in Rigids)
            {
                Destroy(item);
            }
            Rigids.Clear();
        }

        while (showCameras && Cameras.Count < cameras.Length)
        {
            print(System.String.Format("new camera: {0}", Cameras.Count));
            Cameras.Add(GameObject.Instantiate(CameraPrefab) as GameObject);
        }

        if (!showCameras && Cameras.Any())
        {
            foreach (var item in Cameras)
            {
                Destroy(item);
            }
            Cameras.Clear();
        }
        if (showMarker)
        {

            for (int i = 0; i < markers.Length; i++)
            {
                ((GameObject)Markers[i]).transform.position = markers[i].position;
            }
        }

        if (showRigids)
        {
            for (int i = 0; i < rigids.Length; i++)
            {
                Transform t = ((GameObject)Rigids[i]).transform;
                t.position = rigids[i].position;
                t.rotation = rigids[i].rotation;
            }
        }
        if (showCameras)
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                Transform t = ((GameObject)Cameras[i]).transform;
                t.position = cameras[i].position;
                t.rotation = cameras[i].rotation;

            }
        }
    }
}