using UnityEngine;

//
// Class for drawing markers, rigids, and cameras
//
public class OWLRenderer : MonoBehaviour
{
  protected static Material lineMaterial = null;
  public OWLTracker tracker;

  //
  protected static void CreateLineMaterial()
  {
    if(lineMaterial == null)
    {
      lineMaterial = new Material( "Shader \"Lines/Colored Blended\" {" +
                                   "SubShader { Pass { " +
                                   " Blend SrcAlpha OneMinusSrcAlpha " +
                                   " ZWrite Off Cull Off Fog { Mode Off } " +
                                   " BindChannels {" +
                                   " Bind \"vertex\", vertex Bind \"color\", color }" +
                                   "} } }" );
      lineMaterial.hideFlags = HideFlags.HideAndDontSave;
      lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
    }
  }

  //
  void OnPostRender()
  {

    PSMarker [] markers = tracker.OWL.GetMarkers();
    PSRigid [] rigids = tracker.OWL.GetRigids();
    PSCamera [] cameras = tracker.OWL.GetCameras();

    CreateLineMaterial();
	lineMaterial.SetPass(0);

    GL.Color(new Color(1, 1, 0, 1));

    // draw markers
    GL.Color(new Color(1, 1, 0, 1));
    for(int i = 0; i < markers.Length; i++)
    {
      if(markers[i].cond > 0)
      {
        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.TRS(markers[i].position, Quaternion.identity, new Vector3(1, 1, 1)));
        DrawCube();
        GL.PopMatrix();
      }
    }

    // draw rigids
    for(int i = 0; i < rigids.Length; i++)
    {
      if(rigids[i].cond > 0)
      {
        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.TRS(rigids[i].position, rigids[i].rotation, new Vector3(1,1,1)));
        DrawAxes();
        GL.PopMatrix();
      }
    }

    // draw cameras
    for(int i = 0; i < cameras.Length; i++)
    {
      GL.PushMatrix();
      GL.MultMatrix(Matrix4x4.TRS(cameras[i].position, cameras[i].rotation, new Vector3(1,1,1)));
      GL.Color(new Color(1, 1, 1, 1));
      DrawCube();
      DrawAxes();
      GL.Color(new Color(1, 1, 1, 1));
      GL.PopMatrix();
      GL.Begin(GL.LINES);
      GL.Vertex3(cameras[i].position.x, cameras[i].position.y, cameras[i].position.z);
      GL.Vertex3(cameras[i].position.x, 0, cameras[i].position.z);
      float s = 0.05f;
      GL.Vertex3(cameras[i].position.x + s, 0, cameras[i].position.z + s);
      GL.Vertex3(cameras[i].position.x - s, 0, cameras[i].position.z - s);
      GL.Vertex3(cameras[i].position.x + s, 0, cameras[i].position.z - s);
      GL.Vertex3(cameras[i].position.x - s, 0, cameras[i].position.z + s);
      GL.End();
    }

    GL.End();
  }

  //
  void DrawCube()
  {
    float s = 0.02f;

    GL.Begin(GL.LINES);
    GL.Vertex3(-s, -s, -s);
    GL.Vertex3( s, -s, -s);

    GL.Vertex3(-s, -s, -s);
    GL.Vertex3(-s,  s, -s);

    GL.Vertex3( s, -s, -s);
    GL.Vertex3( s, s, -s);

    GL.Vertex3(-s, s, -s);
    GL.Vertex3( s, s, -s);

    GL.Vertex3(-s, -s, s);
    GL.Vertex3( s, -s, s);

    GL.Vertex3(-s, -s, s);
    GL.Vertex3(-s, s, s);

    GL.Vertex3( s, -s, s);
    GL.Vertex3( s, s, s);

    GL.Vertex3(-s, s, s);
    GL.Vertex3( s, s, s);

    GL.Vertex3(-s, -s, -s);
    GL.Vertex3(-s, -s,  s);

    GL.Vertex3(-s, s, -s);
    GL.Vertex3(-s, s, s);

    GL.Vertex3( s, -s, -s);
    GL.Vertex3( s, -s,  s);

    GL.Vertex3( s, s, -s);
    GL.Vertex3( s, s,  s);
    GL.End();
  }

  //
  void DrawAxes()
  {
    float s = 0.08f;
    GL.Begin(GL.LINES);
    GL.Color(new Color(1, 0, 0, 1));
    GL.Vertex3(0, 0, 0);
    GL.Vertex3(s, 0, 0);
    GL.Color(new Color(0, 1, 0, 1));
    GL.Vertex3(0, 0, 0);
    GL.Vertex3(0, s, 0);
    GL.Color(new Color(0.2f, 0.2f, 0.8f, 1));
    GL.Vertex3(0, 0, 0);
    GL.Vertex3(0, 0, s);
    GL.End();
  }
}