using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Assets.Paradigms.SearchAndFind
{
    public enum Internal_Trial_State { Searching, Returning }

    public interface ITrial
    {
        void Initialize(string mazeName, int pathID, string category, string objectName);

        void StartTrial();

        event Action BeforeStart;

        event Action<Trial, TrialResult> Finished;

        void CleanUp();
    }
    
    public class Trial : MonoBehaviour, ITrial
    {
        public VirtualRealityManager VRManager;

        public ObjectPool objectPool;

        public IMarkerStream marker;

        public HUD_Instruction hud;

        public HUD_DEBUG debug;

        protected beMobileMaze mazeInstance;

        protected PathInMaze path;

        public StartPoint startPoint;

        public Transform positionAtTrialBegin;

        public GameObject ObjectDisplaySocket;

        public GameObject MazeEntranceDoor;

        public GameObject hidingSpotPrefab;

        public HidingSpot hidingSpotInstance;

        public PathController pathController;

        public GameObject objectToRemember;

        public Material lightningMaterial;

        public int SecondsToDisplay = 10;

        public int currentPathID = -1;
        public string currentMazeName = string.Empty;

        protected GameObject activeEnvironment;

        protected Vector2 PathEnd;

        protected Internal_Trial_State currentTrialState;
        private LinkedList<PathElement> currentPathAsLinkedList;

        protected Stopwatch stopWatch;
        public FullScreenFade fading;

        public virtual void Initialize(string mazeName, int pathID, string category, string objectName)
        {
            UnityEngine.Debug.Log(string.Format("Initialize Trial: {0} {1} {2} {3}", mazeName, pathID, category, objectName));

            var expectedWorld = VRManager.ChangeWorld(mazeName);

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

            mazeInstance = activeEnvironment.GetComponent<beMobileMaze>();
            currentMazeName = mazeName;
            mazeInstance.MazeUnitEventOccured += OnMazeUnitEvent;

            startPoint.EnterStartPoint += OnStartPointEntered;
            startPoint.LeaveStartPoint += OnStartPointLeaved;

            currentTrialState = Internal_Trial_State.Searching;

            ResetStartConditions();

            ActivatePathAndSetHidingSpot(pathID);

            SwitchAllLightsOff(mazeInstance);
            
            GatherObjectFromObjectPool(category, objectName);
            
            StartCoroutine(DisplayObjectAtStartFor(SecondsToDisplay));
        }

        private void ResetStartConditions()
        {
            ObjectDisplaySocket.SetActive(true);
            
        }

        private void ActivatePathAndSetHidingSpot(int pathId)
        {
            pathController = activeEnvironment.GetComponent<PathController>();

            currentPathID = pathId;

            path = pathController.EnablePathContaining(pathId);

            this.currentPathAsLinkedList = path.PathAsLinkedList;

            var unitAtPathEnd = currentPathAsLinkedList.Last.Value.Unit;

            PathEnd = unitAtPathEnd.GridID;

            // hiding spot look at inactive (open wall)
            var targetRotation = GetRotationFrom(unitAtPathEnd);
            var hidingSpotHost = Instantiate(hidingSpotPrefab);

            hidingSpotHost.transform.SetParent(unitAtPathEnd.transform, false);
            hidingSpotHost.transform.localPosition = Vector3.zero;
            hidingSpotHost.transform.Rotate(targetRotation);
            
            hidingSpotInstance = hidingSpotHost.GetComponent<HidingSpot>();
        }

        private void GatherObjectFromObjectPool(string categoryName, string objectName)
        {
            var objectCategory = objectPool.Categories.Where(c => c.name.Equals(categoryName)).FirstOrDefault();
            var targetObject = objectCategory.GetObjectBy(objectName);
            objectToRemember = Instantiate(targetObject);
            objectToRemember.SetActive(true);
            objectToRemember.transform.SetParent(positionAtTrialBegin, false);
            objectToRemember.transform.localPosition = Vector3.zero;
            objectToRemember.transform.rotation = Quaternion.identity;
            objectToRemember.transform.localScale = Vector3.one;
        }

        IEnumerator DisplayObjectAtStartFor(float waitingTime)
        {
            yield return new WaitForSeconds(waitingTime);

            HideSocketAndOpenEntranceAtStart();

        }

        private void HideSocketAndOpenEntranceAtStart()
        {
            MazeEntranceDoor.SetActive(false);
            ObjectDisplaySocket.SetActive(false);
            var socketAtThePathEnd = hidingSpotInstance.GetSocket();

            var objectSocket = socketAtThePathEnd.GetComponent<ObjectSocket>();

            objectSocket.PutIn(objectToRemember);

            //objectToRemember.transform.SetParent(socketAtThePathEnd);
            //objectToRemember.transform.localPosition = Vector3.zero;
            
        }

        protected void SwitchAllLightsOff(beMobileMaze maze)
        {
            var allLights = maze.GetComponentsInChildren<TopLighting>();

            foreach (var light in allLights)
            {
                light.SwitchOff();
                light.gameObject.transform.AllChildren().ForEach(l => l.gameObject.SetActive(false));
            }

        }

        protected virtual void SetLightningOn(PathInMaze path, beMobileMaze maze)
        {
            UnityEngine.Debug.Log(string.Format("Try enabling lights on Path: {0} in Maze: {1}",path.ID, maze.name));
            
            var lineRenderer = maze.gameObject.AddComponent<LineRenderer>();

            lineRenderer.SetWidth(0.2f, 0.2f);

            lineRenderer.material = lightningMaterial;
            
            var elements = path.PathElements.Values.ToList();

            var countOfPositions = path.PathElements.Count;

            lineRenderer.SetVertexCount(countOfPositions);

            for (int i = 0; i < countOfPositions; i++)
            {
                var unitPosition = elements[i].Unit.transform.position + new Vector3(0,maze.RoomDimension.y,0);

                lineRenderer.SetPosition(i, unitPosition);
            }

            #region stashed
            //var currentElement = path.PathAsLinkedList.First;

            //do
            //{
            //    var currentPathElement = currentElement.Value;
            //    var nextPathElement = currentElement.Next.Value;

            //    var topLight = currentPathElement.Unit.GetComponentInChildren<TopLighting>();

            //    ChangeLightningOn(topLight, currentPathElement, nextPathElement);

            //    topLight.SwitchOn();

            //    if (currentElement.Next == null)
            //        continue;

            //    currentElement = currentElement.Next;

            //} while (currentElement.Next != null);
            #endregion
        }

        private void ChangeLightningOn(TopLighting light, PathElement current, PathElement next)
        {
            var unit = current.Unit;

            var currentUnitsChildren = unit.transform.AllChildren();
            var lightChildren = light.gameObject.transform.AllChildren();

            // Enable only for open walls

            foreach (var lightPanel in lightChildren)
            {
                if (lightPanel.name.Equals("Center"))
                {
                    lightPanel.SetActive(true);
                }

                if (lightPanel.name.Equals("North"))
                { 

                    var state = currentUnitsChildren.First(w => w.name.Equals("North")).activeSelf;

                    lightPanel.SetActive(!state);
                }

                if (lightPanel.name.Equals("South"))
                {
                    var state = currentUnitsChildren.First(w => w.name.Equals("South")).activeSelf;
                    lightPanel.SetActive(!state);
                }

                if (lightPanel.name.Equals("East"))
                {
                    var state = currentUnitsChildren.First(w => w.name.Equals("East")).activeSelf;
                    lightPanel.SetActive(!state);
                }

                if (lightPanel.name.Equals("West"))
                {
                    var state = currentUnitsChildren.First(w => w.name.Equals("West")).activeSelf;
                    lightPanel.SetActive(!state);
                }
            }

        }

        private Vector3 GetRotationFrom(MazeUnit unit)
        {
            var childs = unit.transform.AllChildren();
            // try LookAt functions
            foreach (var wall in childs)
            {
                if (wall.name.Equals("South") && !wall.activeSelf) {
                    return Vector3.zero;
                }

                if (wall.name.Equals("North") && !wall.activeSelf)
                {
                    return new Vector3(0,180,0);
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
            UnityEngine.Debug.Log("Subject enters Startpoint");

            if (currentTrialState == Internal_Trial_State.Searching)
            {
                return;
            }

            if(currentTrialState == Internal_Trial_State.Returning)
            {
                stopWatch.Stop();
                OnFinished(stopWatch.Elapsed);
            }
        }

        public virtual void LeavesStartPoint(VRSubjectController subject)
        {
            UnityEngine.Debug.Log("Subject leaves Startpoint");

            if (currentTrialState == Internal_Trial_State.Searching)
            {
                // write a marker when the subject starts walking!?
            }
        }

        public virtual void OnMazeUnitEvent(MazeUnitEvent evt)
        {
            throw new NotImplementedException("Override the OnMazeUnitEvent Method!");
        }

        public virtual void StartTrial()
        {
            OnBeforeStart();
            marker.Write(string.Format(MarkerPattern.BeginTrial, GetType().Name, currentMazeName, path.ID, 0));
            stopWatch.Start();
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
            if(mazeInstance != null) { 
                var lineRenderer = mazeInstance.GetComponent<LineRenderer>();
            
                Destroy(lineRenderer);

                mazeInstance.MazeUnitEventOccured -= OnMazeUnitEvent;
            }

            ClearCallbacks();

            startPoint.ClearSubscriptions();

            if(hidingSpotInstance != null)
            {
                Destroy(hidingSpotInstance.gameObject);
            }
        }
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
