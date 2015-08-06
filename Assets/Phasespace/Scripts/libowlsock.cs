//
// PhaseSpace, Inc. 2014
//
using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

//
// C# wrapper for native libowlsock library
//
public class libowlsock {

  // structs from owl.h
  [StructLayout(LayoutKind.Sequential)]
  public struct OWLEvent {
    public int type;
    public int frame;
  }

  //
  [StructLayout(LayoutKind.Sequential)]
  public struct OWLMarker {
    public int id;
    public int frame;
    public float x, y, z;
    public float cond;
    public uint flag;
  }

  //
  [StructLayout(LayoutKind.Sequential)]
  public struct OWLRigid {
    public int id;
    public int frame;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7, ArraySubType = UnmanagedType.R4)]
    public float [] pose;
    public float cond;
    public uint flag;
  }

  //
  [StructLayout(LayoutKind.Sequential)]
  public struct OWLCamera {
    public int id;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7, ArraySubType = UnmanagedType.R4)]
    public float [] pose;
    float cond;
    public uint flag;
  }

  // structs from owl_planes.h
  [StructLayout(LayoutKind.Sequential)]
  public struct OWLPlane {
    public int id;
    public int camera;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.R4)]
    public float [] plane;
    public float cond;
    public uint flag;
  }

  // structs from owl_peaks.h
  [StructLayout(LayoutKind.Sequential)]
  public struct OWLPeak {
    public int id;
    public int frame;
    public int camera;
    public int detector;
    public int width;
    public uint flag;
    public float pos;
    public float amp;
  }

  // owl.h constants
  public const float OWL_MAX_FREQUENCY   = 960.0f;

  /* Errors */
  public const int OWL_NO_ERROR          = 0x0;
  public const int OWL_INVALID_VALUE     = 0x0020;
  public const int OWL_INVALID_ENUM      = 0x0021;
  public const int OWL_INVALID_OPERATION = 0x0022;

  /* Common Events */
  public const int OWL_DONE              = 0x0002;

  /* Common flags */
  public const int OWL_CREATE            = 0x0100;
  public const int OWL_DESTROY           = 0x0101;
  public const int OWL_ENABLE            = 0x0102;
  public const int OWL_DISABLE           = 0x0103;

  /* Init flags */
  public const int OWL_SLAVE             = 0x0001;  // socket only
  public const int OWL_FILE              = 0x0002;  // socket only
  public const int OWL_ASYNC             = 0x0008;  // socket only
  public const int OWL_POSTPROCESS       = 0x0010;
  public const int OWL_MODE1             = 0x0100;
  public const int OWL_MODE2             = 0x0200;
  public const int OWL_MODE3             = 0x0300;
  public const int OWL_MODE4             = 0x0400;
  public const int OWL_LASER             = 0x0A00;
  public const int OWL_CALIB             = 0x0C00;
  public const int OWL_DIAGNOSTIC        = 0x0D00;
  public const int OWL_CALIBPLANAR       = 0x0F00;

  /* Sets */
  public const int OWL_FREQUENCY         = 0x0200;
  public const int OWL_STREAMING         = 0x0201;  // socket only
  public const int OWL_INTERPOLATION     = 0x0202;
  public const int OWL_BROADCAST         = 0x0203;  // socket only
  public const int OWL_EVENTS            = 0x020F;  // socket only
  public const int OWL_BUTTONS           = 0x0210;
  public const int OWL_MARKERS           = 0x0211;
  public const int OWL_RIGIDS            = 0x0212;
  public const int OWL_COMMDATA          = 0x0220;
  public const int OWL_TIMESTAMP         = 0x0221;
  public const int OWL_PLANES            = 0x02A0;
  public const int OWL_DETECTORS         = 0x02A1;
  public const int OWL_PEAKS             = 0x02A2;
  public const int OWL_IMAGES            = 0x02A3;

  public const int OWL_CAMERAS           = 0x02A4;

  public const int OWL_FRAME_BUFFER_SIZE = 0x02B0;  // socket only

  public const int OWL_MARKER_STATS      = 0x02D0;
  public const int OWL_CAMERA_STATS      = 0x02D1;
  public const int OWL_MARKER_COVARIANCE = 0x02D5;

  public const int OWL_HW_CONFIG         = 0x02F0;

  public const int OWL_TRANSFORM         = 0xC200;  // camera transformation

  /* Trackers */
  public const int OWL_POINT_TRACKER     = 0x0300;
  public const int OWL_RIGID_TRACKER     = 0x0301;

  // planar tracker
  public const int OWL_PLANAR_TRACKER    = 0x030A;

  public const int OWL_SET_FILTER        = 0x0310;

  // undocumented freatures
  // use at your own risk
  public const int OWL_FEATURE0          = 0x03F0; // optical
  public const int OWL_FEATURE1          = 0x03F1; // offsets
  public const int OWL_FEATURE2          = 0x03F2; // projection
  public const int OWL_FEATURE3          = 0x03F3; // predicted
  public const int OWL_FEATURE4          = 0x03F4; // valid min
  public const int OWL_FEATURE5          = 0x03F5; // query min
  public const int OWL_FEATURE6          = 0x03F6; // storedepth
  public const int OWL_FEATURE7          = 0x03F7; //
  public const int OWL_FEATURE8          = 0x03F8; // rejection
  public const int OWL_FEATURE9          = 0x03F9; // filtering
  public const int OWL_FEATURE10         = 0x03FA; // window size
  public const int OWL_FEATURE11         = 0x03FB; // LS cutoff
  public const int OWL_FEATURE12         = 0x03FC; // off-fill
  public const int OWL_FEATURE_LAST      = 0x03FD; // last feature

  // calibration only
  public const int OWL_CALIB_TRACKER     = 0x0C01;
  public const int OWL_CALIB_RESET       = 0x0C10;
  public const int OWL_CALIB_LOAD        = 0x0C11;
  public const int OWL_CALIB_SAVE        = 0x0C12;
  public const int OWL_CALIBRATE         = 0x0C13;
  public const int OWL_RECALIBRATE       = 0x0C14;
  public const int OWL_CAPTURE_RESET     = 0x0C20;
  public const int OWL_CAPTURE_START     = 0x0C21;
  public const int OWL_CAPTURE_STOP      = 0x0C22;
  public const int OWL_CALIB_ACTIVE      = 0x0C30;

  // planar calib tracker
  public const int OWL_CALIBPL_TRACKER   = 0x0CA1;

  /* Markers */
  public const int OWL_SET_LED           = 0x0400;
  public const int OWL_SET_POSITION      = 0x0401;
  public const int OWL_CLEAR_MARKER      = 0x0402;

  /* Gets */
  public const int OWL_VERSION           = 0x0500;
  public const int OWL_FRAME_NUMBER      = 0x0510;
  public const int OWL_STATUS_STRING     = 0x0520;
  public const int OWL_CUSTOM_STRING     = 0x05F0;

  // calibration only
  public const int OWL_CALIB_STATUS      = 0x0C51;
  public const int OWL_CALIB_ERROR       = 0x0C52;

  /* Macros */
  public static int MARKER(int tracker, int index)
  {
    return (((tracker)<<12)|(index));
  }

  public static int INDEX(int id)
  {
    return ((id)&0x0fff);
  }

  public static int TRACKER(int id)
  {
    return ((id)>>12);
  }

  // DLL imports
  [DllImport("libowlsock")]
  public static extern int owlInit(string server, int flags);
  [DllImport("libowlsock")]
  public static extern void owlDone();

  [DllImport("libowlsock")]
  public static extern void owlSetFloat(uint pname, float param);
  [DllImport("libowlsock")]
  public static extern void owlSetInteger(uint pname, int param);
  [DllImport("libowlsock")]
  public static extern void owlSetFloatv(uint pname, [param: In,Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] float [] param, uint n);
  [DllImport("libowlsock")]
  public static extern void owlSetIntegerv(uint pname, [param: In,Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int [] param, uint n);
  [DllImport("libowlsock")]
  public static extern void owlSetString(uint pname, string str);

  [DllImport("libowlsock")]
  public static extern void owlTracker(int tracker, uint pname);
  [DllImport("libowlsock")]
  public static extern void owlTrackerf(int tracker, uint pname, float param);
  [DllImport("libowlsock")]
  public static extern void owlTrackeri(int tracker, uint pname, int param);
  [DllImport("libowlsock")]
  public static extern void owlTrackerfv(int tracker, uint pname, [param: In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] float [] param, uint n);
  [DllImport("libowlsock")]
  public static extern void owlTrackeriv(int tracker, uint pname, [param: In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int [] param, uint n);

  [DllImport("libowlsock")]
  public static extern void owlMarker(int marker, uint pname);
  [DllImport("libowlsock")]
  public static extern void owlMarkerf(int marker, uint pname, float param);
  [DllImport("libowlsock")]
  public static extern void owlMarkeri(int marker, uint pname, int param);
  [DllImport("libowlsock")]
  public static extern void owlMarkerfv(int marker, uint pname, [param: In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] float [] param, uint n);
  [DllImport("libowlsock")]
  public static extern void owlMarkeriv(int marker, uint pname, [param: In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int [] param, uint n);

  [DllImport("libowlsock")]
  public static extern void owlScale(float scale);
  [DllImport("libowlsock")]
  public static extern void owlLoadPose(float [] pose);

  [DllImport("libowlsock")]
  public static extern int owlGetStatus();
  [DllImport("libowlsock")]
  public static extern int owlGetError();

  [DllImport("libowlsock")]
  public static extern OWLEvent owlPeekEvent();
  [DllImport("libowlsock")]
  public static extern OWLEvent owlGetEvent();

  // owlGetMarkers
  [DllImport("libowlsock")]
  public static extern int owlGetMarkers([param: Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] OWLMarker [] markers, uint count);

  // owlGetRigids;
  [DllImport("libowlsock")]
  public static extern int owlGetRigids([param: Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] OWLRigid [] rigids, uint count);

  // owlGetCameras
  [DllImport("libowlsock")]
  public static extern int owlGetCameras([param: Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] OWLCamera [] cameras, uint count);

  // owlGetPlanes
  [DllImport("libowlsock")]
  public static extern int owlGetPlanes([param: Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] OWLPlane [] planes, uint count);

  // owlGetPeaks
  [DllImport("libowlsock")]
  public static extern int owlGetPeaks([param: Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] OWLPeak [] peaks, uint count);

  // owlGetFloatv
  [DllImport("libowlsock")]
  public static extern int owlGetFloatv(uint pname, [param: Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] float [] param,  uint n);
  public static float [] owlGetFloatv(uint pname)
  {
    const int n = 64;
    float [] array = new float[n];
    int ret = owlGetFloatv(pname, array, n);
    if(ret < 0)
      throw new OWLException();
    Array.Resize(ref array, ret);
    return array;
  }

  // owlGetIntegerv
  [DllImport("libowlsock")]
  public static extern int owlGetIntegerv(uint pname, [param: Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int [] param,  uint n);
  public static int [] owlGetIntegerv(uint pname)
  {
    const int n = 64;
    int [] array = new int[n];
    int ret = owlGetIntegerv(pname, array, n);
    if(ret < 0)
      throw new OWLException();
    Array.Resize(ref array, ret);
    return array;
  }

  // owlGetString
  [DllImport("libowlsock")]
  public static extern int owlGetString(uint pname, [param: Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte [] str,  uint n);
  public static byte [] owlGetString(uint pname)
  {
    const int n = 1024;
    byte [] str = new byte[n];
    int ret = owlGetString(pname, str, n);
    if(ret < 0)
      throw new OWLException();
    Array.Resize(ref str, ret);
    return str;
  }
}
