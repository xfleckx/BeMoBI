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

        public IMarkerStream marker;

        public HUD_Instruction hud;
        public HUD_DEBUG debug;

        protected beMobileMaze mazeInstance;

        protected PathInMaze path;

        public StartPoint startPoint;

        public GameObject Socket;

        public PathController pathController;

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

            path = pathController.Paths.Single((p) => p.ID == pathID);
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

            path = pathController.EnablePathContaining(currentPathID);

            marker.Write(string.Format(MarkerPattern.BeginTrial, GetType().Name, mazeID, path.ID, 0));
             
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
        }
    }
}
