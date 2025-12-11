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

    private float NormalizedForward => forwardVelocity / Mathf.Abs(forwardVelocity);
    private float NormalizedStrafe => strafeVelocity / Mathf.Abs(strafeVelocity);
    
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
        _horizontalVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.z);
        _moveDirection = InputManager.Instance.moveDirection;

        if (_horizontalVelocity.magnitude != 0) CounterMovement(fixedDeltaTime);

        // Set our horizontal velocity to be our true velocity, excluding the vertical velocity.
        _data.trueVelocity = _horizontalVelocity.magnitude;

        // Handle all acceleration logic
        HandleAcceleration(fixedDeltaTime);

        // Apply our new velocities
        SetVelocity();
    }

    //TODO:
    //if we press the input opposite of our move direction, we should decelerate faster / immediately
    //for example, if we are moving forward and press backward, we should stop almost immediately
    private void CounterMovement(float fixedDeltaTime)
    {
        if (_moveDirection.x == 0)
        {
            if (Mathf.Abs(strafeVelocity) > 0.75)
            {
                float velocityLoss = NormalizedStrafe * _data.decelerationForce * fixedDeltaTime;
                strafeVelocity -= velocityLoss;

                //if horizontalVelocity > max speed and you're not strafing, add the strafe counter velocity to the forward velocity to keep the speed the same.
                if (_horizontalVelocity.magnitude > _data.maxRunVelocity + 1)
                {
                    forwardVelocity += velocityLoss;
                }
            }
            else 
			{
				if (_moveDirection.y != 0)
				{
					forwardVelocity += strafeVelocity; //this might be adding in the wrong direction if the player is moving backwards
				}

				strafeVelocity = 0;
			}
        }

        if (_moveDirection.y == 0)
        {
            if (Mathf.Abs(forwardVelocity) > 0.75)
            {
                forwardVelocity -= NormalizedForward * _data.decelerationForce * fixedDeltaTime;
            }
            else forwardVelocity = 0;
        }
    }

    private void HandleAcceleration(float fixedDeltaTime)
    {
        if (_moveDirection.y != 0)
        {
            float accelerationValue = _moveDirection.y * _data.accelerationForce * fixedDeltaTime;
            Vector2 hypotheticalVelocity = new Vector2(forwardVelocity + accelerationValue, strafeVelocity);

            if (hypotheticalVelocity.magnitude < _data.maxRunVelocity)
            {
                forwardVelocity += accelerationValue;
            }
        }

        if (_moveDirection.x != 0)
        {
            // Calculate the acceleration to add to our current strafe velocity
            float accelerationValue = _moveDirection.x * _data.accelerationForce * fixedDeltaTime;
            strafeVelocity += accelerationValue;

            if (_horizontalVelocity.magnitude > _data.maxRunVelocity)
            {
                // Remove the acceleration from the forward velocity to keep the magnitude of our velocity the same
                forwardVelocity -= NormalizedForward * Mathf.Abs(accelerationValue);
            }
        }

        if (Mathf.Abs(strafeVelocity) > _data .maxStrafeVelocity)
        {
            strafeVelocity = NormalizedStrafe * _data.maxStrafeVelocity;
        }
    }

    private void SetVelocity()
    {
        Vector3 vForwardVelocity = forwardVelocity * _transform.forward;
        Vector3 vStrafeVelocity = strafeVelocity * _transform.right;
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