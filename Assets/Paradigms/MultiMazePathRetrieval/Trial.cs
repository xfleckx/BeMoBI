using UnityEngine;
using System.Collections;
using System.Linq;
using System;

namespace Assets.Paradigms.MultiMazePathRetrieval
{
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

        public GameObject MazeEntranceDoor;

        public GameObject hidingSpotPrefab;

        public HidingSpot hidingSpotInstance;

        public PathController pathController;

        public GameObject objectToShow;

        public int currentPathID = -1;
        public int mazeID = -1;
        
        public void Initialize(string mazeName, int pathID, string category, string objectName)
        {  
            var activeEnvironment = environment.ChangeWorld(mazeName);

            mazeInstance = activeEnvironment.GetComponent<beMobileMaze>();

            mazeInstance.MazeUnitEventOccured += OnMazeUnitEvent;

            startPoint.EnterStartPoint += OnStartPointEntered;
            startPoint.LeaveStartPoint += OnStartPointLeaved;

            pathController = activeEnvironment.GetComponent<PathController>();
            
            path = pathController.EnablePathContaining(currentPathID);

            var lastGridItem = path.PathElements.Last();
            var unit = lastGridItem.Value.Unit;

            var targetRotation = GetRotationFrom(unit);
            var hidingSpotHost = Instantiate<GameObject>(hidingSpotPrefab);
            hidingSpotHost.transform.SetParent(unit.transform,false);
            hidingSpotHost.transform.localPosition = Vector3.zero;
            hidingSpotHost.transform.Rotate(targetRotation);

            hidingSpotInstance = hidingSpotHost.GetComponent<HidingSpot>();

            var objectCategory = objectPool.Categories.Where(c => c.name.Equals(category)).FirstOrDefault();
            var targetObject = objectCategory.GetObjectBy(objectName);
            objectToShow = Instantiate<GameObject>(targetObject);

            objectToShow.transform.SetParent(positionAtTrialBegin, false);
            objectToShow.transform.localPosition = Vector3.zero;
            objectToShow.SetActive(true);

            MazeEntranceDoor.SetActive(false);
        }

        private void OnStartPointEntered(Collider c)
        {
            var subject = c.GetComponent<SubjectController>();

            if (subject == null)
                return;

            EntersStartPoint(subject);
        }

        private void OnStartPointLeaved(Collider c)
        {
            var subject = c.GetComponent<SubjectController>();

            if (subject == null)
                return;

            LeavesStartPoint(subject);
        }

        public virtual void EntersStartPoint(SubjectController subject)
        {
            throw new NotImplementedException("Override the EntersStartPoint Method!");
        }

        public virtual void LeavesStartPoint(SubjectController subject)
        {
            throw new NotImplementedException("Override the EntersStartPoint Method!");
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
            // try look at
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
