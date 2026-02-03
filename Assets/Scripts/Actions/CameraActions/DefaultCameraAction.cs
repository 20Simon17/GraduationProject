using UnityEngine;
using UnityEngine.InputSystem;

public class DefaultCamera : ActionStack.ActionBehavior
{
    private float _verticalRotation = 0f;
    private float _clampAngleMin = -90f;
    private float _clampAngleMax = 90f;
    private float _mouseSensitivity = 2f;
    private GameObject _playerObject;

    public override bool IsDone() => false;

    public override void OnBegin(bool bFirstTime)
    {
        if (bFirstTime)
        {
            // get references to the settings for the camera
        }

        transform.localEulerAngles = _playerObject.transform.forward;

        InputManager.Instance.OnLookEvent += OnLook;
    }

    public override void OnEnd()
    {
        InputManager.Instance.OnLookEvent -= OnLook;
    }

    public override void OnUpdate()
    {
        
    }
    
    private void OnLook(InputValue value)
    {
        Vector2 lookVector = value.Get<Vector2>();
        
        _playerObject.transform.Rotate(Vector3.up, lookVector.x * _mouseSensitivity);
        
        _verticalRotation += -lookVector.y * _mouseSensitivity;
        _verticalRotation = Mathf.Clamp(_verticalRotation, _clampAngleMin, _clampAngleMax);
        transform.localEulerAngles = new Vector3(_verticalRotation, 0f, 0f);
    }
}