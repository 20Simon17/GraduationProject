using UnityEngine;

public class PlayerSlide
{
    private readonly Rigidbody _rb;
    private PlayerDataStruct _data;
    private readonly Transform _transform;
    
    public PlayerSlide(Rigidbody inRb, Transform inTransform, PlayerDataStruct inData)
    {
        _rb = inRb;
        _transform = inTransform;
        _data = inData;
    }

    public void UpdateSlide()
    {
        if (_data.isSliding && _rb.linearVelocity.magnitude < _data.slideFallOfThreshold && _rb.linearVelocity.magnitude > _data.counterForceSpeedThreshold)
        {
            if(_rb.linearVelocity.magnitude < 1f)
            {
                _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
                return;
            }
            
            Vector3 relativeVelocity = _transform.InverseTransformPoint(_rb.linearVelocity);
            _rb.AddForce(-relativeVelocity.normalized * _data.slideFallOfForce, ForceMode.Acceleration);
        }
    }
    
    public void StartSlide()
    {
        _data.isSliding = true;
        _data.physicsMaterial.dynamicFriction = _data.slideFriction;
        _transform.localScale = new Vector3(_transform.localScale.x, _data.slidePlayerScaleY, _transform.localScale.z);

        if (!_data.isGrounded) return;
            
        _rb.AddForce(-_rb.transform.up * 100, ForceMode.Impulse); //Send the player downwards to stick to the ground //TODO: Ensure that this uses players transform
        _data.timeAtLastSlide = Time.time;
            
        if (_rb.linearVelocity.magnitude < _data.maxRunVelocity)
        {
            _rb.linearVelocity = _transform.forward * _data.slideSpeed;
        }
        else
        {
            _rb.linearVelocity = _transform.forward * _rb.linearVelocity.magnitude * _data.slideSpeedBoost;
        }
    }

    public void StopSlide()
    {
        _data.isSliding = false;
        _data.physicsMaterial.dynamicFriction = _data.defaultFriction;
        _transform.localScale = new Vector3(_transform.localScale.x, _data.defaultPlayerScaleY, _transform.localScale.z);
    }
}