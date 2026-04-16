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
    private float maxHorizontalAngle = 25f;
    private GameObject playerObject;

    private bool isDone;

    public override bool IsDone() => isDone;

    public override void OnBegin(bool bFirstTime)
    {
        if (bFirstTime)
        {
            playerObject = PlayerTransform.gameObject;
            float dot = Vector3.Dot(PlayerTransform.right, wallNormal);
            if (dot > 0)
            {
                SetCameraZRotation(-10);
            }
            else
            {
                SetCameraZRotation(10);
            }
        }
    }
    
    public override void RotateCamera(Vector2 input)
    {
        Vector3 previousPlayerRotation = PlayerTransform.eulerAngles;
        playerObject?.transform.Rotate(Vector3.up, input.x * mouseSensitivity);

        if (Mathf.Abs(Vector3.Angle(PlayerTransform.eulerAngles, wallRunDirection)) > maxHorizontalAngle)
        {
            PlayerTransform.eulerAngles = previousPlayerRotation;
        }

        //Rotate the camera up/down based on the input, clamp to min/max angles
        VerticalRotation += -input.y * mouseSensitivity;
        VerticalRotation = Mathf.Clamp(VerticalRotation, clampAngleMin, clampAngleMax);
        CameraTransform.localEulerAngles = new Vector3(VerticalRotation, CameraTransform.localEulerAngles.y, CameraTransform.localEulerAngles.z);
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