﻿using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Paradigms.SearchAndFind.ImageEffects;

namespace Assets.Paradigms.SearchAndFind
{
    public enum Internal_Trial_State { Searching, Returning }

    public interface ITrial
    {
        void Initialize(string mazeName, int pathID, string category, string objectName);

        void SetReady();

        event Action BeforeStart;

        event Action<Trial, TrialResult> Finished;

        void CleanUp();
    }
    
    public class Trial : MonoBehaviour, ITrial
    {
        #region Dependencies 

        public ParadigmController paradigm;

        protected beMobileMaze mazeInstance;

        protected PathInMaze path;

        public HidingSpot hidingSpotInstance;

        public PathController pathController;
        public GameObject objectToRemember;
        
        public int currentPathID = -1;

        public string currentMazeName = string.Empty;

        public CustomGlobalFog fog;
        
        #endregion

        #region Trial state only interesting for the trial itself!

        protected string objectName;

        protected string categoryName;

        protected GameObject activeEnvironment;

        protected LinkedListNode<PathElement> currentPathElement;
             
        protected Internal_Trial_State currentTrialState;

        protected Stopwatch stopWatch;

        protected Stopwatch unitStopWatch;

        bool lastTurnWasIncorrect = false;
        private bool isReady;

        #endregion

        #region Setup methods

        public virtual void Initialize(string mazeName, int pathID, string category, string objectName)
        {
            UnityEngine.Debug.Log(string.Format("Initialize Trial: {0} {1} {2} {3}", mazeName, pathID, category, objectName));
            
            var expectedWorld = paradigm.VRManager.ChangeWorld(mazeName);

            if (expectedWorld != null)
            {
                activeEnvironment = expectedWorld.gameObject;
            }
            else
            {
                UnityEngine.Debug.Log(string.Format("Expected VR Environment \"{0}\" not found! Ending Trial!", mazeName));
                OnFinished(TimeSpan.Zero);
            }

            stopWatch = new Stopwatch();

            unitStopWatch = new Stopwatch();

            mazeInstance = activeEnvironment.GetComponent<beMobileMaze>();

            currentMazeName = mazeName;

            paradigm.relativePositionStream.currentMaze = mazeInstance;

            mazeInstance.MazeUnitEventOccured += OnMazeUnitEvent;

            paradigm.startingPoint.EnterStartPoint += OnStartPointEntered;
            paradigm.startingPoint.LeaveStartPoint += OnStartPointLeaved;

            currentTrialState = Internal_Trial_State.Searching;

            ResetStartConditions();

            ActivatePathAndSetHidingSpot(pathID);

            SwitchAllLightPanelsOff(mazeInstance);
            
            GatherObjectFromObjectPool(category, objectName);
            
        }

        private void ResetStartConditions()
        {
           paradigm.objectPresenter.SetActive(true);
            
        }

        private void ActivatePathAndSetHidingSpot(int pathId)
        {
            pathController = activeEnvironment.GetComponent<PathController>();

            currentPathID = pathId;

            path = pathController.EnablePathContaining(pathId);
          
            var unitAtPathEnd = path.PathAsLinkedList.Last.Value.Unit;

            // hiding spot look at inactive (open wall)
            var targetRotation = GetRotationFrom(unitAtPathEnd);
            var hidingSpotHost = Instantiate(paradigm.HidingSpotPrefab);

            hidingSpotHost.transform.SetParent(unitAtPathEnd.transform, false);
            hidingSpotHost.transform.localPosition = Vector3.zero;
            hidingSpotHost.transform.Rotate(targetRotation);
            
            hidingSpotInstance = hidingSpotHost.GetComponent<HidingSpot>();
        }

        private void GatherObjectFromObjectPool(string categoryName, string objectName)
        {
            var objectCategory = paradigm.objectPool.Categories.Where(c => c.name.Equals(categoryName)).FirstOrDefault();

            if(objectCategory == null)
                throw new ArgumentException(string.Format("Expected category \"{0}\" not found!", categoryName));

            var targetObject = objectCategory.GetObjectBy(objectName);

            if (targetObject == null)
                throw new ArgumentException(string.Format("Expected Object \"{0}\" from category \"{1}\" not found!", objectName, categoryName));

            objectToRemember = Instantiate(targetObject);
            
            objectToRemember.transform.SetParent(paradigm.objectPositionAtTrialStart, false);
            objectToRemember.transform.localPosition = Vector3.zero;
            objectToRemember.transform.rotation = Quaternion.identity;
            objectToRemember.transform.localScale = Vector3.one;
            //objectToRemember.transform.LookAt(paradigm.subject.transform);

            this.objectName = objectName;

            this.categoryName = categoryName;

        }

