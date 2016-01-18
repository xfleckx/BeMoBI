//
// PhaseSpace, Inc. 2014
//
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PhaseSpace
{
	//
	//
	//
	public class OWLTracker : OWLWrapper
	{
		// should be a singleton
		public static OWLTracker Instance;

		// connection settings
		public string Device = "localhost";
		public bool BroadcastMode = false;
		public bool SlaveMode = false;

		// if attached to a camera, enable to get data at a time closer to actual rendering
		// NOTE: ignored if Threaded is true.
		public bool updateOnPreRender = false;

		// poll data from a separate thread to take advantage of multicore platforms.
		// NOTE: No effect if toggled after Start has been called.
		public bool Threaded = false;
		public bool AutoScan = false;
		protected System.Threading.Mutex Mutex;
		protected System.Threading.Mutex Mutex2; // for server scan

		protected int fslu; // frames since last update
		public float CurrentFPS = 0;
		public int HeartBeat = 0;




		public class ThreadInfo
		{
			public System.Threading.Thread thread;
			public bool persist;
		}
		protected ThreadInfo PollingThread = null;
		protected ThreadInfo ScanningThread = null;

		//
		void Awake ()
		{
			if (Instance != null) {
				Debug.LogWarning ("OWLTracker should be a singleton!");
			}
			Instance = this;
			print ("Creating OWLTracker...");

			Mutex = new System.Threading.Mutex ();
			Mutex2 = new System.Threading.Mutex ();
		}

		void Start ()
		{


		}

		void OnEnable ()
		{
			StartCoroutine (UpdateFPS ());

			// start or restart threads
			if (Threaded) {
				StartPollingThread ();
			} else {
				print ("OWLTracker: Starting polling coroutine...");
				StartCoroutine ("PollingCoroutine");
			}
			// don't start unless user wants it
			if (ScanningThread != null || AutoScan) {
				StartScanningThread ();
			}
		}

		void OnDisable ()
		{
			if (PollingThread != null) {
				print ("OWLTracker: Stopping polling thread...");
				PollingThread.persist = false;
			}
			if (ScanningThread != null) {
				print ("OWLTracker: Stopping scanning thread...");
				ScanningThread.persist = false;
			}
		}


		//
		void OnDestroy ()
		{
			if (PollingThread != null) {
				PollingThread.thread.Join (5000);
			}
			if (ScanningThread != null) {
				ScanningThread.thread.Join (5000);
			}

			// disconnect from OWL server
			Disconnect ();
		}

		//
		protected bool StartPollingThread ()
		{
			try {
				if (PollingThread != null) {
					PollingThread.persist = false;
					PollingThread.thread.Join (1000);
				}
				print ("OWLTracker: Starting thread...");
				PollingThread = new ThreadInfo ();
				PollingThread.thread = new System.Threading.Thread (PollingFunc);
				PollingThread.persist = true;
				PollingThread.thread.Start (PollingThread);
				return true;
			} catch (System.Exception e) {
				print (e.ToString ());
			}
			return false;
		}

		//
		public bool StartScanningThread ()
		{
			try {
				if (ScanningThread != null) {
					ScanningThread.persist = false;
					ScanningThread.thread.Join (1000);
				}
				print ("OWLTracker: Starting scanning thread...");
				ScanningThread = new ThreadInfo ();
				ScanningThread.thread = new System.Threading.Thread (ScanningFunc);
				ScanningThread.persist = true;
				ScanningThread.thread.Start (ScanningThread);
				return true;
			} catch (System.Exception e) {
				print (e.ToString ());
			} finally {

			}
			return false;
		}

		protected IEnumerator UpdateFPS ()
		{
			float t0 = Time.time;
			while (true) {
				yield return new WaitForSeconds (2.0f);
				int frames = 0;
				Mutex.WaitOne ();
				frames = fslu;
				fslu = 0;
				Mutex.ReleaseMutex ();
				float t1 = Time.time;
				CurrentFPS = frames / (t1 - t0);
				t0 = t1;
			}
		}

		//
		void OnPreRender ()
		{
			// only works if attached to a camera
			if (updateOnPreRender && !Threaded)
				UpdateOWL ();
		}

		override public int ScanServers (int timeout_usec)
		{
			OWLConnection [] conn = _ScanServers (timeout_usec);
			Mutex2.WaitOne ();
			ConvertServers (conn, out outservers);
			Mutex2.ReleaseMutex ();
			return conn.Length;
		}

		override public int UpdateOWL ()
		{
			Mutex.WaitOne ();
			int ret = base.UpdateOWL ();
			Mutex.ReleaseMutex ();
			fslu += ret;
			return ret;
		}


		//
		public bool Connect ()
		{
			return Connect (Device, SlaveMode, BroadcastMode);
		}

		//
		override public bool Connect (string device, bool slave, bool broadcast)
		{
			Device = device;
			SlaveMode = slave;
			BroadcastMode = broadcast;
			bool ret = false;
			if (Mutex.WaitOne ()) {
				ret = base.Connect (device, slave, broadcast);
				Mutex.ReleaseMutex ();
			}
			return ret;
		}

		//
		override public void StartStreaming ()
		{
			Mutex.WaitOne ();
			base.StartStreaming ();
			Mutex.ReleaseMutex ();
		}

		//
		public override void Disconnect ()
		{
			Mutex.WaitOne ();
			base.Disconnect ();
			Mutex.ReleaseMutex ();
		}

		//
		override public void CreatePointTracker (int id, int[] leds)
		{
			Mutex.WaitOne ();
			base.CreatePointTracker (id, leds);
			Mutex.ReleaseMutex ();
		}

		//
		override public void CreateRigidTracker (int id, string rbfile)
		{
			Mutex.WaitOne ();
			base.CreateRigidTracker (id, rbfile);
			Mutex.ReleaseMutex ();
		}

		//
		override public void CreateRigidTracker (int id, TextAsset rbtext)
		{
			Mutex.WaitOne ();
			base.CreateRigidTracker (id, rbtext);
			Mutex.ReleaseMutex ();
		}

		//
		override public void CreateRigidTracker (int id, int[] leds, Vector3[] points)
		{
			Mutex.WaitOne ();
			base.CreateRigidTracker (id, leds, points);
			Mutex.ReleaseMutex ();
		}

		override protected void ConvertData ()
		{
			//  should take care of GetMarkers, GetRigids, and GetCameras
			Mutex.WaitOne ();
			base.ConvertData ();
			Mutex.ReleaseMutex ();
		}

		override public Server [] GetServers ()
		{
			// guard GetServers separately since it doesn't go through ConvertData
			Mutex2.WaitOne ();
			Server [] o = base.GetServers ();
			Mutex2.ReleaseMutex ();
			return o;
		}

		protected void ScanningFunc (System.Object obj)
		{
			ThreadInfo ti = (ThreadInfo) obj;
			int timeout = 1000000;
			int maxtimeout = 4000000;
			while (ti.persist) {
				if (!Connected ()) {
					ScanServers (timeout);
					timeout = Mathf.Clamp(timeout + 1000000, 0, maxtimeout);
					System.Threading.Thread.Sleep (5000);
				}
			}
			print ("OWLTracker: Scanning thread terminate.");
		}

		//
		protected void PollingFunc (System.Object obj)
		{
			ThreadInfo ti = (ThreadInfo) obj;
			while (ti.persist) {
				if (!Connected ()) {
					System.Threading.Thread.Sleep (500);
					continue;
				}

				if (Mutex.WaitOne (300)) {
					try {
						base.UpdateOWL ();
					} catch (System.Exception e) {
						print (e.ToString ());
					}
					Mutex.ReleaseMutex ();
				}

				HeartBeat += 1;
			}
			print ("OWLTracker: Polling thread terminate.");
		}


		//
		protected IEnumerator PollingCoroutine ()
		{
			while (true) {
				if (!Connected ()) {
					yield return null;
					continue;
				}

				if (Mutex.WaitOne (300)) {
					try {
						base.UpdateOWL ();
						Mutex.ReleaseMutex ();
					} catch (System.Exception e) {
						print (e.ToString ());
					}
				}

				HeartBeat += 1;
				yield return null;
			}
		}


	}
}