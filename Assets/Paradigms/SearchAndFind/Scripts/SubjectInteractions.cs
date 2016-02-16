using UnityEngine;
using System.Collections;
using WiimoteApi;


namespace Assets.BeMoBI.Paradigms.SearchAndFind { 

    /// <summary>
    /// Represents all possible interactions for subject regarding the paradigm or trial behaviour
    /// </summary>
    public class SubjectInteractions : MonoBehaviour {

        private const string SUBMIT_INPUT = "Subject_Submit";

        private const string REQUIRE_PAUSE = "Subject_Requires_Break";

        ParadigmController controller;

        private Wiimote wiimote;

        public bool TestMode = false;

        private bool homeButtonIsDown = false;

        public bool wiimoteDetected = false;

        void Awake()
        {
            controller = FindObjectOfType<ParadigmController>();
        }

	    // Update is called once per frame
	    void Update () {

            if (!WiimoteManager.HasWiimote())
            {
                //if(wiimoteDetected)

                wiimoteDetected = false;
                return;
            }


            wiimote = WiimoteManager.Wiimotes[0];

            wiimoteDetected = true;

            int ret;
            do
            {
                ret = wiimote.ReadWiimoteData();

                if (ret > 0 && wiimote.current_ext == ExtensionController.MOTIONPLUS)
                {
                    Vector3 offset = new Vector3(-wiimote.MotionPlus.PitchSpeed,
                                                    wiimote.MotionPlus.YawSpeed,
                                                    wiimote.MotionPlus.RollSpeed) / 95f; // Divide by 95Hz (average updates per second from wiimote)
                   
                }
            } while (ret > 0);


            // get the Buttons
            if (wiimote.Button.a)
            {
                controller.SubjectTriesToSubmit();
            }

            //model.b.enabled = wiimote.Button.b;

            //model.one.enabled = wiimote.Button.one;
            //model.two.enabled = wiimote.Button.two;

            if (wiimote.Button.d_up)
            {

            }

            //model.d_down.enabled = wiimote.Button.d_down;
            //model.d_left.enabled = wiimote.Button.d_left;
            //model.d_right.enabled = wiimote.Button.d_right;
            //model.plus.enabled = wiimote.Button.plus;
            //model.minus.enabled = wiimote.Button.minus;

            if(TestMode && wiimote.Button.home && !homeButtonIsDown)
            { 
                controller.Restart();
            }

            homeButtonIsDown = wiimote.Button.home;

            if (Input.GetAxis(SUBMIT_INPUT) > 0)
            {
                controller.SubjectTriesToSubmit();
            }

            if (Input.GetAxis(REQUIRE_PAUSE) > 0)
            {
                controller.ForceABreakInstantly();
            }
	    }
    }
}