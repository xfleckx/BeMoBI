using UnityEngine;
using System;

public class MainUI : MonoBehaviour
{
  public OWLTracker tracker;
  public string device;
  public bool slave;
  protected string message = "";

  //
  void Awake()
  {
    // load user settings
    device = PlayerPrefs.GetString("device", "localhost");
    slave = PlayerPrefs.GetInt("slave", 0) == 1;
  }

  //
  void OnDestroy()
  {
    // save user settings
    PlayerPrefs.SetString("device", device);
    PlayerPrefs.SetInt("slave", Convert.ToInt32(slave));
  }

  //
  void OnGUI()
  {
    OWLWrapper OWL = tracker.OWL;
    bool connected = OWL.Connected();
    GUILayout.BeginArea(new Rect(8, 8, Screen.width - 16, Screen.height/8 + 8));
    GUILayout.BeginHorizontal();
    GUILayout.Label("Device", GUILayout.ExpandWidth(false));
    // disable controls if connected already
    if(connected) GUI.enabled = false;
    // get device string from UI
    device = GUILayout.TextField(device, 256, GUILayout.ExpandWidth(true));
    // get slave flag from UI
    slave = GUILayout.Toggle(slave, "Enable Slave Mode", GUILayout.ExpandWidth(false));
    // reenable controls
    GUI.enabled = true;

    // connect button
    if(connected)
    {
      if(GUILayout.Button("Disconnect", GUILayout.ExpandWidth(false)))
        OWL.Disconnect();
    }
    else
    {
      if(GUILayout.Button("Connect", GUILayout.ExpandWidth(false)))
      {
        // connect to device
        if(OWL.Connect(device, slave))
        {
          if(!slave)
          {
            // create default point tracker
            int n = 128;
            int [] leds = new int[n];
            for(int i = 0; i < n; i++)
              leds[i] = i;
            OWL.CreatePointTracker(0, leds);
          }

          // start streaming
          OWL.Start();
        }
      }
    }
    GUILayout.EndHorizontal();

    // display condition message or current frame number
    if(OWL.error != 0) {
      message = String.Format("owl condition: 0x{0,0:X}", OWL.error);
    } else {
      message = String.Format("frame = {0}, m = {1}, r = {2}, c = {3}", OWL.frame, OWL.NumMarkers, OWL.NumRigids, OWL.NumCameras);
    }
    GUILayout.Label(message);
    GUILayout.EndArea();
  }
}