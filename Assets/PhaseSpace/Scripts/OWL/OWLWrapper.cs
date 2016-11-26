//
// PhaseSpace, Inc. 2014
//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace PhaseSpace
{

	// generic exception for OWL related errors
	public class OWLException : Exception
	{
		//
		public OWLException () :  base("OWL exception")
		{

		}

		//
		public OWLException (string msg) :  base(msg)
		{

		}
	}

	//
	public class Server
	{
		public string address;
		public string info;

		public Server ()
		{
			address = "";
			info = "";
		}

		public Server (string address, string info)
		{
			this.address = address;
			this.info = info;
		}
	}

	// output type
	public class Marker
	{
		public int id;
		public float cond;
		public Vector3 position;

		public Marker () : this(-1, -1, Vector3.zero)
		{

		}

		public Marker (int id, float cond, Vector3 pos)
		{
			this.id = id;
			this.cond = cond;
			this.position = pos;
		}
	}

	// output type
	public class Rigid : Marker
	{
		public Quaternion rotation;

		public Rigid () : this(-1, -1, Vector3.zero, Quaternion.identity)
		{

		}

		public Rigid (int id, float cond, Vector3 pos, Quaternion rot) :  base(id, cond, pos)
		{
			rotation = rot;
		}
	}

	// output type
	public class Camera : Rigid
	{
		public Camera (int id, float cond, Vector3 pos, Quaternion rot) :  base(id, cond, pos, rot)
		{

		}
	}

	//
	// wrapper class for libowlsock
	//
	public class OWLWrapper : libowlsock
	{
		protected bool connected = false;

		// frequency to stream at
#if UNITY_ANDROID || UNITY_IPHONE
		public float default_frequency = 120.0f;
#else
		public float default_frequency = 480.0f;
#endif

		// incoming data buffers
		protected OWLMarker[] markers = new OWLMarker[MAX_MARKERS];
		protected OWLRigid[] rigids = new OWLRigid[MAX_RIGIDS];
		protected OWLCamera[] cameras = new OWLCamera[MAX_CAMERAS];
		protected OWLPlane[] planes = new OWLPlane[MAX_PLANES];
		protected OWLPeak[] peaks = new OWLPeak[MAX_PEAKS];		

		protected int numMarkers = 0;
		protected int numRigids = 0;
		protected int numCameras = 0;
		protected int numPlanes = 0;
		protected int numPeaks = 0;

        [Range(1,4)]
        public int mode = 1;

		// lastest reported error
		protected int error = 0;

		public int Error {
			get { return error; }
		}

		// latest reported frame
		protected int frame;

		public int Frame {
			get { return frame; }
		}

		public int NumRigids {
			get { return numRigids; }
		}

		public int NumMarkers {
			get { return numMarkers; }
		}

		public int NumCameras {
			get { return numCameras; }
		}

		[SerializeField]
		protected float _Scale = 0.001f;
		public float Scale {
			get { return _Scale; }
			set {
				_Scale = value;
				owlScale(_Scale);
			}
		}

		[SerializeField]
		protected int _Interpolation = 0;
		public int Interpolation {
			get { return _Interpolation; }
			set {
				_Interpolation = value;
				if(_Interpolation < 0) _Interpolation = 0;
				owlSetInteger(OWL_INTERPOLATION, _Interpolation);
			}
		}

		// outgoing data buffers
		protected int dirtyFlag = 0;
		protected Marker[] outmarkers = new Marker[MAX_MARKERS];
		protected Rigid[] outrigids = new Rigid[MAX_RIGIDS];
		protected Camera[] outcameras = new Camera[MAX_RIGIDS];
		protected Server[] outservers = new Server[0];

		//
		const int MAX_MARKERS = 1024;
		const int MAX_RIGIDS = 256;
		const int MAX_CAMERAS = 128;
		const int MAX_PLANES = 1024;
		const int MAX_PEAKS = 1024;
		protected Quaternion rigidoffset = Quaternion.Euler (0, 0, 0);

		//
		void FixedUpdate ()
		{
			UpdateOWL();
		}

		//
		virtual protected OWLConnection [] _ScanServers(int timeout_usec)
		{						
			OWLConnection [] connections = new OWLConnection[8];
			int	ret = libowlsock.owl_scan(null, 8999, "unity owl wrapper", timeout_usec, connections, connections.Length);						
			OWLConnection [] conn2 = new OWLConnection[ret>0?ret:0];
			if(ret > 0)
			{
				conn2 = new OWLConnection[ret];
				Array.Copy(connections, conn2, ret);
			}						
			return conn2;
		}

		//
		static protected void ConvertServers(OWLConnection [] owlservers, out Server [] servers)
		{
			servers = new Server[owlservers.Length];
			for(int i = 0; i < owlservers.Length; i++)
			{								
				servers[i] = new Server(System.Text.Encoding.Default.GetString(owlservers[i].address),
				                        System.Text.Encoding.Default.GetString(owlservers[i].info));				                        
				//servers[i] = new Server(owlservers[i].address.ToString(), owlservers[i].info.ToString());
			}
		}

		//
		virtual public int ScanServers(int timeout_usec)
		{
			OWLConnection [] conn = _ScanServers(timeout_usec);
			ConvertServers(conn, out outservers);
			return conn.Length;
		}

		//
		public bool Connected ()
		{
			return connected;
		}

		//
		virtual public bool Connect (string server, bool slave, bool broadcast)
		{
			// force Unity to set network permissions.
			if (Application.internetReachability == NetworkReachability.NotReachable) {
				if (Application.platform == RuntimePlatform.Android) {
					throw new OWLException ("network unreachable.  Is android.permission.INTERNET set?");
				} else {
					throw new OWLException ("network unreachable.");
				}
			}

			// if already connected, do nothing
			if (connected)
				return false;

			// connect to OWL server in slave mode
			int flag = 0;
			if (slave)
				flag |= OWL_SLAVE;
             
            switch (mode)
            {
                case 1:
                    flag |= OWL_MODE1;
                    break;

                case 2:
                    flag |= OWL_MODE2;
                    break;

                case 3:
                    flag |= OWL_MODE3;
                    break;

                case 4:
                    flag |= OWL_MODE4;
                    break;

                default:
                    flag |= OWL_MODE1;
                    break;
            }
            print(String.Format("OWL Init with Mode: {0}", mode));
            int ret = owlInit (server, flag);
			if (ret < 0) {
				print (String.Format ("OWL Connect error: 0x{0,0:X}", error));
				error = ret;
				connected = false;
				return false;
			}

			connected = true;

			error = 0;

			// query server version
			Debug.Log (System.Text.Encoding.UTF8.GetString (owlGetString (OWL_VERSION)));

			//
			Debug.Log (String.Format ("owl frequency: {0}", default_frequency));

			// set streaming frequency
			owlSetFloat (OWL_FREQUENCY, default_frequency);

			if ((!slave) && broadcast) {
				// set broadcast mode
				owlSetInteger (OWL_BROADCAST, OWL_ENABLE);
			}

            // make sure nothing went wrong
            owlGetStatus ();
			while (true) {
				int err = owlGetError ();
				if (err == OWL_NO_ERROR) {
					break;
				} else {
					error = err;
					print (String.Format ("OWL Connect error: 0x{0,0:X}", error));
					connected = false;
					owlDone ();
					return false;
				}
			}
			connected = true;
			return true;
		}

		virtual public void StartStreaming ()
		{
			//enable streaming of events, markers, and rigids
			owlSetInteger (OWL_EVENTS, OWL_ENABLE);
			owlSetInteger (OWL_MARKERS, OWL_ENABLE);
			owlSetInteger (OWL_RIGIDS, OWL_ENABLE);

			// Recap enables planes, peaks, images,
			// and commdata by default, we don't want them.
			owlSetInteger (OWL_PLANES, OWL_DISABLE);
			owlSetInteger (OWL_PEAKS, OWL_DISABLE);
			owlSetInteger (OWL_IMAGES, OWL_DISABLE);
			owlSetInteger (OWL_COMMDATA, OWL_DISABLE);

			// make sure nothing went wrong
			owlGetStatus ();
			while (true) {
				int err = owlGetError ();
				if (err == OWL_NO_ERROR) {
					break;
				} else {
					print(String.Format ("owl set error: 0x{0,0:X}", err));
				}
			}

			// owlScale
			Scale = Scale;
			Interpolation = Interpolation;

			// start streaming
			owlSetInteger (OWL_STREAMING, OWL_ENABLE);

			// make sure nothing went wrong
			owlGetStatus ();
			while (true) {
				int err = owlGetError ();
				if (err == OWL_NO_ERROR) {
					break;
				} else {
					error = err;
					connected = false;
					owlDone ();
					return;
				}
			}
		}
		//
		virtual public void Disconnect ()
		{
			owlDone ();
			numCameras = 0;
			numRigids = 0;
			numMarkers = 0;
			connected = false;
		}

		//
		virtual public void CreatePointTracker (int id, int[] leds)
		{
			owlTrackeri (id, OWL_CREATE, OWL_POINT_TRACKER);
			for (int i = 0; i < leds.Length; i++) {
				owlMarkeri (MARKER (id, i), OWL_SET_LED, leds [i]);
			}
			owlTracker (id, OWL_ENABLE);

			int err = owlGetError ();
			if (err != OWL_NO_ERROR) {
				error = err;
				connected = false;
				owlDone ();
				throw new OWLException (String.Format ("owl error: 0x{0,0:X}", err));
			}
		}

		virtual public void ParseRB (string text, out int[] _leds, out Vector3[] _points)
		{

			string [] delim1 = {"\n"};
			string [] delim2 = {",", " "};
			string [] lines = text.Split (delim1, StringSplitOptions.RemoveEmptyEntries);

			List<int> leds = new List<int> ();
			List<Vector3> positions = new List<Vector3> ();

			for (int i = 0; i < lines.Length; i++) {
				string [] elems = lines [i].Split (delim2, StringSplitOptions.RemoveEmptyEntries);
				if (elems.Length < 4)
					throw new OWLException ("error parsing rb file");
				int led = Convert.ToInt32 (elems [0]);
				leds.Add (led);
				Vector3 v = new Vector3 ();
				v.x = Convert.ToSingle (elems [1]);
				v.y = Convert.ToSingle (elems [2]);
				v.z = Convert.ToSingle (elems [3]);
				positions.Add (v);
			}

			_leds = leds.ToArray ();
			_points = positions.ToArray ();
		}

		//
		virtual public void CreateRigidTracker (int id, string rbfile)
		{
			byte [] b = System.IO.File.ReadAllBytes (rbfile);
			string s = System.Text.Encoding.UTF8.GetString (b);

			int [] leds;
			Vector3 [] points;
			ParseRB (s, out leds, out points);
			CreateRigidTracker (id, leds, points);

		}

		//
		virtual public void CreateRigidTracker (int id, TextAsset rbtext)
		{

			int [] leds;
			Vector3 [] points;
			ParseRB (rbtext.ToString (), out leds, out points);
			CreateRigidTracker (id, leds, points);


		}

		// points must be in PhaseSpace's coordinate system
		virtual public void CreateRigidTracker (int id, int[] leds, Vector3[] points)
		{
			// create tracker
			owlTrackeri (id, OWL_CREATE, OWL_RIGID_TRACKER);

			// add markers to tracker
			for (int i = 0; i < points.Length; i++) {
				int m_id = MARKER ((int)id, (int)leds [i]);
				float [] pos = new float[3];
				pos [0] = points [i].x;
				pos [1] = points [i].y;
				pos [2] = points [i].z;
				owlMarkeri (m_id, OWL_SET_LED, (int)leds [i]);
				owlMarkerfv (m_id, OWL_SET_POSITION, pos, (uint)pos.Length);
			}

			owlTracker (id, OWL_ENABLE);

			int err = owlGetError ();
			if (err != OWL_NO_ERROR) {
				error = err;
				connected = false;
				owlDone ();
				throw new OWLException (String.Format ("owl error: 0x{0,0:X}", err));
			}
		}


		// Call in main loop to update OWL data
		virtual public int UpdateOWL ()
		{
			if (!connected)
				return 0;

			int frames = 0;

			// check OWL events until none are left
			OWLEvent e = owlGetEvent ();

			int count = 0;
			while (e.type != 0 && count < 32) {
				count += 1;
				int err = owlGetError ();
				if (err != OWL_NO_ERROR) {
					error = err;
					connected = false;
					owlDone ();
					throw new OWLException (String.Format ("owl error: 0x{0,0:X}", err));
				}

				// read data for each event
				switch (e.type) {
				case OWL_FRAME_NUMBER:
					frame = e.frame;
					frames++;
					break;
				case OWL_MARKERS:
					numMarkers = owlGetMarkers (markers, (uint)markers.Length);
					dirtyFlag |= OWL_MARKERS;
					if (numMarkers < 0)
						numMarkers = 0;
					break;
				case OWL_RIGIDS:
					numRigids = owlGetRigids (rigids, (uint)rigids.Length);
					dirtyFlag |= OWL_RIGIDS;
					if (numRigids < 0)
						numRigids = 0;
					break;
				case OWL_CAMERAS:
					numCameras = owlGetCameras (cameras, (uint)cameras.Length);
					dirtyFlag |= OWL_CAMERAS;
					if (numCameras < 0)
						numCameras = 0;
					break;
				case OWL_PLANES:
					numPlanes = owlGetPlanes (planes, (uint)planes.Length);
					break;
				case OWL_PEAKS:
					numPeaks = owlGetPeaks (peaks, (uint)peaks.Length);
					break;
				case OWL_COMMDATA:
					owlGetString (OWL_COMMDATA);
					break;
				case OWL_FREQUENCY:
					break;
				default:
					throw new OWLException (String.Format ("unknown event: 0x{0,0:X}", e.type));
				}

				// get next event
				e = owlGetEvent ();
			}

			// force camera acquisition
			if (numCameras == 0) {
				numCameras = owlGetCameras (cameras, (uint)cameras.Length);
				dirtyFlag |= OWL_CAMERAS;
				if (numCameras < 0)
					numCameras = 0;
			}

			return frames;
		}

		// Turn OWLMarkers, OWLRigids, and OWLCameras to Markers, Rigids, and Cameras
		// and convert data to Unity's coordinate system.
		virtual protected void ConvertData ()
		{
			if ((dirtyFlag & OWL_RIGIDS) == OWL_RIGIDS) {
				for (int i = 0; i < numRigids; i++) {
					// convert to Unity coordinate system
					float [] pose = rigids [i].pose;
					outrigids [i] = new Rigid (rigids [i].id, rigids [i].cond,
				                           new Vector3 (pose [0], pose [1], -pose [2]),
				                           new Quaternion (-pose [4], -pose [5], pose [6], pose [3]) * rigidoffset);
				}
			}
			if ((dirtyFlag & OWL_MARKERS) == OWL_MARKERS) {
				for (int i = 0; i < numMarkers; i++) {
					// convert to Unity coordinate system
					outmarkers [i] = new Marker (markers [i].id, markers [i].cond,
				                             new Vector3 (markers [i].x, markers [i].y, -markers [i].z));
				}
			}
			if ((dirtyFlag & OWL_CAMERAS) == OWL_CAMERAS) {
				for (int i = 0; i < numCameras; i++) {
					// convert to Unity coordinate system
					float [] pose = cameras [i].pose;
					outcameras [i] = new Camera (rigids [i].id, rigids [i].cond,
				                             new Vector3 (pose [0], pose [1], -pose [2]),
				                             new Quaternion (-pose [4], -pose [5], pose [6], pose [3]));
				}
			}

			dirtyFlag = 0;
		}

		//
		virtual public Rigid GetRigid (int tracker_id)
		{
			if (dirtyFlag != 0)
				ConvertData ();
			for (int i = 0; i < numRigids; i++) {
				if (outrigids [i].id == tracker_id)
					return outrigids [i];
			}
			return null;
		}

		//
		virtual public Rigid [] GetRigids ()
		{
			if (dirtyFlag != 0)
				ConvertData ();
			Rigid [] o = new Rigid[numRigids];
			Array.Copy (outrigids, o, numRigids);
			return o;
		}

		//
		virtual public Camera [] GetCameras ()
		{
			if (dirtyFlag != 0)
				ConvertData ();
			Camera [] o = new Camera[numCameras];
			Array.Copy (outcameras, o, numCameras);
			return o;
		}

		//
		virtual public Marker GetMarker (int tracker_id, int index)
		{
			if (dirtyFlag != 0)
				ConvertData ();

			int id = MARKER (tracker_id, index);
			for (int i = 0; i < numMarkers; i++) {
				if (outmarkers [i].id == id)
					return outmarkers [i];
			}
			return null;
		}

		//
		virtual public Marker [] GetMarkers ()
		{
			if (dirtyFlag != 0)
				ConvertData ();
			Marker [] o = new Marker[numMarkers];
			Array.Copy (outmarkers, o, numMarkers);
			return o;
		}

		virtual public Server [] GetServers()
		{
			Server [] o = new Server[outservers.Length];
			Array.Copy (outservers, o, outservers.Length);
			return o;
		}

	}
}