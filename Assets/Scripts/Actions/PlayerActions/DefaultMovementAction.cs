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
        moveDirection = InputManager.Instance.moveDirection.normalized;

        if (moveDirection != Vector2.zero)
        {
            // TODO: if the player is grounded, use this movement below
            // else use acceleration instead of instant speed
            if (dataRecord.isGrounded)
            {
                if (rb.linearVelocity.magnitude > data.maxRunVelocity)
                {
                    Vector2 hVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
                    Vector2 scaledInput = moveDirection * hVelocity.magnitude;
            
                    Vector3 newVelocity = new Vector3(scaledInput.x, rb.linearVelocity.y, scaledInput.y);
                    rb.linearVelocity = transform.rotation * newVelocity;
                }
                else
                {
                    Vector3 newVelocity = new Vector3(moveDirection.x * data.maxRunVelocity, rb.linearVelocity.y, moveDirection.y * data.maxRunVelocity);
                    rb.linearVelocity = transform.rotation * newVelocity;
                }
            }
            else
            {
                Vector3 accelerationAmount = transform.right * (moveDirection.x * data.accelerationForce * fixedDeltaTime) + 
                                             transform.forward * (moveDirection.y * data.accelerationForce * fixedDeltaTime);
                
                Vector3 newVelocity = rb.linearVelocity + accelerationAmount;

                Vector2 hVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
                Vector2 hNewVelocity = new Vector2(newVelocity.x, newVelocity.z);
                
                if (hNewVelocity.magnitude > hVelocity.magnitude)
                {
                    Vector2 scaledNewVelocity = hNewVelocity.normalized * hVelocity.magnitude;
                    rb.linearVelocity = new Vector3(scaledNewVelocity.x, rb.linearVelocity.y, scaledNewVelocity.y);
                }
                else
                {
                    rb.linearVelocity = newVelocity;
                }
                
                //rb.linearVelocity += accelerationAmount;
            }
        }
        else if (rb.linearVelocity.x != 0 && rb.linearVelocity.z != 0)
        {
            if (dataRecord.isGrounded)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
            else
            {
                Vector2 hVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
                Vector2 hVelocityNormalized = hVelocity.normalized;
            
                int xScaleFactor = hVelocityNormalized.x > 0 ? 1 : -1;
                int yScaleFactor = hVelocityNormalized.y > 0 ? 1 : -1;
            
                hVelocity.x -= xScaleFactor * data.decelerationForce * Time.deltaTime;
                hVelocity.y -= yScaleFactor * data.decelerationForce * Time.deltaTime;
            
                if (Mathf.Abs(hVelocity.x) < 2) hVelocity.x = 0;
                if (Mathf.Abs(hVelocity.y) < 2) hVelocity.y = 0;
            
                rb.linearVelocity = new Vector3(hVelocity.x, rb.linearVelocity.y, hVelocity.y);
            }
        }
        
        if (dataRecord.isOnSlope && dataRecord.isGrounded)
        {
            rb.linearVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, dataRecord.slopeNormal).normalized * rb.linearVelocity.magnitude;
        }
        
        // TODO: If the player runs from flat ground onto a slope, make the movement follow the slope direction immediately
        // UNLESS the players velocity is greater than a certain threshold. Then it wouldn't make sense to have such a sharp
        // direction change. In that case, just make the player keep the velocity direction (would result in being airborne)
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