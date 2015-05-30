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


public class Trial : MonoBehaviour, ITrial
{
    public string MazeNamePattern = "Maze{0}";

    public VirtualRealityManager environment;

    public IMarkerStream marker;

    public HUDInstruction hud;

    protected beMobileMaze mazeInstance;

    protected PathInMaze path;

    protected PathController pathController;

    private int currentPathID = -1;

    public void Initialize(int mazeID, int pathID, SubjectControlMode mode)
    {
        var targetWorld = string.Format(MazeNamePattern, mazeID);
        
        var activeEnvironment = environment.ChangeWorld(targetWorld);

        mazeInstance = activeEnvironment.GetComponent<beMobileMaze>();

        pathController = activeEnvironment.GetComponent<PathController>();

        currentPathID = pathID;
    }

    public virtual void OnMazeUnitEvent(MazeUnitEvent evt)
    {
        throw new NotImplementedException("Override the OnMazeUnitEvent Method!");
    }
    
    public virtual void StartTrial()
    {
        OnBeforeStart();

        path = pathController.EnablePathContaining(currentPathID);
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
    }
}
