using UnityEngine;

public class WallRunCameraAction : CameraActionStack.CameraAction
{
    public WallRunCameraAction(Transform player, Transform camera, Vector3 inWallNormal): base(player, camera)
    {
        wallNormal = inWallNormal;
    }

    private readonly float clampAngleMin = -90f;
    private readonly float clampAngleMax = 90f;
    private readonly float mouseSensitivity = 0.15f;
    private Vector3 wallNormal;
    private float horizontalRotation;
    private readonly float cameraTilt = 10f;

    private bool smoothRotate;
    private float targetRotation;
    private float smoothRotationTime;
    private float startRotation;
    private float previousTargetRotation;
    private float smoothRotationDuration = 0.15f;

    private bool isDone;

    public override bool IsDone() => isDone && !smoothRotate;

    public override void OnBegin(bool bFirstTime)
    {
        if (bFirstTime)
        {
            horizontalRotation = PlayerTransform.eulerAngles.y;

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
    
    public override void RotateCamera(Vector2 input)
    {
         //Rotate the player left/right based on the input
        horizontalRotation += input.x * mouseSensitivity;
        PlayerTransform.eulerAngles = new Vector3(PlayerTransform.eulerAngles.x, horizontalRotation, PlayerTransform.eulerAngles.z);

        //Rotate the camera up/down based on the input, clamp to min/max angles
        VerticalRotation += -input.y * mouseSensitivity;
        VerticalRotation = Mathf.Clamp(VerticalRotation, clampAngleMin, clampAngleMax);
        CameraTransform.localEulerAngles = new Vector3(VerticalRotation, CameraTransform.localEulerAngles.y, CameraTransform.localEulerAngles.z);
    }

    public override void OnUpdate(float deltaTime)
    {
        if (smoothRotate)
        {
            smoothRotationTime += deltaTime;
            float zRotation = Mathf.Lerp(startRotation, targetRotation, smoothRotationTime / smoothRotationDuration);
            CameraTransform.localEulerAngles = new Vector3(CameraTransform.localEulerAngles.x, CameraTransform.localEulerAngles.y, zRotation);

            //Debug.Log($"Smooth rotating camera. Time: {smoothRotationTime}, Start: {startRotation}, Target: {targetRotation}, Current: {zRotation}");

            if (smoothRotationTime >= smoothRotationDuration)
            {
                smoothRotate = false;
            }
        }
    }

    public override void OnEnd()
    {
        //SetCameraZRotation(0);
    }

    private void SetCameraZRotation(float zRotation)
    {
        smoothRotate = false;
        smoothRotationTime = 0;
        Debug.Log(CameraTransform.localRotation.z);
        previousTargetRotation = targetRotation;
        targetRotation = zRotation;
        startRotation = CameraTransform.localRotation.z;
        smoothRotate = true;
    }

    public void SetIsDone(bool newDone)
    {
        SetCameraZRotation(0);
        isDone = newDone;
    }
}