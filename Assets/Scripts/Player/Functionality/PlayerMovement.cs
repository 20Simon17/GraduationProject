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
    private Vector2 _horizontalVelocity = Vector2.zero;

    

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
        if (_moveDirection.x == 0)
        {
            if (Mathf.Abs(strafeVelocity) > 0.75)
            {
                float velocityLoss = strafeVelocity / Mathf.Abs(strafeVelocity) * _data.decelerationForce * fixedDeltaTime;
                strafeVelocity -= velocityLoss;

                //if horizontalVelocity > max speed and you're not strafing, add the strafe counter velocity to the forward velocity to keep the speed the same.
                if (_horizontalVelocity.magnitude > _data.maxRunVelocity + 1)
                {
                    forwardVelocity += velocityLoss;
                }
            }
            else strafeVelocity = 0;
        }

        if (_moveDirection.y == 0)
        {
            if (Mathf.Abs(forwardVelocity) > 0.75)
            {
                forwardVelocity -= forwardVelocity / Mathf.Abs(forwardVelocity) * _data.decelerationForce * fixedDeltaTime;
            }
            else forwardVelocity = 0;
        }
    }

    private void HandleAcceleration(float fixedDeltaTime)
    {
        // Do we have forward input and are we moving less than the max speed?
        if (_moveDirection.y != 0 && _horizontalVelocity.magnitude < _data.maxRunVelocity)
        {
            if (Mathf.Abs(forwardVelocity) < _data.initialVelocity && Mathf.Abs(forwardVelocity) < 0.5) //this will break when decelerating, it will just go from initialVelocity to -initialVelocity immediately.
            {
                forwardVelocity = _moveDirection.y < 0 ? -_data.initialVelocity : _data.initialVelocity;
            }

            // Accelerate forwards
            forwardVelocity += _moveDirection.y * _data.accelerationForce * fixedDeltaTime;
        }
        
        // Do we have strafe input and is our strafe velocity less than its max?
        if (_moveDirection.x != 0 && Mathf.Abs(strafeVelocity) < _data.maxStrafeVelocity)
        {
            /*if (strafeVelocity < _data.initialVelocity && Mathf.Abs(strafeVelocity) < 0.5) //this will break when decelerating, it will just go from initialVelocity to -initialVelocity immediately.
            {
                strafeVelocity = _moveDirection.x < 0 ? -_data.initialVelocity : _data.initialVelocity;
                
                if (_horizontalVelocity.magnitude > _data.maxRunVelocity)
                {
                    forwardVelocity -= forwardVelocity / Mathf.Abs(forwardVelocity) * Mathf.Abs(strafeVelocity);
                }
            }*/
            
            // Calculate the acceleration to add to our current strafe velocity
            float extraVelocity = _moveDirection.x * _data.accelerationForce * fixedDeltaTime;
            strafeVelocity += extraVelocity;

            // If we're moving faster than our max
            if (_horizontalVelocity.magnitude > _data.maxRunVelocity)
            {
                // Remove the acceleration from the forward velocity to keep the magnitude of our velocity the same
                forwardVelocity -= forwardVelocity / Mathf.Abs(forwardVelocity) * Mathf.Abs(extraVelocity);
            }
        }

        if (Mathf.Abs(forwardVelocity) > _data.maxRunVelocity)
        {
            //forwardVelocity = forwardVelocity / Mathf.Abs(forwardVelocity) * _data.maxRunVelocity;
        }

        if (Mathf.Abs(strafeVelocity) > _data .maxStrafeVelocity)
        {
            strafeVelocity = strafeVelocity / Mathf.Abs(strafeVelocity) * _data.maxStrafeVelocity;
        }
    }

    public void UpdateMovement(float fixedDeltaTime)
    {
        _horizontalVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.z);

        if (_horizontalVelocity.magnitude != 0) CounterMovement(fixedDeltaTime);
        /*else
        {
            _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
        }*/

        // Set our horizontal velocity to be our true velocity, excluding the vertical velocity.
        _data.trueVelocity = _horizontalVelocity.magnitude;

        // Handle all acceleration logic
        HandleAcceleration(fixedDeltaTime);

        // Apply our new velocities
        SetVelocity();
    }

    private void SetVelocity()
    {
        Vector3 vForwardVelocity = /*_moveDirection.y **/ forwardVelocity * _transform.forward;
        Vector3 vStrafeVelocity = /*_moveDirection.x **/ strafeVelocity * _transform.right;
        Vector3 vNewHorizontalVelocity = vForwardVelocity + vStrafeVelocity;
        
        if (_moveDirection != Vector2.zero)
        {
            // Set the velocity towards our characters forward
            _rb.linearVelocity = new Vector3(vNewHorizontalVelocity.x, _rb.linearVelocity.y, vNewHorizontalVelocity.z);
        }
        else
        {
            // Keep the velocity going the same direction but scale it to use our new values
            Vector3 newVelocity = _rb.linearVelocity.normalized * vNewHorizontalVelocity.magnitude;
            _rb.linearVelocity = new Vector3(newVelocity.x, _rb.linearVelocity.y, newVelocity.z);
        }

        _data.forwardVelocity = forwardVelocity;
        _data.strafeVelocity = strafeVelocity;
    }
}