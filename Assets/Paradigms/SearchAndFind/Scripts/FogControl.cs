using UnityEngine;
using System.Collections;
using Assets.Paradigms.SearchAndFind.ImageEffects;

namespace Assets.BeMoBI.Paradigms.SearchAndFind.Scripts
{
    public class FogControl : MonoBehaviour
    {
        public CustomGlobalFog fogEffect;
        
        [Tooltip("Fog top Y coordinate")]
        public float HeightWhenRaised = 1.0f;
        [Range(0.001f, 10.0f)]
        public float HeightDensityWhenRaised = 2.0f;
        [Tooltip("Push fog away from the camera by this amount")]
        public float StartDistanceWhenRaised = 0.0f;

        public float RaisingSpeed = 0.1f;

        [Tooltip("Fog top Y coordinate when Fog disappeared")]
        public float HeightWhenDisappeared = 1.0f;
        [Range(0.001f, 10.0f)]
        public float DensityWhenDisappeared = 2.0f;
        [Tooltip("Push fog away from the camera by this amount")]
        public float StartDistanceWhenDisappeared = 0.0f;

        public float Disappearingspeed = 0.1f;

        public void RaiseFog()
        {
            StopCoroutine(ReduceFogToTargetValues());
            StartCoroutine(RaiseToTargetValues());
        }

        IEnumerator RaiseToTargetValues()
        {
            while (true)
            {
                if(fogEffect.height < HeightWhenRaised)
                    fogEffect.height += 1 * RaisingSpeed * Time.deltaTime;

                if (fogEffect.heightDensity < HeightDensityWhenRaised)
                    fogEffect.heightDensity += 1 * RaisingSpeed * Time.deltaTime;

                if(fogEffect.startDistance < StartDistanceWhenRaised)
                    fogEffect.startDistance += 1 * RaisingSpeed * Time.deltaTime;

                yield return new WaitWhile(FogHasRaisedCompletely);
            }
        }

        bool FogHasRaisedCompletely()
        {
            var result = fogEffect.height >= HeightWhenRaised &&
                         fogEffect.heightDensity >= HeightDensityWhenRaised &&
                         fogEffect.startDistance >= StartDistanceWhenRaised;

            return result;
        }

        public void LetFogDisappeare()
        {
            StopCoroutine(RaiseToTargetValues());
            StartCoroutine(ReduceFogToTargetValues());
        }

        IEnumerator ReduceFogToTargetValues()
        {
            var state = FogHasDisappearedCompletely();

            do
            {
                if (fogEffect.height > HeightWhenDisappeared)
                    fogEffect.height -= 1 * Disappearingspeed * Time.deltaTime;

                if (fogEffect.heightDensity > DensityWhenDisappeared)
                    fogEffect.heightDensity -= 1 * Disappearingspeed * Time.deltaTime;

                if (fogEffect.startDistance > StartDistanceWhenDisappeared)
                    fogEffect.startDistance -= 1 * Disappearingspeed * Time.deltaTime;

                yield return new WaitWhile(() => state);

            } while (!state);

            fogEffect.height = HeightWhenDisappeared;
            fogEffect.startDistance = StartDistanceWhenDisappeared;
            fogEffect.heightDensity = DensityWhenDisappeared;

        }

        bool FogHasDisappearedCompletely()
        {
            var result = fogEffect.height <= HeightWhenDisappeared &&
                         fogEffect.heightDensity <= DensityWhenDisappeared &&
                         fogEffect.startDistance <= StartDistanceWhenDisappeared;

            return result;
        }

        public void DisappeareImmediately()
        {
            fogEffect.height = HeightWhenDisappeared;
            fogEffect.heightDensity = DensityWhenDisappeared;
            fogEffect.startDistance = StartDistanceWhenDisappeared;
        }

        public void RaisedImmediately()
        {
            fogEffect.height = HeightWhenRaised;
            fogEffect.heightDensity = HeightDensityWhenRaised;
            fogEffect.startDistance = StartDistanceWhenRaised;
        }
    }

}
