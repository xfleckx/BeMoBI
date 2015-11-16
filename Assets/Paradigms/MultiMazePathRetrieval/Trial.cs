using UnityEngine;
using System.Collections;
using System.Linq;
using System;

namespace Assets.Paradigms.MultiMazePathRetrieval
{
    public enum Internal_Trial_State { Searching, Returning }

    public interface ITrial
    {
        void Initialize(string mazeName, int pathID, string category, string objectName);

        void StartTrial();

        event Action BeforeStart;

        event Action Finished;

        void CleanUp();
    }
    
    public class Trial : MonoBehaviour, ITrial
    {
        public VirtualRealityManager environment;

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

        public int SecondsToDisplay = 10;

        public int currentPathID = -1;
        public int mazeID = -1;

        protected GameObject activeEnvironment;

        protected Vector2 GridPositionOFLastPathElement;

        protected Internal_Trial_State currentTrialState;

        public void Initialize(string mazeName, int pathID, string category, string objectName)
        {  
            activeEnvironment = environment.ChangeWorld(mazeName).gameObject;

            mazeInstance = activeEnvironment.GetComponent<beMobileMaze>();

            mazeInstance.MazeUnitEventOccured += OnMazeUnitEvent;

            startPoint.EnterStartPoint += OnStartPointEntered;
            startPoint.LeaveStartPoint += OnStartPointLeaved;

            currentTrialState = Internal_Trial_State.Searching;

            ResetStartConditions();

            ActivatePath(pathID);

            GatherObjectFromObjectPool(category, objectName);

            StartCoroutine(DisplayObjectAtStartFor(SecondsToDisplay));
        }

        private void ResetStartConditions()
        {
            ObjectDisplaySocket.SetActive(true);
            
        }

        private void ActivatePath(int pathId)
        {
            pathController = activeEnvironment.GetComponent<PathController>();

            path = pathController.EnablePathContaining(pathId);

            var lastGridItem = path.PathElements.Last();

            var unit = lastGridItem.Value.Unit;
             
            GridPositionOFLastPathElement = lastGridItem.Key;

            var targetRotation = GetRotationFrom(unit);
            var hidingSpotHost = Instantiate<GameObject>(hidingSpotPrefab);

            hidingSpotHost.transform.SetParent(unit.transform, false);
            hidingSpotHost.transform.localPosition = Vector3.zero;
            hidingSpotHost.transform.Rotate(targetRotation);
            
            hidingSpotInstance = hidingSpotHost.GetComponent<HidingSpot>();
        }

        private void GatherObjectFromObjectPool(string categoryName, string objectName)
        {
            var objectCategory = objectPool.Categories.Where(c => c.name.Equals(categoryName)).FirstOrDefault();
            var targetObject = objectCategory.GetObjectBy(objectName);
            objectToRemember = Instantiate<GameObject>(targetObject);
            objectToRemember.SetActive(true);
            objectToRemember.transform.SetParent(positionAtTrialBegin, false);
            objectToRemember.transform.localPosition = Vector3.zero;
              
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
            objectToRemember.transform.SetParent(socketAtThePathEnd);
            objectToRemember.transform.localPosition = Vector3.zero;
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
            if (currentTrialState == Internal_Trial_State.Searching)
            {
                return;
            }

            if(currentTrialState == Internal_Trial_State.Returning)
            {
                OnFinished();
            }
        }

        public virtual void LeavesStartPoint(VRSubjectController subject)
        {
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
             

            marker.Write(string.Format(MarkerPattern.BeginTrial, GetType().Name, mazeID, path.ID, 0));
            
        }

        // look at inactive (open wall)
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

        public event Action BeforeStart;
        protected void OnBeforeStart()
        {
            if (BeforeStart != null)
                BeforeStart();

        }

        public event Action Finished;
        protected void OnFinished()
        {
            if (Finished != null)
                Finished();
        }

        public void CleanUp()
        {
            mazeInstance.MazeUnitEventOccured -= OnMazeUnitEvent;
            startPoint.LeaveStartPoint -= OnStartPointLeaved;
            startPoint.EnterStartPoint -= OnStartPointEntered;

            if(hidingSpotInstance != null)
            {
                Destroy(hidingSpotInstance.gameObject);
            }
        }
    }
}
