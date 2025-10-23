using UnityEngine;

public class PlayerMovement
{
    private Vector2 _moveDirection;
    private readonly Rigidbody _rb;
    private readonly PlayerData _data;
    private readonly Transform _transform;
    
    public PlayerMovement(Rigidbody inRb, Transform inTransform, PlayerData inData)
    {
        _rb = inRb;
        _transform = inTransform;
        _data = inData;
    }
    
    public void SetMoveDirection(Vector2 direction)
    {
        _moveDirection = direction;
    }

    public void UpdateMovement()
    {
        if (_moveDirection == Vector2.zero && _rb.linearVelocity.magnitude > _data.counterForceSpeedThreshold && _data.isGrounded)
        {
            Vector3 counterForce = new Vector3(-_rb.linearVelocity.x, 0, -_rb.linearVelocity.z).normalized;
            _rb.AddForce(counterForce * _data.decelerationForce);
        }
        else if (_rb.linearVelocity.magnitude < _data.maxRunVelocity)
        {
            _rb.AddForce(_transform.forward * (_moveDirection.y * _data.accelerationForce));
            _rb.AddForce(_transform.right * (_moveDirection.x * _data.accelerationForce));
        }
    }
}