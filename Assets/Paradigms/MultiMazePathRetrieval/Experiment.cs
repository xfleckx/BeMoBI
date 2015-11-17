using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace Assets.Paradigms.MultiMazePathRetrieval
{

    public class Experiment : Trial
    {
        /// <summary>
        /// A Trial Start may caused from external source (e.g. a key press)
        /// </summary>
        public override void StartTrial()
        {
            base.StartTrial();
             
            hud.ShowInstruction("Retrieve the path to this object");
            
        }

        public override void OnMazeUnitEvent(MazeUnitEvent obj)
        {
            if (obj.MazeUnitEventType == MazeUnitEventType.Entering)
            {
                var current = obj.MazeUnit.GridID;

                marker.Write(string.Format(MarkerPattern.Unit, mazeID, current.x, current.y));

                if (!path.PathElements.ContainsKey(current))
                {
                    hud.ShowInstruction("You`re wrong! Please turn!");
                }
                else
                {
                    if(hud.IsRendering)
                        hud.Clear();

                    var currentPathElement = path.PathElements[current];

                    WriteMarkerFor(currentPathElement);
                }

                if (PathEnd.Equals(current))
                {
                    currentTrialState = Internal_Trial_State.Returning;
                }
            }
        }
        

        public override void LeavesStartPoint(VRSubjectController subject)
        {
            Debug.Log("Subject leaves Startpoint");
        }

        private void WriteMarkerFor(PathElement pathElement)
        {
            var type = Enum.GetName(typeof(UnitType), pathElement.Type);
            marker.Write(string.Format(MarkerPattern.Enter, type, pathElement.Unit.GridID.x, pathElement.Unit.GridID.x));
        }
    }
}
