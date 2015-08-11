using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public interface ITrial
{
    void Initialize(int mazeID, int pathID, SubjectControlMode mode);

    void StartTrial();

    event Action BeforeStart;

    event Action Finished;

    void CleanUp();
}

enum TrialType { Training, Pause, Experiment }

public class Trial : MonoBehaviour, ITrial
{
    public string MazeNamePattern = "Maze{0}";

    public VirtualRealityManager environment;

    public IMarkerStream marker;

    public HUDInstruction hud;

    protected beMobileMaze mazeInstance;

    protected PathInMaze path;

    public StartPoint startPoint;

    public GameObject Socket;

    public PathController pathController;

    public int currentPathID = -1;
    public int mazeID = -1;

    public int RunCount = 0;

    public void Initialize(int mazeId, int pathID, SubjectControlMode mode)
    {
        var targetWorldName = string.Format(MazeNamePattern, mazeId);
        
        var activeEnvironment = environment.ChangeWorld(targetWorldName);

        mazeInstance = activeEnvironment.GetComponent<beMobileMaze>();

        mazeInstance.MazeUnitEventOccured += OnMazeUnitEvent;

        startPoint.EnterStartPoint += OnStartPointEntered;
        startPoint.LeaveStartPoint += OnStartPointLeaved;

        pathController = activeEnvironment.GetComponent<PathController>();

        currentPathID = pathID;

        mazeID = mazeId;
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

        marker.Write(string.Format(MarkerPattern.BeginTrial, mazeID, path.ID, 0));

        var currentObject = GameObject.Instantiate(path.HideOut.TargetObject);

        currentObject.transform.parent = null;
        currentObject.transform.position = Socket.transform.position;
    }

    void Update()
    {
//        if (Input.GetMouseButton(0))
//            hud.SkipCurrent();
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
