using UnityEngine;

public class FreeMoveCameraAction : CameraActionStack.CameraAction
{
    public FreeMoveCameraAction(Transform player, Transform camera)
        : base(player, camera) {}
    
    private float clampAngleMin = -85f;
    private float clampAngleMax = 90f;
    private float mouseSensitivity = 0.15f;
    
    private bool isDone;

    public override bool IsDone() => isDone;

    public override void OnBegin(bool bFirstTime)
    {
        if (bFirstTime)
        {
            CameraTransform = Camera.main.transform;
        }
        
        VerticalRotation = CameraTransform.rotation.x;
    }
    
    public override void RotateCamera(Vector2 input)
    {
        if (CameraTransform == null) return;
        
        VerticalRotation += -input.y * mouseSensitivity;
        VerticalRotation = Mathf.Clamp(VerticalRotation, clampAngleMin, clampAngleMax);
        
        CameraTransform.Rotate(Vector3.up, input.x * mouseSensitivity);
        CameraTransform.localEulerAngles = new Vector3(VerticalRotation, CameraTransform.localEulerAngles.y, 0);
    }

    public void SetIsDone(bool newDone)
    {
        isDone = newDone;
    }
}
