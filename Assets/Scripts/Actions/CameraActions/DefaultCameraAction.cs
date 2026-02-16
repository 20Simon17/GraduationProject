using UnityEngine;

public class DefaultCameraAction : CameraActionStack.CameraAction
{
    public DefaultCameraAction(Transform player, Transform camera)
        : base(player, camera) {}

    private float clampAngleMin = -90f;
    private float clampAngleMax = 90f;
    private float mouseSensitivity = 0.15f;
    private GameObject playerObject;

    public override bool IsDone() => false;

    public override void OnBegin(bool bFirstTime)
    {
        if (bFirstTime)
        {
            playerObject = PlayerTransform.gameObject;
        }

        VerticalRotation = 0f;
        CameraTransform.localEulerAngles = Vector3.zero;
    }
    
    public override void RotateCamera(Vector2 input)
    {
        playerObject?.transform.Rotate(Vector3.up, input.x * mouseSensitivity);

        VerticalRotation += -input.y * mouseSensitivity;
        VerticalRotation = Mathf.Clamp(VerticalRotation, clampAngleMin, clampAngleMax);
        CameraTransform.localEulerAngles = new Vector3(VerticalRotation, 0, 0);
    }
}