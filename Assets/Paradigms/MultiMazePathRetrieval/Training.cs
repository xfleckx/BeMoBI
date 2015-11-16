using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace Assets.Paradigms.MultiMazePathRetrieval
{
    public class Training : Trial
    {
        /// <summary>
        /// A Trial Start may caused from external source (e.g. a key press)
        /// </summary>
        public override void StartTrial()
        {
            base.StartTrial();

            var instruction = new Instruction();

            instruction.DisplayTime = 5f;
            instruction.Text = "Remember the given path for this object!";

            if (hud.enabled)
                hud.StartDisplaying(instruction);
        }

        public override void OnMazeUnitEvent(MazeUnitEvent obj)
        {
            if (obj.MazeUnitEventType == MazeUnitEventType.Entering)
            {
                var current = obj.MazeUnit.GridID;

                marker.Write(string.Format(MarkerPattern.Unit, mazeID, current.x, current.y));

                if (!path.PathElements.ContainsKey(current))
                {
                    var instruction = new Instruction();

                    instruction.Text = "You`re wrong! Please turn!";

                    hud.StartDisplaying(instruction);
                }
                else
                {
                    hud.StopAllCoroutines();

                    var currentPathElement = path.PathElements[current];
                    WriteMarkerFor(currentPathElement);

                }

                if (GridPositionOFLastPathElement.Equals(current))
                {
                    hidingSpotInstance.Reveal();
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
