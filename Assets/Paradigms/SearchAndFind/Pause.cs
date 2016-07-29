using UnityEngine;
using System.Collections;

namespace Assets.BeMoBI.Paradigms.SearchAndFind
{

    public class Pause : MonoBehaviour
    {
        ParadigmController paradigm;

        void Start()
        {
            if (paradigm == null)
                paradigm = FindObjectOfType<ParadigmController>();
        }

        public void WhenPauseBegin()
        {
            paradigm.fading.StartFadeOut();

            paradigm.marker.Write("Pause Begin");
        }

        public void WhenPauseEnds()
        {
            paradigm.fading.StartFadeIn();

            paradigm.marker.Write("Pause End");
        }
    }
}
