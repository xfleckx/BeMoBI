using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Paradigms.MultiMazePathRetrieval
{
    interface IBodyMovementController
    {
        void ApplyMovement(CharacterController controller);
    }
}
