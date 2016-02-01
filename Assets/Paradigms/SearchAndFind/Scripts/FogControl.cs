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



        public void RaiseFog()
        {
            StartCoroutine(RaiseToTargetValues());
        }

        IEnumerator RaiseToTargetValues()
        {
            fogEffect.height += 0.1f;
            fogEffect.heightDensity += 0.1f;
            fogEffect.startDistance += 0.1f;
            
            yield return new WaitWhile(FogHasRaisedCompletely);
        }

        bool FogHasRaisedCompletely()
        {
            var result = current_Height >= Target_Height &&
                         current_Density >= Target_heightDensity &&
                         current_Distance >= Target_startDistance;

            return result;
        }

        public void LetFogDisappeare()
        {
            StartCoroutine(ReduceFogToTargetValues());
        }

        IEnumerator ReduceFogToTargetValues()
        {
            fogEffect.height -= 0.1f;
            fogEffect.heightDensity -= 0.1f;
            fogEffect.startDistance -= 0.1f;
            
            yield return new WaitWhile(FogHasDisappearedCompletely);
        }

        bool FogHasDisappearedCompletely()
        {
            var result = fogEffect.height <= Source_Height &&
                         fogEffect.heightDensity <= Source_heightDensity &&
                         fogEffect.startDistance <= Source_startDistance;

            return result;
        }
    }

}
