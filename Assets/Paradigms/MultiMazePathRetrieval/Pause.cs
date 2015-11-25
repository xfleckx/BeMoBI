using UnityEngine;
using System.Collections;

namespace Assets.Paradigms.MultiMazePathRetrieval
{
    public class Pause : Trial
    {
        public override void Initialize(string mazeName, int pathID, string category, string objectName)
        {
            // does actual nothing
        }

        public override void StartTrial()
        {
            OnBeforeStart();
            marker.Write(string.Format(MarkerPattern.BeginTrial, GetType().Name, -1, -1, -1));
            stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            fading.StartFadeOut();
        }

        public override void ForceTrialEnd()
        {
            fading.StartFadeIn();

            base.ForceTrialEnd();
        }
        
    }
}