        IEnumerator DisplayObjectAtStartFor(float waitingTime)
        {
            yield return new WaitForSeconds(waitingTime);

            HideSocketAndOpenEntranceAtStart();

        }

        private void HideSocketAndOpenEntranceAtStart()
        {
            paradigm.entrance.SetActive(false);
            paradigm.objectPresenter.SetActive(false);
            var socketAtThePathEnd = hidingSpotInstance.GetSocket();

            var objectSocket = socketAtThePathEnd.GetComponent<ObjectSocket>();

            objectSocket.PutIn(objectToRemember);
        }

        protected void SwitchAllLightPanelsOff(beMobileMaze maze)
        {
            var allLights = maze.GetComponentsInChildren<TopLighting>();

            foreach (var light in allLights)
            {
                light.gameObject.transform.AllChildren().ForEach(l => {
                    if (l.gameObject.name != "Light")
                        l.gameObject.SetActive(false);
                    }
                );
            }

        }

        protected virtual void SetLightningOn(PathInMaze path, beMobileMaze maze)
        {
            // UnityEngine.Debug.Log(string.Format("Try enabling lights on Path: {0} in Maze: {1}",path.ID, maze.name));
       
            var currentElement = path.PathAsLinkedList.First;
            
            int globalRotation = 0;

            do
            { 

                // TODO: special case last element!

                var previousElement = currentElement.Previous;

                var nextElement = currentElement.Next;

                if (previousElement == null)
                    previousElement = currentElement;
                
                if (nextElement == null) { 
                    nextElement = previousElement;
                }

                var previousElementsPosition = previousElement.Value.Unit.transform.position;
                var currentElementsPosition = currentElement.Value.Unit.transform.position;
                var nextPathElementsPosition = nextElement.Value.Unit.transform.position;
                
                var a = previousElementsPosition - currentElementsPosition;

                Vector3 b = Vector3.zero;

                if (currentElement.Next != null)
                    b = currentElementsPosition - nextPathElementsPosition;
                else
                    b = nextPathElementsPosition - currentElementsPosition;

                var turningAngle = a.SignedAngle(b, Vector3.up);

                globalRotation = (globalRotation + (int) turningAngle) % 360;

                //UnityEngine.Debug.Log(string.Format("From {2} to {3} ## Current Angle: {0} ## GlobalRotation {1}", turningAngle, globalRotation, currentElementsPosition, nextPathElementsPosition));

                var topLight = currentElement.Value.Unit.GetComponentInChildren<TopLighting>();

                ChangeLightningOn(topLight, currentElement.Value, globalRotation);

               // topLight.SwitchOn();
                
                currentElement = currentElement.Next;

            } while (currentElement != null);
        }

        private void ChangeLightningOn(TopLighting light, PathElement current, int globalRotation)
        {
            var lightChildren = light.gameObject.transform.AllChildren();

            var toDirectionPanelName = OrientationDefinition.Current.GetDirectionNameFromEuler(globalRotation);

            int rotationOffset = 0;
            if(current.Type == UnitType.L || current.Type == UnitType.T || current.Type == UnitType.X)
            {
                if (current.Turn == TurnType.LEFT)
                    rotationOffset = -90;
                
                if (current.Turn == TurnType.RIGHT)
                    rotationOffset = 90;

                if (current.Turn == TurnType.STRAIGHT)
                    rotationOffset = 180;
            }

            if (current.Type == UnitType.I)
            {
               rotationOffset = 180;
            }

            var fromDirectionPanelName = OrientationDefinition.Current.GetDirectionNameFromEuler(globalRotation + rotationOffset);

            // UnityEngine.Debug.Log(string.Format("From Direction: {0} ## To direction: {1}", fromDirectionPanelName, toDirectionPanelName));

            // Enable only for open walls and the direction
            
            foreach (var lightPanel in lightChildren)
            {
                if (lightPanel.name.Equals("Center"))
                {
                    lightPanel.SetActive(true);
                }

                if (lightPanel.name.Equals(toDirectionPanelName) || lightPanel.name.Equals(fromDirectionPanelName))
                { 
                    lightPanel.SetActive( true );
                }
            }

        }

