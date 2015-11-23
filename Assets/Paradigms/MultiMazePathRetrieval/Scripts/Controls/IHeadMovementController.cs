using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
 
public interface IHeadMovementController
{
    string Identifier { get; }

    void ApplyMovement(Transform head);
}