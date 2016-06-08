using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using LSL;
using Assets.LSL4Unity.Scripts;

public class AppRuntimeStatistics : MonoBehaviour {

    public HUD_DEBUG hud;

    public bool publishToLSL = true;

    public float frameTimeAvgInterval = 0.5f;

    #region lsl
    
    private const string unique_source_id = "E493C423896E4783A004E93AA3D81051";

    private liblsl.StreamOutlet outlet;
    private liblsl.StreamInfo streamInfo;
    private float[] currentSample;

    public string StreamName = "BeMoBI.Unity3D.AppStatistics";
    public string StreamType = "Unity3D.FPS.FT";
    public int ChannelCount = 2;

    #endregion

    #region FPS Counter
    private readonly int fixedFPSIntervall = 1;

    private int lastFrameCount = 0;
    private float fps = 0;
    private float avgFrameTime;
    
    List<float> frameTimeBuffer = new List<float>();
    
    #endregion
    
    void Start()
    {
        if (!publishToLSL)
            return;

        currentSample = new float[ChannelCount];

        streamInfo = new liblsl.StreamInfo(StreamName, StreamType, ChannelCount, Time.fixedDeltaTime, liblsl.channel_format_t.cf_float32, unique_source_id);

        try
        {
            outlet = new liblsl.StreamOutlet(streamInfo);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
        
        StartCoroutine(GetFPS());
        StartCoroutine(GetAvgFrameTime());
    }

    void Update () {

        frameTimeBuffer.Add(Time.unscaledDeltaTime);

        if (hud != null)
            hud.UpdateFpsAndFTView(fps, avgFrameTime);
    }

    IEnumerator GetFPS()
    {
        while (true)
        {
            //int currentFrameCount = Time.renderedFrameCount;
            //fps = currentFrameCount - lastFrameCount;
            //lastFrameCount = currentFrameCount;

            fps = 1 / Time.deltaTime;

            yield return new WaitForSeconds(fixedFPSIntervall);
        }
    }

    IEnumerator GetAvgFrameTime()
    {
        while (true)
        {
            if(frameTimeBuffer.Any())
                avgFrameTime = frameTimeBuffer.Average();

            frameTimeBuffer.Clear();

            yield return new WaitForSeconds(frameTimeAvgInterval);
        }
    }

    public void LateUpdate()
    {
        if (outlet == null)
            return;
        
        currentSample[0] = fps;
        currentSample[1] = Time.unscaledDeltaTime;
        outlet.push_sample(currentSample, LSLTimeSync.Instance.UpdateTimeStamp);
    }
}