        #endregion

        public virtual void SetReady()
        {
            this.isReady = true; // Trial starts when Subject enters Startpoint

            paradigm.hud.Clear();

            paradigm.hud.ShowInstruction("Please enter the pink start point to start the trial!", "Task");
        }

        private void OnStartPointEntered(Collider c)
        {
            var subject = c.GetComponent<VRSubjectController>();

            if (subject == null)
                return;

            EntersStartPoint(subject);
        }

        private void OnStartPointLeaved(Collider c)
        {
            var subject = c.GetComponent<VRSubjectController>();

            if (subject == null)
                return;

            LeavesStartPoint(subject);
        }

        public virtual void EntersStartPoint(VRSubjectController subject)
        {
            if (isReady && currentTrialState == Internal_Trial_State.Searching)
            {
                OnBeforeStart();

                paradigm.marker.Write(MarkerPattern.FormatBeginTrial(this.GetType().Name, currentMazeName, path.ID, objectName, categoryName));
                
                stopWatch.Start();

                ShowObjectAtStart();

            }
        }

        protected virtual void ShowObjectAtStart()
        {
            objectToRemember.SetActive(true);

            paradigm.marker.Write(MarkerPattern.FormatDisplayObject(objectName, categoryName));

            StartCoroutine(
                DisplayObjectAtStartFor(
                    paradigm.config.TimeToDisplayObjectToRememberInSeconds)
                    );
        }

        #region Write Marker at the end of a frame

        private string pendingMarker;

        IEnumerator WriteAfterPostPresent()
        {
            yield return new WaitForEndOfFrame();

            paradigm.marker.Write(pendingMarker);

            yield return null;
        }

        public void WriteOnLastPossibleMoment(string marker)
        {
            pendingMarker = marker;
            StartCoroutine(WriteAfterPostPresent());
        }

        #endregion

        public virtual void LeavesStartPoint(VRSubjectController subject)
        {
            if (currentTrialState == Internal_Trial_State.Searching)
            {
                paradigm.startingPoint.gameObject.SetActive(false);
                // write a marker when the subject starts walking!?
                paradigm.hud.Clear();
            }
        }

        public virtual void EntersWaypoint(ActionWaypoint waypoint)
        {
            if (!this.isActiveAndEnabled || waypoint.WaypointId != 0)
                return;

            if (currentTrialState == Internal_Trial_State.Returning)
            {
                paradigm.marker.Write(MarkerPattern.FormatEndTrial(this.GetType().Name, currentMazeName, path.ID, objectName, categoryName));

                paradigm.startingPoint.gameObject.SetActive(true);

                stopWatch.Stop();

                waypoint.HideInfoText();

                paradigm.hud.ShowInstruction("Turn and go back to Start point for the next trial!","Task:");

                OnFinished(stopWatch.Elapsed);
            }
        }

        public virtual void LeavesWaypoint(ActionWaypoint waypoint)
        {
            if (!this.isActiveAndEnabled || waypoint.WaypointId != 0)
                return;


        }

