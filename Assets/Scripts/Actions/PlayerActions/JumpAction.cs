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
        // TODO: if (forceJump) perform jump regardless of other conditions. (from zipline)
        // Do I want a way to combine slam and slide jump? slam -> land, slide + jump = forward + up boost
        if (CanDoubleJump())
        {
            PerformDoubleJump();
        }
        else if (CanSlamJump())
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

    private bool CanDoubleJump()
    {
        return false;
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
    
    private void PerformDoubleJump()
    {
        Debug.Log("Performing double jump");
        //Reset vertical velocity so we always get the same height from double jump
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); 
        rb.AddForce(transform.up * (data.jumpForce * data.jumpForceScaling), ForceMode.Force);
        dataRecord.CanJump = false;
    }
    
    private void PerformJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * (data.jumpForce * data.jumpForceScaling), ForceMode.Force);
        dataRecord.CanJump = false;
    }
}