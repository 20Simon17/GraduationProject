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

    private void CounterMovement(float fixedDeltaTime)
    {
        if (_moveDirection.x == 0 && Mathf.Abs(strafeVelocity) > 1)
        {
            strafeVelocity -= strafeVelocity / Mathf.Abs(strafeVelocity) * _data.decelerationForce * fixedDeltaTime;
        }

        if (_moveDirection.y == 0 && Mathf.Abs(forwardVelocity) > 1)
        {
            forwardVelocity -= forwardVelocity / Mathf.Abs(forwardVelocity) * _data.decelerationForce * fixedDeltaTime;
        }
    }

    public void UpdateMovement(float fixedDeltaTime)
    {
        Vector2 horizontalVelocity2D = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.z);
        
        if (horizontalVelocity2D.magnitude > 1)
            CounterMovement(fixedDeltaTime);
        else
        {
            strafeVelocity = 0;
            forwardVelocity = 0;
            _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
        }
        
        _data.trueVelocity = horizontalVelocity2D.magnitude;
        if (horizontalVelocity2D.magnitude < _data.maxRunVelocity && _moveDirection != Vector2.zero)
        {
            if (_moveDirection.y != 0)
            {
                if (Mathf.Abs(forwardVelocity) < _data.initialVelocity) //this will break when decelerating, it will just go from initialVelocity to -initialVelocity immediately.
                {
                    forwardVelocity = _moveDirection.y < 0 ? -1 * _data.initialVelocity : _data.initialVelocity;
                }
                forwardVelocity += _moveDirection.y * _data.accelerationForce * fixedDeltaTime;
            }
            
            if (_moveDirection.x != 0)              
            {
                if (strafeVelocity < _data.initialVelocity)
                {
                    strafeVelocity = _moveDirection.x < 0 ? -1 * _data.initialVelocity : _data.initialVelocity;
                }
                strafeVelocity += _moveDirection.x * _data.accelerationForce * fixedDeltaTime;
            }
        }
        
        Vector3 horizontalVelocity = _moveDirection.y * forwardVelocity * _transform.forward +
                                     _moveDirection.x * strafeVelocity  * _transform.right;
        if (_moveDirection != Vector2.zero) // change this to not happen if the new velocity is opposite of the other one
        {
            _rb.linearVelocity = new Vector3(horizontalVelocity.x, _rb.linearVelocity.y, horizontalVelocity.z);
        }
        else
        {
            Vector3 newVelocity = _rb.linearVelocity.normalized * horizontalVelocity.magnitude;
            _rb.linearVelocity = new Vector3(newVelocity.x, _rb.linearVelocity.y, newVelocity.z);
        }
        
        _data.forwardVelocity = forwardVelocity;
        _data.strafeVelocity = strafeVelocity;
    }
}