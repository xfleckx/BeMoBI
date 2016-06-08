using UnityEngine;
using System.Collections;

namespace Assets.BeMoBI.Paradigms.SearchAndFind { 

	/// <summary>
	/// Represents all possible interactions for subject regarding the paradigm or trial behaviour
	/// </summary>
	public class SubjectInteractions : MonoBehaviour {

		private const string SUBMIT_INPUT = "Subject_Submit";
		private const string REQUIRE_PAUSE = "Subject_Requires_Break";

		ParadigmController paradigm;

		void Awake()
		{
			paradigm = FindObjectOfType<ParadigmController>();
		}

		// Update is called once per frame
		void Update () {
			
			if (Input.GetAxis(SUBMIT_INPUT) > 0)
			{
				paradigm.SubjectTriesToSubmit();
			}

			if (Input.GetAxis(REQUIRE_PAUSE) > 0)
			{
				paradigm.ForceABreakInstantly();
			}
		}
	}
}