using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public interface ITrial
{
    void Initialize();

    void Start();

    event Action OnStart;

    event Action OnEnd;

    void CleanUp();
}

public class Training : ITrial {

    /// <summary>
    /// Könnten auch Eigenschaften einer Basisklasse sein
    /// </summary>
    VirtualRealityManager environment;

    IMarkerStream marker;

    HUDInstruction hud;

    beMobileMaze mazeControl;

    PathInMaze path;

    public Training(VirtualRealityManager vrmanager, IMarkerStream streamInterface, HUDInstruction instructions)
    {
        environment = vrmanager;
        marker = streamInterface;
        hud = instructions;
    }

    public void Initialize()
    {
        // Idea
        // var targetWorld = string.Format("maze{0}", Random.Range(0,1));
        // var targetPath = string.Format("path{0}", Random.Range(0,1));

        var targetWorld = "maze";
        var targetPath = "pathA";

        //environment.ChangeWorld(targetWorld);

        //mazeControl = environment.ActiveEnvironments.First().GetComponent<beMobileMaze>(); 

        //path = mazeControl.RequirePath(targetPath);

        //mazeControl.MazeUnitEventOccured += mazeControl_MazeUnitEventOccured; 
    }

    /// <summary>
    /// A Trial Start may caused from external source (e.g. a key press)
    /// </summary>
    public void Start()
    {
        var instruction = new Instruction();
        
        instruction.DisplayTime = 30f;
        instruction.Text = "Remember the given path for this labyrinth";

        if(hud.enabled)
            hud.StartDisplaying(instruction);

        if (OnStart != null)
            OnStart();
    }

    public event Action OnStart;

    void mazeControl_MazeUnitEventOccured(MazeUnitEvent obj)
    {
        if (obj.MazeUnitEventType == MazeUnitEventType.Entering)
        {
            Debug.Log("Training Trial Instance recieved MazeUnitEvent: Entering");
        }

        if (obj.MazeUnitEventType == MazeUnitEventType.Exiting)
        {
            Debug.Log("Training Trial Instance recieved MazeUnitEvent: Exiting");
        }
    }

    public event Action OnEnd;

    public void CleanUp()
    {
        mazeControl.MazeUnitEventOccured -= mazeControl_MazeUnitEventOccured;
    }

}
