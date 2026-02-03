using UnityEngine;
using UnityEngine.InputSystem;

public class FreeMoveCameraAction : ActionStack.ActionBehavior
{
    private float _verticalRotation;
    private float _clampAngleMin = -90f;
    private float _clampAngleMax = 90f;
    private float _mouseSensitivity = 2f;
    private GameObject _playerObject;
    
    private bool _isDone = false;

    public override bool IsDone() => _isDone;

    public override void OnBegin(bool bFirstTime)
    {
        if (bFirstTime)
        {
            
        }
        
        _verticalRotation = transform.localEulerAngles.x;
        
        InputManager.Instance.OnLookEvent += OnLook;
    }

    public override void OnEnd()
    {
        InputManager.Instance.OnLookEvent -= OnLook;
    }

    public override void OnUpdate()
    {
        // if toggle mode is on, only check value.isPressed in the input function.
        // else, do and if else.
    }

    private void OnLook(InputValue value)
    {
        Vector2 lookVector = value.Get<Vector2>();
        
        _verticalRotation += -lookVector.y * _mouseSensitivity;
        _verticalRotation = Mathf.Clamp(_verticalRotation, _clampAngleMin, _clampAngleMax);
        transform.localEulerAngles = new Vector3(_verticalRotation, 0f, 0f);
    }
}
