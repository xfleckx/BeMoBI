using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
namespace Assets.BeMoBI.Scripts
{
    public class RotationObserver : MonoBehaviour
    {
        private enum RotationState { None, Onset, Peak, Offset }

        public RotationEvent OnHeadRotation;
        public RotationEvent OnBodyRotation;
        
        public Transform observable;

        public float EventThreshold = 0.01f;

        public float AccelerationThreshold = 1;

        private Queue<double> BufferOfAlphas;

        private float lastAcceleration  = 0;
        private float currentAcceleration = 0;
        private float lastVelocity = 0;
        private double currentVelocity = 0;

        private RotationState currentState = RotationState.None;

        /// <summary>
        /// WindowSize aka Buffersize
        /// </summary>
        private int bufferSize = 0;

        void Start()
        {
            if (observable == null)
                this.enabled = false;

            bufferSize = (int) (2 / Time.fixedDeltaTime);

            BufferOfAlphas = new Queue<double>(bufferSize);

            lastRotationState = observable.rotation;
        }

        Quaternion lastRotationState = Quaternion.identity;

        double lastRotationSpeed = 0;
        double lastRotationVelocity = 0;

        void FixedUpdate()
        {
            
            var currentAcceleration = lastVelocity - currentVelocity;

            if(currentAcceleration > AccelerationThreshold && currentState == RotationState.None)
            {
                currentState = RotationState.Onset;
            }

            if(currentAcceleration < -AccelerationThreshold && currentState == RotationState.Onset)
            {
                currentState = RotationState.Offset;
            }

            if (currentAcceleration < AccelerationThreshold &&
                currentAcceleration > -AccelerationThreshold &&
                currentState == RotationState.Offset)
            {
                currentState = RotationState.None;
            }
        }

        private double GetCurrentRotationDelta()
        {
            var currentRotationState = observable.rotation;

            Quaternion relative = Quaternion.Inverse(lastRotationState) * currentRotationState;

            double deltaAngle = 2 * Math.Atan2(relative.eulerAngles.magnitude, relative.w);
            
            var rotationSpeed = deltaAngle * Time.fixedDeltaTime;

            currentVelocity = (lastRotationSpeed - rotationSpeed) * Time.fixedDeltaTime;
            
            lastRotationSpeed = rotationSpeed;
            lastRotationState = currentRotationState;

            return deltaAngle;
        }

        void LateUpdate()
        {

            //if(Math.Abs(velocity) > EventThreshold)
            //{
            //    if(velocity > 0)
            //    {
            //        var args = new RotationEventArgs();
            //        args.state = RotationEventArgs.State.Begin;

            //        if (OnBodyRotation.GetPersistentEventCount() > 0)
            //            OnBodyRotation.Invoke(args);
            //    }

            //    if (velocity < 0)
            //    {
            //        var args = new RotationEventArgs();
            //        args.state = RotationEventArgs.State.End;

            //        if (OnBodyRotation.GetPersistentEventCount() > 0)
            //            OnBodyRotation.Invoke(args);
            //    }
            //}

        }


        // Alternative approach
        //void LateUpdate()
        //{
        //    if (RotationPassesThreshold())
        //    {
        //        if (!isBodyRotating)
        //        {
        //            var args = new RotationEventArgs();
        //            args.state = RotationEventArgs.State.Begin;

        //            if (OnBodyRotation.GetPersistentEventCount() > 0)
        //                OnBodyRotation.Invoke(args);

        //            isBodyRotating = true;
        //        }

        //        if (isBodyRotating)
        //        {
        //            var args = new RotationEventArgs();
        //            args.state = RotationEventArgs.State.End;

        //            if (OnBodyRotation.GetPersistentEventCount() > 0)
        //                OnBodyRotation.Invoke(args);

        //            isBodyRotating = false;
        //        }
        //    }

        //    if (recentRotation.Count > BufferSize) { 
        //        recentRotation.Dequeue(); // throw away the oldest rotation state
        //        var oldestOfCurrent = currentRotation.Dequeue(); 
        //        currentRotation.Enqueue(observable.rotation);
        //    }
        //    else if(currentRotation.Count > BufferSize)
        //    {
        //        var oldestOfCurrent = currentRotation.Dequeue();
        //        recentRotation.Enqueue(oldestOfCurrent);
        //        currentRotation.Enqueue(observable.rotation);
        //    }
        //    else
        //    {
        //        currentRotation.Enqueue(observable.rotation);
        //    }
        //}
        //Queue<Quaternion> recentRotation = new Queue<Quaternion>();
        //Queue<Quaternion> currentRotation = new Queue<Quaternion>();


        //private bool RotationPassesThreshold()
        //{
        //    var averageDistanceOnRecentRotation = AverageDistance(recentRotation.ToList());
        //    var averageDistanceOnCurrentRotation = AverageDistance(currentRotation.ToList());

        //    var delta = averageDistanceOnRecentRotation - 
        //                averageDistanceOnCurrentRotation;

        //    return Math.Abs(delta) >= EventThreshold;
        //}
        //private float AverageDistance(List<Quaternion> rotations)
        //{
        //    var deltaAngles = new List<float>();
        //    var lastOne = rotations[0];
        //    for (int i = 1; i < rotations.Count - 1; i++)
        //    {
        //        var nextOne = rotations[i];
        //        Quaternion relative = Quaternion.Inverse(lastOne) * nextOne;
        //        float deltaAngle = 2 * Math.Atan2(relative.eulerAngles.magnitude, relative.w);

        //    }
        //}



        //if (BufferOfAlphas.Count == bufferSize)
        //{
        //    BufferOfAlphas.Dequeue();
        //}
        //else
        //{
        //    BufferOfAlphas.Enqueue(GetCurrentRotationDelta());
        //}

        //var bufferAvg = BufferOfAlphas.Average();
    }

}
