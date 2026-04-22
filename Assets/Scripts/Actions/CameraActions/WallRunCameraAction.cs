using System;
using UnityEngine;

public class WallRunCameraAction : CameraActionStack.CameraAction
{
    public WallRunCameraAction(Transform player, Transform camera, Vector3 inWallNormal, Vector3 inWallRunDirection)
        : base(player, camera)
    {
        wallNormal = inWallNormal;
        wallRunDirection = inWallRunDirection;
    }

    private float clampAngleMin = -90f;
    private float clampAngleMax = 90f;
    private float mouseSensitivity = 0.15f;
    private Vector3 wallNormal;
    private Vector3 wallRunDirection;
    private float maxHorizontalAngle = 35f;
    private float forwardY;
    private float horizontalRotation;
    private float cameraTilt = 5f;

    private bool isDone;

    public override bool IsDone() => isDone;

    public override void OnBegin(bool bFirstTime)
    {
        if (bFirstTime)
        {
            GetRotationOffset();
            Debug.Log("Camera direction is " + wallRunDirection);

            float dot = Vector3.Dot(PlayerTransform.right, wallNormal);
            if (dot > 0)
            {
                SetCameraZRotation(-cameraTilt);
            }
            else
            {
                SetCameraZRotation(cameraTilt);
            }
        }
    }

    private void GetRotationOffset()
    {
        forwardY = wallRunDirection.y;

        horizontalRotation = PlayerTransform.eulerAngles.y - forwardY;
        horizontalRotation = Mathf.Clamp(horizontalRotation, forwardY - maxHorizontalAngle, forwardY + maxHorizontalAngle);
        PlayerTransform.eulerAngles = new Vector3(PlayerTransform.eulerAngles.x, horizontalRotation, PlayerTransform.eulerAngles.z);
    }
    
    public override void RotateCamera(Vector2 input)
    {
         //Rotate the player left/right based on the input, clamp to min/max angles
        horizontalRotation += input.x * mouseSensitivity;
        horizontalRotation = Mathf.Clamp(horizontalRotation, forwardY - maxHorizontalAngle, forwardY + maxHorizontalAngle);
        PlayerTransform.eulerAngles = new Vector3(PlayerTransform.eulerAngles.x, horizontalRotation, PlayerTransform.eulerAngles.z);

        //Rotate the camera up/down based on the input, clamp to min/max angles
        VerticalRotation += -input.y * mouseSensitivity;
        VerticalRotation = Mathf.Clamp(VerticalRotation, clampAngleMin, clampAngleMax);
        CameraTransform.localEulerAngles = new Vector3(VerticalRotation, CameraTransform.localEulerAngles.z, CameraTransform.localEulerAngles.z);
    }

    public override void OnEnd()
    {
        SetCameraZRotation(0);
    }

    private void SetCameraZRotation(float zRotation)
    {
        CameraTransform.localEulerAngles = new Vector3(CameraTransform.localEulerAngles.x, CameraTransform.localEulerAngles.y, zRotation);
    }

    public void SetIsDone(bool newDone)
    {
        isDone = newDone;
    }
}