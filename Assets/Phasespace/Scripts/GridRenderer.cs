using UnityEngine;

//
// Class for drawing the grid and axes
//
public class GridRenderer : MonoBehaviour
{

  protected static Material lineMaterial = null;

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
	CreateLineMaterial();
	// set the current material
	lineMaterial.SetPass(0);

    // draw grid
    GL.PushMatrix();
	GL.Begin(GL.LINES);
    float scale = 10.0f;
    float div = 10f;
    float div2 = 100f;
    float seg = scale / div;
    float seg2 = scale / div2;
    float ex = scale / 2f;
	GL.Color(new Color(0.2f, 0.2f, 0.2f, 0.4f));
    for(float i = -ex; i <= ex; i+=seg2)
    {
      GL.Vertex3(i, 0, -ex);
      GL.Vertex3(i, 0,  ex);
      GL.Vertex3(ex, 0, i);
      GL.Vertex3(-ex, 0, i);
    }
	GL.Color(new Color(0.4f, 0.4f, 0.4f, 0.8f));
    for(float i = -ex; i <= ex; i+=seg)
    {
      GL.Vertex3(i, 0, -ex);
      GL.Vertex3(i, 0,  ex);
      GL.Vertex3(ex, 0, i);
      GL.Vertex3(-ex, 0, i);
    }
	GL.Color(new Color(0.8f, 0.8f, 0.8f, 0.8f));
    GL.Vertex3(0, 0, -ex);
    GL.Vertex3(0, 0, ex);
    GL.Vertex3(ex, 0, 0);
    GL.Vertex3(-ex, 0, 0);
	GL.End();

    GL.PopMatrix();

    // draw compass
    float s = 1.0f;
    GL.PushMatrix();
    GL.Viewport(new Rect(0, 0, Screen.width / 8, Screen.width / 8));
    GL.modelview = Matrix4x4.TRS(new Vector3(0, 0, -2), Quaternion.Inverse(transform.rotation), new Vector3(1, 1, 1));
	GL.Begin(GL.LINES);
    GL.Color(new Color(1f, 0f, 0f, 1f));
    GL.Vertex3(0, 0, 0);
    GL.Vertex3(s, 0, 0);
    GL.Color(new Color(0f, 1f, 0f, 1f));
    GL.Vertex3(0, 0, 0);
    GL.Vertex3(0, s, 0);
    GL.Color(new Color(0.2f, 0.2f, 8f, 1f));
    GL.Vertex3(0, 0, 0);
    GL.Vertex3(0, 0, s);
    GL.End();
    GL.Viewport(new Rect(0, 0, Screen.width, Screen.width));
    GL.PopMatrix();
  }
}