        public virtual void OnMazeUnitEvent(MazeUnitEvent evt)
        {
            var unit = evt.MazeUnit;

            if(evt.MazeUnitEventType == MazeUnitEventType.Entering) { 

                if(currentPathElement == null)
                {
                    if (path.PathAsLinkedList.First.Value.Unit.Equals(unit))
                    {
                        currentPathElement = path.PathAsLinkedList.First;

                        // special case entering the maze
                        paradigm.marker.Write(MarkerPattern.FormatCorrectTurn(currentPathElement.Value, currentPathElement.Value));

                        paradigm.hud.Clear();
                    }
                    else
                    {
                        UnityEngine.Debug.Log("Seems as something entered the maze on the wrong entrance!");
                    }
                }
                else
                {
                    // end of the path is reached
                    if (path.PathAsLinkedList.Last.Value.Unit.Equals(unit))
                    {
                        if( currentTrialState == Internal_Trial_State.Searching) {

                            paradigm.marker.Write(MarkerPattern.FormatCorrectTurn(currentPathElement.Value, currentPathElement.Value));

                            hidingSpotInstance.Reveal();

                            paradigm.marker.Write(MarkerPattern.FormatFoundObject(currentMazeName, path.ID, objectName, categoryName));

                            paradigm.TrialEndPoint.gameObject.SetActive(true);

                            currentTrialState = Internal_Trial_State.Returning;


                            if (paradigm.config.useTeleportation) {

                                paradigm.hud.ShowInstruction("You made it, you will be teleported back to the start point", "Yeah!");
                                
                                StartCoroutine(BeginTeleportation());

                            }
                            else
                            {

                                paradigm.hud.ShowInstruction("You made it, please return to the start point!", "Yeah!");

                                path.InvertPath();

                                currentPathElement = path.PathAsLinkedList.First;
                            }

                        }
                    } // A correct turn is defined as entering the last correct or the next element of the path
                    else if (currentPathElement.Value.Unit.Equals(unit) || currentPathElement.Next.Value.Unit.Equals(unit))
                    {
                        // avoid write correct marker duplication
                        if (!lastTurnWasIncorrect) {
                            // Don't get confused here! From the current state of the trial we are actual one element behind!
                            paradigm.marker.Write(MarkerPattern.FormatCorrectTurn(currentPathElement.Value, currentPathElement.Next.Value));
                        }

                        lastTurnWasIncorrect = false;

                        if (paradigm.hud.IsRendering)
                            paradigm.hud.Clear();
                        
                        // now change the current state of the trial for the next unit event!
                        currentPathElement = currentPathElement.Next;
                    }
                    else
                    {
                        lastTurnWasIncorrect = true;

                        paradigm.marker.Write(MarkerPattern.FormatIncorrectTurn(unit, currentPathElement.Value, currentPathElement.Next.Value));

                        paradigm.hud.ShowInstruction("You`re wrong! Please turn!", "Task");
                    }
                }
            }

        }
        
        IEnumerator BeginTeleportation()
        {
            yield return new WaitForSeconds(paradigm.config.offsetToTeleportation);

            paradigm.teleporter.Teleport();

            yield return null;
        }

        /// <summary>
        /// Warning using this could cause inconsistent behaviour within the paradigm!
        /// In most cases, the trial should end itself!
        /// </summary>
        public virtual void ForceTrialEnd()
        {
            stopWatch.Stop();

            OnFinished(stopWatch.Elapsed);
        }
        
        public event Action BeforeStart;
        protected void OnBeforeStart()
        {
            if (BeforeStart != null)
                BeforeStart();
        }
         
        public event Action<Trial, TrialResult> Finished;
        protected void OnFinished(TimeSpan trialDuration)
        {
            UnityEngine.Debug.Log("Trial report finished");

            if (Finished != null)
                Finished(this, new TrialResult(trialDuration));
        }

        protected void ClearCallbacks()
        {

            Finished = null;
            BeforeStart = null;

        }

        public void CleanUp()
        {
            currentPathElement = null;

            if(path.Inverse)
                path.InvertPath();

            if(mazeInstance != null) { 
                var lineRenderer = mazeInstance.GetComponent<LineRenderer>();
            
                Destroy(lineRenderer);

                mazeInstance.MazeUnitEventOccured -= OnMazeUnitEvent;
            }

            ClearCallbacks();

            paradigm.startingPoint.ClearSubscriptions();

            if(hidingSpotInstance != null)
            {
                Destroy(hidingSpotInstance.gameObject);
            }
        }
         
        #region Helper functions

        private Vector3 GetRotationFrom(MazeUnit unit)
        {
            var childs = unit.transform.AllChildren();
            // try LookAt functions
            foreach (var wall in childs)
            {
                if (wall.name.Equals("South") && !wall.activeSelf)
                {
                    return Vector3.zero;
                }

                if (wall.name.Equals("North") && !wall.activeSelf)
                {
                    return new Vector3(0, 180, 0);
                }

                if (wall.name.Equals("West") && !wall.activeSelf)
                {
                    return new Vector3(0, 90, 0);
                }

                if (wall.name.Equals("East") && !wall.activeSelf)
                {
                    return new Vector3(0, 270, 0);
                }

            }

            return Vector3.one;
        }
        
        #endregion
    }

    public class TrialResult
    {
        private TimeSpan duration;
        public TimeSpan Duration { get { return duration; } }

        public TrialResult(TimeSpan duration)
        {
            this.duration = duration;
        }
    }
}
