using UnityEngine;

public class DefaultMovementAction : PlayerActionStack.PlayerAction
{
    public DefaultMovementAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData)
        : base(inRb, inTransform, inData) { }
    
    private Vector2 moveDirection;
    private Vector3 HorizontalVelocity => new(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    
    public override bool IsDone() => false;

    public override void OnUpdate(float deltaTime)
    {
        UpdateMovement(deltaTime);
    }

    private void UpdateMovement(float fixedDeltaTime)
    {
        moveDirection = InputManager.Instance.moveDirection.normalized;

        if (moveDirection != Vector2.zero)
        {
            if (playerData.isGrounded) HandleGroundMovement(moveDirection);
            else HandleAirMovement(moveDirection, fixedDeltaTime);
        }
        else if (rb.linearVelocity.x != 0 && rb.linearVelocity.z != 0)
        {
            if (playerData.isGrounded)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
            else
            {
                Vector3 horizontalVelocity = HorizontalVelocity;
                horizontalVelocity = horizontalVelocity.normalized * (horizontalVelocity.magnitude - (staticData.decelerationForce * fixedDeltaTime));
                rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
            }
        }
        
        // Slope movement
        if (playerData.isOnSlope && playerData.isGrounded)
        {
            rb.linearVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, playerData.slopeNormal).normalized * rb.linearVelocity.magnitude;
        }
        
        // TODO: If the player runs from flat ground onto a slope, make the movement follow the slope direction immediately
        // UNLESS the players velocity is greater than a certain threshold. Then it wouldn't make sense to have such a sharp
        // direction change. In that case, just make the player keep the velocity direction (would result in being airborne)
    }

    private void HandleGroundMovement(Vector2 moveDirection)
    {
        if (rb.linearVelocity.magnitude > staticData.maxRunVelocity)
        {
            Vector2 scaledInput = moveDirection * HorizontalVelocity.magnitude;
            Vector3 newVelocity = new(scaledInput.x, rb.linearVelocity.y, scaledInput.y);
            rb.linearVelocity = transform.rotation * newVelocity;
        }
        else
        {
            Vector3 newVelocity = new(moveDirection.x * staticData.maxRunVelocity, rb.linearVelocity.y, moveDirection.y * staticData.maxRunVelocity);
            rb.linearVelocity = transform.rotation * newVelocity;
        }
    }

    private void HandleAirMovement(Vector2 moveDirection, float fixedDeltaTime)
    {
        float acceleration = staticData.accelerationForce * fixedDeltaTime;
        Vector3 accelerationAmount = transform.right * (moveDirection.x * acceleration) + transform.forward * (moveDirection.y * acceleration);

        Vector3 newVelocity = rb.linearVelocity + accelerationAmount;
        if (HorizontalVelocity.magnitude < staticData.maxRunVelocity)
        {
            rb.linearVelocity = newVelocity;
        }
        else
        {
            Vector2 hVelocity = new(rb.linearVelocity.x, rb.linearVelocity.z);
            Vector2 hNewVelocity = new(newVelocity.x, newVelocity.z);
            
            if (hNewVelocity.magnitude > hVelocity.magnitude)
            {
                Vector2 scaledNewVelocity = hNewVelocity.normalized * hVelocity.magnitude;
                rb.linearVelocity = new Vector3(scaledNewVelocity.x, rb.linearVelocity.y, scaledNewVelocity.y);
            }
            else rb.linearVelocity = newVelocity;
        }
    }
}