using UnityEngine;

public class JumpAction : PlayerActionStack.PlayerAction
{
    private bool actionCompleted;
    
    public override bool IsDone() => actionCompleted;

    public override void OnBegin(bool bFirstTime)
    {
        CheckJumps();
    }

    private void CheckJumps()
    {
        // TODO: Do I want a way to combine slam and slide jump? slam -> land, slide + jump = forward + up boost
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
        
        actionCompleted = true;
    }

    private bool CanSlideJump()
    {
        return data.timeAtLastSlide != 0 && Time.time - data.timeAtLastSlide <= data.slideJumpTimeFrame;
    }
    
    private bool CanSlamJump()
    {
        return data.timeAtLastSlam != 0 && Time.time - data.timeAtLastSlam <= data.slamJumpTimeFrame;
    }

    private bool CanDoubleJump()
    {
        return false;
    }

    private bool CanJump()
    {
        return data.CanJump;
    }
    
    private void PerformSlideJump()
    {
        data.timeAtLastSlide = 0;
                
        rb.AddForce(transform.forward * data.slideJumpSpeedMultiplier, ForceMode.Force); //Little speed boost when jumping from slide
        rb.AddForce(transform.up * (data.jumpForce * data.slideJumpForceMultiplier * data.jumpForceScaling), ForceMode.Force); //Weaker jump when sliding
        data.CanJump = false;
    }
    
    private void PerformSlamJump()
    {
        data.timeAtLastSlam = 0;
                
        rb.AddForce(transform.up * (data.jumpForce * data.slamJumpForceMultiplier * data.jumpForceScaling), ForceMode.Force); //Higher jump when jumping from slam
        data.CanJump = false;
    }
    
    private void PerformDoubleJump()
    {
        //Reset vertical velocity so we always get the same height from double jump
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); 
        rb.AddForce(transform.up * (data.jumpForce * data.jumpForceScaling), ForceMode.Force);
        data.CanJump = false;
    }
    
    private void PerformJump()
    {
        rb.AddForce(transform.up * (data.jumpForce * data.jumpForceScaling), ForceMode.Force);
        data.CanJump = false;
    }
}
