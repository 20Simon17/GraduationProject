using UnityEngine;

public class DefaultMovementAction : PlayerActionStack.PlayerAction
{
    public DefaultMovementAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) { }
    
    private Vector2 moveDirection;
    
    private float forwardVelocity;
    private float strafeVelocity;
    private Vector2 horizontalVelocity = Vector2.zero;
    
    private float NormalizedForward => forwardVelocity / Mathf.Abs(forwardVelocity);
    private float NormalizedStrafe => strafeVelocity / Mathf.Abs(strafeVelocity);
    
    public override bool IsDone() => false;

    public override void OnUpdate(float deltaTime)
    {
        UpdateMovement(deltaTime);
        
        //TODO: if current speed is X more than "max speed", slowly remove speed back to the default
    }

    private void UpdateMovement(float fixedDeltaTime)
    {
        moveDirection = InputManager.Instance.moveDirection.normalized * data.maxRunVelocity;
        Vector3 newVelocity = new Vector3(moveDirection.x, rb.linearVelocity.y, moveDirection.y);

        rb.linearVelocity = transform.rotation * newVelocity;
    }
    
    /*private void UpdateMovement(float fixedDeltaTime)
    {
        horizontalVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
        moveDirection = InputManager.Instance.moveDirection;

        if (horizontalVelocity.magnitude != 0) CounterMovement(fixedDeltaTime);

        // Set our horizontal velocity to be our true velocity, excluding the vertical velocity.
        dataRecord.dataStruct.trueVelocity = horizontalVelocity.magnitude;

        // Handle all acceleration logic
        HandleAcceleration(fixedDeltaTime);

        // Apply our new velocities
        SetVelocity();
    }
    
    private void CounterMovement(float fixedDeltaTime)
    {
        if (moveDirection.x == 0)
        {
            if (Mathf.Abs(strafeVelocity) > 0.75)
            {
                float velocityLoss = NormalizedStrafe * data.decelerationForce * fixedDeltaTime;
                strafeVelocity -= velocityLoss;

                //if horizontalVelocity > max speed and you're not strafing, add the strafe counter velocity to the forward velocity to keep the speed the same.
                if (horizontalVelocity.magnitude > data.maxRunVelocity + 1)
                {
                    forwardVelocity += velocityLoss;
                }
            }
            else 
            {
                if (moveDirection.y != 0)
                {
                    forwardVelocity += strafeVelocity; //this might be adding in the wrong direction if the player is moving backwards
                }

                strafeVelocity = 0;
            }
        }

        if (moveDirection.y == 0)
        {
            if (Mathf.Abs(forwardVelocity) > 0.75)
            {
                forwardVelocity -= NormalizedForward * data.decelerationForce * fixedDeltaTime;
            }
            else forwardVelocity = 0;
        }
    }
    
    private void HandleAcceleration(float fixedDeltaTime)
    {
        if (moveDirection.y != 0)
        {
            float accelerationValue = moveDirection.y * data.accelerationForce * fixedDeltaTime;
            Vector2 hypotheticalVelocity = new Vector2(forwardVelocity + accelerationValue, strafeVelocity);

            if (hypotheticalVelocity.magnitude < data.maxRunVelocity)
            {
                forwardVelocity += accelerationValue;
            }
        }

        if (moveDirection.x != 0)
        {
            // Calculate the acceleration to add to our current strafe velocity
            float accelerationValue = moveDirection.x * data.accelerationForce * fixedDeltaTime;
            strafeVelocity += accelerationValue;

            if (horizontalVelocity.magnitude > data.maxRunVelocity)
            {
                // Remove the acceleration from the forward velocity to keep the magnitude of our velocity the same
                forwardVelocity -= NormalizedForward * Mathf.Abs(accelerationValue);
            }
        }

        if (Mathf.Abs(strafeVelocity) > data .maxStrafeVelocity)
        {
            strafeVelocity = NormalizedStrafe * data.maxStrafeVelocity;
        }
    }
    
    private void SetVelocity()
    {
        Vector3 vForwardVelocity = forwardVelocity * transform.forward;
        Vector3 vStrafeVelocity = strafeVelocity * transform.right;
        Vector3 vNewHorizontalVelocity = vForwardVelocity + vStrafeVelocity;
        
        if (dataRecord.isOnSlope)
        {
            vNewHorizontalVelocity = Vector3.ProjectOnPlane(vNewHorizontalVelocity, dataRecord.slopeNormal);
        }
        
        if (moveDirection != Vector2.zero)
        {
            // Set the velocity towards our characters forward
            rb.linearVelocity = new Vector3(vNewHorizontalVelocity.x, rb.linearVelocity.y, vNewHorizontalVelocity.z);
        }
        else
        {
            // Keep the velocity going the same direction but scale it to use our new values
            Vector3 newVelocity = rb.linearVelocity.normalized * vNewHorizontalVelocity.magnitude;
            rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.z);
        }
        
        dataRecord.dataStruct.forwardVelocity = forwardVelocity;
        dataRecord.dataStruct.strafeVelocity = strafeVelocity;
    }*/
}