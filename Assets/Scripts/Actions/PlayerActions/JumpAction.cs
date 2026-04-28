using UnityEngine;

public class JumpAction : PlayerActionStack.PlayerAction
{
	public JumpAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData) 
        : base(inRb, inTransform, inData) {}

    public override void OnBegin(bool bFirstTime)
    {
        CheckJumps();
    }

    private void CheckJumps()
    {
        if (CanSlamJump())
        {
            PerformSlamJump();
        }
        else if (CanSlideJump())
        {
            PerformSlideJump();
        }
        else if (CanJump())
        {
            PerformJump();
        }
        
        ActionCompleted = true;
        playerData.hasJumped = true;
        playerData.timeAtLastJump = Time.time;
    }

    private bool CanSlideJump()
    {
        return playerData.timeAtLastSlide != 0 && Time.time - playerData.timeAtLastSlide <= staticData.slideJumpTimeFrame && CanJump();
    }
    
    private bool CanSlamJump()
    {
        return playerData.timeAtLastSlam != 0 && Time.time - playerData.timeAtLastSlam <= staticData.slamJumpTimeFrame && CanJump();
    }

    private bool CanJump()
    {
        return playerData.CanJump;
    }
    
    private void PerformSlideJump()
    {
        Debug.Log("Performing slide jump");
        playerData.timeAtLastSlide = 0;
                
        rb.AddForce(transform.forward * staticData.slideJumpSpeedMultiplier, ForceMode.Force); //Little speed boost when jumping from slide
        rb.AddForce(transform.up * (staticData.jumpForce * staticData.slideJumpForceMultiplier * staticData.jumpForceScaling), ForceMode.Force); //Weaker jump when sliding
        playerData.CanJump = false;
    }
    
    private void PerformSlamJump()
    {
        Debug.Log("Performing slam jump");
        playerData.timeAtLastSlam = 0;
                
        rb.AddForce(transform.up * (staticData.jumpForce * staticData.slamJumpForceMultiplier * staticData.jumpForceScaling), ForceMode.Force); //Higher jump when jumping from slam
        playerData.CanJump = false;
    }
    
    private void PerformJump()
    {
        Vector3 horizontalVelocity = new(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (playerData.canWallRunJump)
        {
            if (InputManager.Instance.moveDirection != Vector2.zero)
            {
                float forwardDot = Vector3.Dot(transform.forward, playerData.previousWallRunDirection);
                float outwardDot = Vector3.Dot(transform.forward, -playerData.previousWallNormal);

                Vector3 jumpDirection = playerData.previousWallNormal;
                if (forwardDot > outwardDot)
                {
                    jumpDirection = transform.forward;
                }

                rb.linearVelocity = jumpDirection * rb.linearVelocity.magnitude;
                rb.AddForce(transform.forward * staticData.wallRunJumpSpeedBoost, ForceMode.Force);
            }
        }
        else if (playerData.canWallClimbJump)
        {
            if (Vector3.Dot(transform.forward, -playerData.previousWallNormal) < 0)
            {
                rb.linearVelocity = horizontalVelocity;
                rb.AddForce(playerData.previousWallNormal * staticData.wallClimbJumpOutwardForce, ForceMode.Force);
            }
        }
        else if (playerData.timeAtLastPullGrapple != 0 && Time.time - playerData.timeAtLastPullGrapple <= staticData.pullGrappleJumpTimeFrame)
        {
            Debug.Log("Grapple Jump");
            rb.linearVelocity = horizontalVelocity;
            rb.AddForce(transform.forward * staticData.grappleJumpForwardForce, ForceMode.Impulse);
        }
        else rb.linearVelocity = horizontalVelocity;

        rb.AddForce(transform.up * (staticData.jumpForce * staticData.jumpForceScaling), ForceMode.Force);
        playerData.CanJump = false;
    }
}