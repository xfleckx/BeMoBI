using UnityEngine;
using System.Collections;

namespace Assets.BeMoBI.Scripts
{
    public class RotationObserver : MonoBehaviour
    {
        public RotationEvent OnHeadRotation;
        public RotationEvent OnBodyRotation;

        public Transform observable;

        void Start()
        {
            if (observable == null)
                this.enabled = false;
        }
        
        void LateUpdate()
        {

        }
    }

}
