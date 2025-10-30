using UnityEngine;

public class PlayerMovement
{
    private Vector2 _moveDirection;
    private readonly Rigidbody _rb;
    private readonly PlayerData _data;
    private readonly Transform _transform;

    private float forwardVelocity = 0;
    private float strafeVelocity = 0;
    private float initialVelocity = 0;
    
    public PlayerMovement(Rigidbody inRb, Transform inTransform, PlayerData inData)
    {
        _rb = inRb;
        _transform = inTransform;
        _data = inData;

        forwardVelocity = _data.forwardVelocity;
        strafeVelocity = _data.strafeVelocity;
        initialVelocity = _data.initialVelocity;
    }
    
    public void SetMoveDirection(Vector2 direction)
    {
        _moveDirection = direction;
    }

    public void UpdateMovement(float fixedDeltaTime)
    {
        Vector2 horizontalVelocity2D = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.z);
        if (horizontalVelocity2D.magnitude < _data.maxRunVelocity)
        {
            if (_moveDirection.y != 0)
            {
                /*if (_moveDirection.y > 0) forwardVelocity += _data.accelerationForce * fixedDeltaTime;
                else forwardVelocity -= _data.accelerationForce * fixedDeltaTime;*/

                forwardVelocity += _moveDirection.y * _data.accelerationForce * fixedDeltaTime;
            }

            if (_moveDirection.x != 0)              
            {
                /*if (_moveDirection.x > 0) strafeVelocity += _data.accelerationForce * fixedDeltaTime;
                else strafeVelocity -= _data.accelerationForce * fixedDeltaTime;*/
                
                strafeVelocity += _moveDirection.x * _data.accelerationForce * fixedDeltaTime;
            }
        }
        
        if (_moveDirection != Vector2.zero) // change this to not happen if the new velocity is opposite of the other one
        {
            Vector3 horizontalVelocity = _transform.forward * forwardVelocity + _transform.right * strafeVelocity;
            _rb.linearVelocity = new Vector3(horizontalVelocity.x, _rb.linearVelocity.y, horizontalVelocity.z);
        }
        
        _data.forwardVelocity = forwardVelocity;
        _data.strafeVelocity = strafeVelocity;
    }
}