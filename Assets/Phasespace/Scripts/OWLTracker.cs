//
// PhaseSpace, Inc. 2014
//
using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

// generic exception for OWL related errors
public class OWLException : Exception
{
  //
  public OWLException() :
  base("OWL exception")
  {

  }

  //
  public OWLException(string msg) :
  base(msg)
  {

  }
}

// output type
public class PSMarker
{
  public int id;
  public float cond;
  public Vector3 position;

  public PSMarker(int id, float cond, Vector3 pos)
  {
    this.id = id;
    this.cond = cond;
    this.position = pos;
  }
}

// output type
public class PSRigid : PSMarker
{
  public Quaternion rotation;

  public PSRigid(int id, float cond, Vector3 pos, Quaternion rot) :
  base(id, cond, pos)
  {
    rotation = rot;
  }
}

// output type
public class PSCamera : PSRigid
{
  public PSCamera(int id, float cond, Vector3 pos, Quaternion rot) :
  base(id, cond, pos, rot)
  {

  }
}

//
// wrapper class for libowlsock
//
public class OWLWrapper : libowlsock
{
  protected bool connected = false;

  // lastest reported condition
  public int error = 0;

  // latest reported frame
  public int frame;

  // incoming data buffers
  protected OWLMarker [] markers = new OWLMarker[MAX_MARKERS];
  protected OWLRigid [] rigids = new OWLRigid[MAX_RIGIDS];
  protected OWLCamera [] cameras = new OWLCamera[MAX_CAMERAS];
  protected OWLPlane [] planes = new OWLPlane[MAX_PLANES];
  protected OWLPeak [] peaks = new OWLPeak[MAX_PEAKS];

  protected int numMarkers = 0;
  protected int numRigids = 0;
  protected int numCameras = 0;
  protected int numPlanes = 0;
  protected int numPeaks = 0;

  public int NumRigids
  {
    get { return numRigids; }
  }
  public int NumMarkers
  {
    get { return numMarkers; }
  }
  public int NumCameras
  {
    get { return numCameras; }
  }

  // outgoing data buffers
  protected int dirtyFlag = 0;
  protected PSMarker [] outmarkers = new PSMarker[MAX_MARKERS];
  protected PSRigid [] outrigids = new PSRigid[MAX_RIGIDS];
  protected PSCamera [] outcameras = new PSCamera[MAX_RIGIDS];

  //
  const int MAX_MARKERS = 1024;
  const int MAX_RIGIDS = 256;
  const int MAX_CAMERAS = 128;
  const int MAX_PLANES = 1024;
  const int MAX_PEAKS = 1024;

  //
  public bool Connected()
  {
    return connected;
  }

  //
  public bool Connect(string server, bool slave)
  {
    if(connected)
      return false;

    // connect to OWL server in slave mode
    int flag = 0;
    if(slave) flag |= OWL_SLAVE;
    int ret = owlInit(server, flag);
    if(ret < 0)
    {
      error = ret;
      connected = false;
      return false;
    }

    connected = true;

    error = 0;

    // query server version
    System.Console.WriteLine(System.Text.Encoding.UTF8.GetString(owlGetString(OWL_VERSION)));

    // set streaming frequency
    if(!slave) owlSetFloat(OWL_FREQUENCY, OWL_MAX_FREQUENCY);

    // make sure nothing went wrong
    owlGetStatus();
    while(true) {
      int err = owlGetError();
      if(err == OWL_NO_ERROR) {
        break;
      } else {
        error = err;
        connected = false;
        owlDone();
        return false;
      }
    }
    connected = true;
    return true;
  }

  //
  public void Disconnect()
  {
    owlDone();
    numCameras = 0;
    numRigids = 0;
    numMarkers = 0;
    connected = false;
  }

  //
  public void Start()
  {

    //enable streaming of events, markers, and rigids
    owlSetInteger(OWL_EVENTS, OWL_ENABLE);
    owlSetInteger(OWL_MARKERS, OWL_ENABLE);
    owlSetInteger(OWL_RIGIDS, OWL_ENABLE);

    // Recap enables planes, peaks, images,
    // and commdata by default, we don't want them.
    owlSetInteger(OWL_PLANES, OWL_DISABLE);
    owlSetInteger(OWL_PEAKS, OWL_DISABLE);
    owlSetInteger(OWL_IMAGES, OWL_DISABLE);
    owlSetInteger(OWL_COMMDATA, OWL_DISABLE);

    // autoscale incoming data to fit Unity
    owlScale(0.001f); // mm to meters

    // start streaming
    owlSetInteger(OWL_STREAMING, OWL_ENABLE);

    // make sure nothing went wrong
    owlGetStatus();
    while(true) {
      int err = owlGetError();
      if(err == OWL_NO_ERROR) {
        break;
      } else {
        error = err;
        connected = false;
        owlDone();
        return;
      }
    }
  }


