using UnityEngine;

public class DefaultCameraAction : CameraActionStack.CameraAction
{
    private float _clampAngleMin = -90f;
    private float _clampAngleMax = 90f;
    private float _mouseSensitivity = 0.15f;
    private GameObject _playerObject;

    public override bool IsDone() => false;

    public override void OnBegin(bool bFirstTime)
    {
        if (bFirstTime)
        {
            //TODO: Refactor the way this gets the camera reference
            CameraTransform = Camera.main.transform;
            _playerObject = CameraTransform.parent.gameObject;
        }

        VerticalRotation = 0f;
        CameraTransform.localEulerAngles = Vector3.zero;
    }
    
    public override void RotateCamera(Vector2 input)
    {
        _playerObject.transform.Rotate(Vector3.up, input.x * _mouseSensitivity);

        VerticalRotation += -input.y * _mouseSensitivity;
        VerticalRotation = Mathf.Clamp(VerticalRotation, _clampAngleMin, _clampAngleMax);
        CameraTransform.localEulerAngles = new Vector3(VerticalRotation, 0, 0);
    }
}