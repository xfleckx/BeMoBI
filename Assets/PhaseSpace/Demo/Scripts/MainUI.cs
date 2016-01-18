using UnityEngine;
using System;
using PhaseSpace;
using System.Collections.Generic;

public class MainUI : MonoBehaviour
{
		public OWLTracker tracker;
		public string device;
		public bool slave;
		protected string message = "";

		//
		void Awake ()
		{
				// load user settings
				device = PlayerPrefs.GetString ("device", "192.168.128.10");
				slave = PlayerPrefs.GetInt ("slave", 0) == 1;
		}

		//
		void OnDestroy ()
		{
				// save user settings
				PlayerPrefs.SetString ("device", device);
				PlayerPrefs.SetInt ("slave", Convert.ToInt32 (slave));
		}

		//
		void OnGUI ()
		{				
				bool connected = tracker.Connected ();
				GUILayout.BeginArea (new Rect (8, 8, Screen.width - 16, Screen.height / 6 + 8));
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Device", GUILayout.ExpandWidth (false), GUILayout.ExpandHeight (true));
				// disable controls if connected already
				if (connected)
						GUI.enabled = false;
				// get device string from UI
				device = GUILayout.TextField (device, 256, GUILayout.ExpandWidth (true), GUILayout.ExpandHeight (true));				
				GUILayout.Label ("Found Servers:", GUILayout.ExpandWidth (false), GUILayout.ExpandHeight (true));
				GUILayout.BeginScrollView (new Vector2 (0, 0), false, true, GUILayout.ExpandWidth (true), GUILayout.ExpandHeight (true));		       
				PhaseSpace.Server [] servers = tracker.GetServers();
				if (servers.Length == 0) {				
						GUILayout.Label ("None", GUILayout.ExpandWidth (false), GUILayout.ExpandHeight (false));
				} else {
			            Array.Sort<PhaseSpace.Server>(servers, (o1,o2) => o1.address.CompareTo(o2.address));
				}
				for (int i = 0; i < servers.Length; i++) {
						if (GUILayout.Button (servers [i].address)) {
								device = servers [i].address;
						}
				}
				GUILayout.EndScrollView ();
																
				// get slave flag from UI
				slave = GUILayout.Toggle (slave, "Enable Slave Mode", GUILayout.ExpandWidth (false), GUILayout.ExpandHeight (true));
				// reenable controls
				GUI.enabled = true;

				// connect button
				if (connected) {
						if (GUILayout.Button ("Disconnect", GUILayout.ExpandWidth (false), GUILayout.ExpandHeight (true)))
								tracker.Disconnect ();
				} else {
						if (GUILayout.Button ("Connect", GUILayout.ExpandWidth (false), GUILayout.ExpandHeight (true))) {
								// connect to device
								if (tracker.Connect (device, slave, false)) {
										if (!slave) {
												// create default point tracker
												int n = 128;
												int [] leds = new int[n];
												for (int i = 0; i < n; i++)
														leds [i] = i;
												tracker.CreatePointTracker (0, leds);
										}

										// start streaming
										tracker.StartStreaming ();
								}
						}
				}
				GUILayout.EndHorizontal ();

				// display error message or current frame number
				if (tracker.Error != 0) {
						message = String.Format ("owl error: 0x{0,0:X}", tracker.Error);
				} else {
						message = String.Format ("frame = {0}, m = {1}, r = {2}, c = {3}", tracker.Frame, tracker.NumMarkers, tracker.NumRigids, tracker.NumCameras);
				}
				GUILayout.Label (message);
				GUILayout.EndArea ();
		}
}