  //
  public void CreatePointTracker(int id, int [] leds)
  {
    owlTrackeri(id, OWL_CREATE, OWL_POINT_TRACKER);
    for(int i = 0; i < leds.Length; i++)
    {
      owlMarkeri(MARKER(id, i), OWL_SET_LED, leds[i]);
    }
    owlTracker(id, OWL_ENABLE);

    int err = owlGetError();
    if(err != OWL_NO_ERROR)
    {
      error = err;
      connected = false;
      owlDone();
      throw new OWLException(String.Format("owl condition: 0x{0,0:X}", err));
    }
  }

  public void CreateRigidTrackerFrom(int id, TextAsset rbDefinition)
  {
    string s = rbDefinition.text;

    string [] delim1 = {"\n"};
    string [] delim2 = {",", " "};
    string [] lines = s.Split(delim1, StringSplitOptions.RemoveEmptyEntries);

    // create tracker
    owlTrackeri(id, OWL_CREATE, OWL_RIGID_TRACKER);

    // parse rbfile
    for(int i = 0; i < lines.Length; i++)
    {
      string [] elems = lines[i].Split(delim2, StringSplitOptions.RemoveEmptyEntries);
      if(elems.Length < 4)
        throw new OWLException("condition parsing rb file");
      uint led = Convert.ToUInt32(elems[0]);
      float [] pos = new float[3];
      pos[0] = Convert.ToSingle(elems[1]);
      pos[1] = Convert.ToSingle(elems[2]);
      pos[2] = Convert.ToSingle(elems[3]);
      //print(String.Format("{0}, {1} {2} {3}", led, pos[0], pos[1], pos[2]));

      // add marker to tracker
      owlMarkeri(MARKER((int) id, (int) led), OWL_SET_LED, (int) led);
      owlMarkerfv(MARKER((int) id, (int) led), OWL_SET_POSITION, pos, (uint) pos.Length);
    }

    owlTracker(id, OWL_ENABLE);

    int err = owlGetError();
    if(err != OWL_NO_ERROR)
    {
      error = err;
      connected = false;
      owlDone();
      throw new OWLException(String.Format("owl condition: 0x{0,0:X}", err));
    }

  }

  //
  public void CreateRigidTracker(int id, string rbfile)
  {
    byte [] b = System.IO.File.ReadAllBytes(rbfile);
    string s = System.Text.Encoding.UTF8.GetString(b);

    string [] delim1 = {"\n"};
    string [] delim2 = {",", " "};
    string [] lines = s.Split(delim1, StringSplitOptions.RemoveEmptyEntries);

    // create tracker
    owlTrackeri(id, OWL_CREATE, OWL_RIGID_TRACKER);

    // parse rbfile
    for(int i = 0; i < lines.Length; i++)
    {
      string [] elems = lines[i].Split(delim2, StringSplitOptions.RemoveEmptyEntries);
      if(elems.Length < 4)
        throw new OWLException("condition parsing rb file");
      uint led = Convert.ToUInt32(elems[0]);
      float [] pos = new float[3];
      pos[0] = Convert.ToSingle(elems[1]);
      pos[1] = Convert.ToSingle(elems[2]);
      pos[2] = Convert.ToSingle(elems[3]);
      //print(String.Format("{0}, {1} {2} {3}", led, pos[0], pos[1], pos[2]));

      // add marker to tracker
      owlMarkeri(MARKER((int) id, (int) led), OWL_SET_LED, (int) led);
      owlMarkerfv(MARKER((int) id, (int) led), OWL_SET_POSITION, pos, (uint) pos.Length);
    }

    owlTracker(id, OWL_ENABLE);

    int err = owlGetError();
    if(err != OWL_NO_ERROR)
    {
      error = err;
      connected = false;
      owlDone();
      throw new OWLException(String.Format("owl condition: 0x{0,0:X}", err));
    }
  }

