using UnityEngine;

public class PlayerSlam
{
    private readonly Rigidbody _rb;
    private readonly PlayerData _data;
    private readonly Transform _transform;
    
    public PlayerSlam(Rigidbody inRb, Transform inTransform, PlayerData inData)
    {
        _rb = inRb;
        _transform = inTransform;
        _data = inData;
    }

    public void StartSlam()
    {
        if (!_data.isGrounded)
        {
            _data.isSlamming = true;
            _rb.AddForce(-_transform.up * _data.groundSlamForce * 100, ForceMode.VelocityChange);
        }
        else
        {
            _data.timeAtLastSlam = Time.time;
        }
    }

    public void StopSlam()
    {
        _data.isSlamming = false;
        
        //Start cooldown for when the next slam could be done?
    }
}

// if slamming, no air movement (lose all velocity except downwards)
// otherwise, air movement as normal
// gives incentive to avoid the slam as it would otherwise always be used to land faster and gain more speed