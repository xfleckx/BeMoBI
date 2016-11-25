using UnityEngine;
using System.Collections;

namespace Assets.BeMoBI.Paradigms.SearchAndFind
{ 
	public class DoorControl : MonoBehaviour {

		Animator animator;

		// Use this for initialization
		void Start () {
			animator = GetComponent<Animator>();
		}
	  
		public void Open()
		{
			animator.ResetTrigger("Close");

			animator.SetTrigger("Open");
		}

		public void Close()
		{
			animator.SetTrigger("Close");
		}
	}
}
