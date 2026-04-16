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
        dataRecord.hasJumped = true;
        dataRecord.timeAtLastJump = Time.time;
    }

    private bool CanSlideJump()
    {
        return dataRecord.timeAtLastSlide != 0 && Time.time - dataRecord.timeAtLastSlide <= data.slideJumpTimeFrame && CanJump();
    }
    
    private bool CanSlamJump()
    {
        return dataRecord.timeAtLastSlam != 0 && Time.time - dataRecord.timeAtLastSlam <= data.slamJumpTimeFrame && CanJump();
    }

    private bool CanJump()
    {
        return dataRecord.CanJump;
    }
    
    private void PerformSlideJump()
    {
        Debug.Log("Performing slide jump");
        dataRecord.timeAtLastSlide = 0;
                
        rb.AddForce(transform.forward * data.slideJumpSpeedMultiplier, ForceMode.Force); //Little speed boost when jumping from slide
        rb.AddForce(transform.up * (data.jumpForce * data.slideJumpForceMultiplier * data.jumpForceScaling), ForceMode.Force); //Weaker jump when sliding
        dataRecord.CanJump = false;
    }
    
    private void PerformSlamJump()
    {
        Debug.Log("Performing slam jump");
        dataRecord.timeAtLastSlam = 0;
                
        rb.AddForce(transform.up * (data.jumpForce * data.slamJumpForceMultiplier * data.jumpForceScaling), ForceMode.Force); //Higher jump when jumping from slam
        dataRecord.CanJump = false;
    }
    
    private void PerformJump()
    {
        Vector3 horizontalVelocity = new(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (dataRecord.canWallRunJump)
        {
            if (InputManager.Instance.moveDirection != Vector2.zero)
            {
                int direction = Vector3.Dot(horizontalVelocity, transform.forward) >= 0 ? 1 : -1;
                Debug.Log($"Performing wall jump with direction {direction}");
                rb.linearVelocity = rb.transform.rotation * (horizontalVelocity.normalized * rb.linearVelocity.magnitude * direction);
                rb.AddForce(transform.forward * data.wallRunJumpSpeedBoost, ForceMode.Force);
            }
        }
        else if (dataRecord.canWallClimbJump)
        {
            if (Vector3.Dot(transform.forward, -dataRecord.previousWallNormal) < 0)
            {
                rb.linearVelocity = horizontalVelocity;
                rb.AddForce(dataRecord.previousWallNormal * data.wallClimbJumpOutwardForce, ForceMode.Force);
            }
        }
        else rb.linearVelocity = horizontalVelocity;

        rb.AddForce(transform.up * (data.jumpForce * data.jumpForceScaling), ForceMode.Force);
        dataRecord.CanJump = false;
    }
}