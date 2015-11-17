using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public interface IBodyMovementController
{
    string Identifier { get; }

    void ApplyMovement(CharacterController controller);
}