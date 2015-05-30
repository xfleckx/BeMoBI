using UnityEngine;
using System.Collections;
using System;
using System.Linq;


public class Training : Trial {
     
    /// <summary>
    /// A Trial Start may caused from external source (e.g. a key press)
    /// </summary>
    public override void StartTrial()
    {   
        var instruction = new Instruction();
        
        instruction.DisplayTime = 30f;
        instruction.Text = "Remember the given path for this labyrinth";

        if(hud.enabled)
            hud.StartDisplaying(instruction);
    }

    public override void OnMazeUnitEvent(MazeUnitEvent obj)
    {
        if (obj.MazeUnitEventType == MazeUnitEventType.Entering)
        {
            Debug.Log("Training Trial Instance recieved MazeUnitEvent: Entering");
        }
    }
}
