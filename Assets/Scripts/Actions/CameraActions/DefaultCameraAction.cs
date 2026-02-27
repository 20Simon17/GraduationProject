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

        // Reset the rotation to face the exact forward of the player object
        VerticalRotation = 0f;
        CameraTransform.localEulerAngles = Vector3.zero;
    }
    
    public override void RotateCamera(Vector2 input)
    {
        //Rotate the player left/right based on the input
        playerObject?.transform.Rotate(Vector3.up, input.x * mouseSensitivity);

        //Rotate the camera up/down based on the input, clamp to min/max angles
        VerticalRotation += -input.y * mouseSensitivity;
        VerticalRotation = Mathf.Clamp(VerticalRotation, clampAngleMin, clampAngleMax);
        CameraTransform.localEulerAngles = new Vector3(VerticalRotation, 0, 0);
    }
}