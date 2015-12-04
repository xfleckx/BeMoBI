using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace Assets.Paradigms.SearchAndFind
{
    public class Experiment : Trial
    {
        /// <summary>
        /// A Trial Start may caused from external source (e.g. a key press)
        /// </summary>
        public override void StartTrial()
        {
            SwitchAllLightsOff(mazeInstance);

            base.StartTrial();
             
            hud.ShowInstruction("Retrieve the path to this object");
            
        }
        

        public override void OnMazeUnitEvent(MazeUnitEvent obj)
        {
            if (obj.MazeUnitEventType == MazeUnitEventType.Entering)
            {
                var current = obj.MazeUnit.GridID;

                marker.Write(string.Format(MarkerPattern.Unit, currentMazeName, current.x, current.y));

                if (!path.PathAsLinkedList.Any(e => e.Unit.Equals(obj.MazeUnit)))
                {
                    marker.Write(MarkerPattern.Incorrect);

                    hud.ShowInstruction("You`re wrong! Please turn!");
                }
                else
                {
                    if(hud.IsRendering)
                        hud.Clear();

                    // TODO

                    //var currentPathElement = path.PathElements[current];

                    //WriteMarkerFor(currentPathElement);
                }

                if (PathEnd.Equals(current))
                {
                    hidingSpotInstance.Reveal();

                    hud.ShowInstruction("You made it, please return to the start point!", "Yeah!");

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