  // Call in main loop to update OWL data
  public void Update()
  {
    if(!connected)
      return;

    // check OWL events until none are left
    OWLEvent e = owlGetEvent();

    int count = 0;
    while(e.type != 0 && count < 512)
      {
        count += 1;

        int err = owlGetError();
        if(err != OWL_NO_ERROR)
          {
            error = err;
            connected = false;
            owlDone();
            throw new OWLException(String.Format("owl condition: 0x{0,0:X}", err));
          }

        // read data for each event
        switch(e.type)
          {
          case OWL_FRAME_NUMBER:
            frame = e.frame;
            break;
          case OWL_MARKERS:
            numMarkers = owlGetMarkers(markers, (uint) markers.Length);
            dirtyFlag |= OWL_MARKERS;
            if(numMarkers < 0) numMarkers = 0;
            break;
          case OWL_RIGIDS:
            numRigids = owlGetRigids(rigids, (uint) rigids.Length);
            dirtyFlag |= OWL_RIGIDS;
            if(numRigids < 0) numRigids = 0;
            break;
          case OWL_CAMERAS:
            numCameras = owlGetCameras(cameras, (uint) cameras.Length);
            dirtyFlag |= OWL_CAMERAS;
            if(numCameras < 0) numCameras = 0;
            break;
          case OWL_PLANES:
            numPlanes = owlGetPlanes(planes, (uint) planes.Length);
            break;
          case OWL_PEAKS:
            numPeaks = owlGetPeaks(peaks, (uint) peaks.Length);
            break;
          case OWL_COMMDATA:
            owlGetString(OWL_COMMDATA);
            break;
          default:
            throw new OWLException(String.Format("unknown event: 0x{0,0:X}", e.type));
            break;
          }

        // get next event
        e = owlGetEvent();
      }

    // force camera acquisition
    if(numCameras == 0)
    {
      numCameras = owlGetCameras(cameras, (uint) cameras.Length);
      dirtyFlag |= OWL_CAMERAS;
      if(numCameras < 0) numCameras = 0;
    }
  }

  //
  protected void ConvertData()
  {
    if((dirtyFlag & OWL_RIGIDS) == OWL_RIGIDS)
    {
      for(int i = 0; i < numRigids; i++)
      {
        // convert to Unity coordinate system
        float [] pose = rigids[i].pose;
        outrigids[i] = new PSRigid(rigids[i].id, rigids[i].cond,
                                   new Vector3(pose[0], pose[1], -pose[2]),
                                   new Quaternion(-pose[4], -pose[5], pose[6], pose[3]));
      }
    }
    if((dirtyFlag & OWL_MARKERS) == OWL_MARKERS)
    {
      for(int i = 0; i < numMarkers; i++)
      {
        // convert to Unity coordinate system
        outmarkers[i] = new PSMarker(markers[i].id, markers[i].cond,
                                     new Vector3(markers[i].x, markers[i].y, -markers[i].z));
      }
    }
    if((dirtyFlag & OWL_CAMERAS) == OWL_CAMERAS)
    {
      for(int i = 0; i < numCameras; i++)
      {
        // convert to Unity coordinate system
        float [] pose = cameras[i].pose;
        outcameras[i] = new PSCamera(rigids[i].id, rigids[i].cond,
                                     new Vector3(pose[0], pose[1], -pose[2]),
                                     new Quaternion(-pose[4], -pose[5], pose[6], pose[3]));
      }
    }

    dirtyFlag = 0;
  }

  //
  public PSRigid GetRigid(int tracker_id)
  {
    if(dirtyFlag != 0)
      ConvertData();
    for(int i = 0; i < numRigids; i++)
    {
      if(outrigids[i].id == tracker_id)
        return outrigids[i];
    }
    return null;
  }

  //
  public PSRigid [] GetRigids()
  {
    if(dirtyFlag != 0)
      ConvertData();
    PSRigid [] o = new PSRigid[numRigids];
    Array.Copy(outrigids, o, numRigids);
    return o;
  }

  //
  public PSCamera [] GetCameras()
  {
    if(dirtyFlag != 0)
      ConvertData();
    PSCamera [] o = new PSCamera[numCameras];
    Array.Copy(outcameras, o, numCameras);
    return o;
  }

  //
  public PSMarker GetMarker(int tracker_id, int index)
  {
    if(dirtyFlag != 0)
      ConvertData();

    int id = MARKER(tracker_id, index);
    for(int i = 0; i < numMarkers; i++)
    {
      if(outmarkers[i].id == id)
        return outmarkers[i];
    }
    return null;
  }

  //
  public PSMarker [] GetMarkers()
  {
    if(dirtyFlag != 0)
      ConvertData();
    PSMarker [] o = new PSMarker[numMarkers];
    Array.Copy(outmarkers, o, numMarkers);
    return o;
  }

}

//
//
//
public class OWLTracker : MonoBehaviour {

  //
  public OWLWrapper OWL = new OWLWrapper();

  // if attached to a camera, enable to get data at a time closer to actual rendering
  public bool updateOnPreRender = false;

  //
  void Start () {

  }

  //
  void Awake ()
  {
    print("Creating OWLTracker...");
  }

  //
  void OnPreRender () {
    // only works if attached to a camera
    if(updateOnPreRender) OWL.Update();
  }

  //
  void Update()
  {
    if(!updateOnPreRender) OWL.Update();
  }

  //
  void OnDestroy()
  {
    // disconnect from OWL server
    OWL.Disconnect();
  }
}
