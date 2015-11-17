using UnityEngine;
using System.Collections;
using Assets.Paradigms.MultiMazePathRetrieval;
using System;



/// <summary>
/// Custom Control Implementation for Rotating the body with Joystick x achses and movement with its y axes 
/// </summary>
public class JoystickControl : MonoBehaviour, IBodyMovementController
{
    private const string X_AXIS_NAME = "Horizontal";
    private const string Y_AXIS_NAME = "Vertical";

    public float xSensitivity = 1f;
    public float ySensitivity = 1f;

    [Range(10, 50)]
    public float WalkingSpeed = 10f;
    [Range(10, 50)]
    public float RotationSpeed = 40f;

    public bool RotateSmooth = true;
    public float SmoothTime = 1;

    private Quaternion targetRotation; 

    public void ApplyMovement(CharacterController character)
    {
        targetRotation = character.transform.rotation;
        
        var x = Input.GetAxis(X_AXIS_NAME); // use as body rotation

        var y = Input.GetAxis(Y_AXIS_NAME); // use as movement to forward / backwards

        Vector3 desiredMove = transform.forward * y * WalkingSpeed * Time.deltaTime;

        targetRotation *= Quaternion.Euler(0f, x * RotationSpeed, 0f);
        
        if (RotateSmooth)
        {
            character.transform.rotation = Quaternion.Slerp(character.transform.rotation, targetRotation,
                SmoothTime * Time.deltaTime);
        }
        else {
            character.transform.rotation = targetRotation;
        }

        character.Move(desiredMove);
    }
}
