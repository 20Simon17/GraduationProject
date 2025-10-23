using UnityEngine;

public class PlayerJump
{
    private readonly Rigidbody _rb;
    private readonly PlayerData _data;
    private readonly Transform _transform;
    
    public PlayerJump(Rigidbody inRb, Transform inTransform, PlayerData inData)
    {
        _rb = inRb;
        _transform = inTransform;
        _data = inData;
    }

    public void Jump()
    {
        float now = Time.time;

        // The most recent of the two special jumps takes priority
        if (_data.timeAtLastSlide > _data.timeAtLastSlam)
        {
            if (CheckSlideJump()) return;
            else if (CheckSlamJump()) return;
        }
        else
        {
            if (CheckSlamJump()) return;
            else if (CheckSlideJump()) return;
        }
            
        float slideJumpMultiplier = _data.isSliding ? _data.slideJumpForceMultiplier : 1f;
        _rb.AddForce(_transform.up * _data.jumpForce * _data.jumpForceScaling * slideJumpMultiplier, ForceMode.Force);
        _data.CanJump = false;
    }

    private bool CheckSlideJump()
    {
        float now = Time.time; //extremely minor optimization would be to send this in as a parameter

        if (_data.timeAtLastSlide == 0 || !(now - _data.timeAtLastSlide <= _data.slideJumpTimeFrame)) return false;
        _data.timeAtLastSlide = 0;
                
        _rb.AddForce(_transform.forward * _data.slideJumpSpeedMultiplier, ForceMode.Force); //Little speed boost when jumping from slide
        _rb.AddForce(_transform.up * _data.jumpForce * _data.slideJumpForceMultiplier * _data.jumpForceScaling, ForceMode.Force); //Weaker jump when sliding
        _data.CanJump = false;
        return true;

    }

    private bool CheckSlamJump()
    {
        float now = Time.time; //extremely minor optimization would be to send this in as a parameter

        if (_data.timeAtLastSlam == 0 || !(now - _data.timeAtLastSlam <= _data.slamJumpTimeFrame)) return false;
        _data.timeAtLastSlam = 0;
                
        _rb.AddForce(_transform.up * _data.jumpForce * _data.slamJumpForceMultiplier * _data.jumpForceScaling, ForceMode.Force); //Higher jump when jumping from slam
        _data.CanJump = false;
        return true;
    }
}