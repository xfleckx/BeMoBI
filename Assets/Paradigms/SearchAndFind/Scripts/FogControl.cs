using UnityEngine;
using System.Collections;
using Assets.Paradigms.SearchAndFind.ImageEffects;

namespace Assets.BeMoBI.Paradigms.SearchAndFind.Scripts
{
    public class FogControl : MonoBehaviour
    {
        public CustomGlobalFog fogEffect;
        
        [Tooltip("Fog top Y coordinate")]
        public float Target_Height = 1.0f;
        [Range(0.001f, 10.0f)]
        public float Target_heightDensity = 2.0f;
        [Tooltip("Push fog away from the camera by this amount")]
        public float Target_startDistance = 0.0f;
        
        [Tooltip("Fog top Y coordinate")]
        public float Source_Height = 1.0f;
        [Range(0.001f, 10.0f)]
        public float Source_heightDensity = 2.0f;
        [Tooltip("Push fog away from the camera by this amount")]
        public float Source_startDistance = 0.0f;

          
        private float current_Height = 1.0f;
        private float current_Density = 2.0f;
        private float current_Distance = 0.0f;


        public void RaiseFog()
        {

        }

        IEnumerator RaiseToTargetValues()
        {
            yield return new WaitWhile(FogHasRaisedCompletely);
        }

        bool FogHasRaisedCompletely()
        {
            var result = current_Height == Target_Height &&
                         current_Density == Target_heightDensity &&
                         current_Distance == Target_startDistance;

            return result;
        }

        public void LetFogDisappeare()
        {

        }

        bool FogHasDisappearedCompletely()
        {
            var result = current_Height == Source_Height &&
                         current_Density == Source_heightDensity &&
                         current_Distance == Source_startDistance;

            return result;
        }
    }

}
