using UnityEngine;
using UnityEngine.InputSystem;

public class ZiplineAction : PlayerActionStack.PlayerAction
{
    public ZiplineAction(Rigidbody inRb, Transform inTransform, PlayerDataRecord inData, Zipline inZipline)
        : base(inRb, inTransform, inData)
    {
        attachedZipline = inZipline; 
    }

    private readonly Zipline attachedZipline;
    private Vector3 ziplineDirection;
    private float ziplineAngleAcceleration;
    private Vector3 gravityUponEntering;

    private Vector3 velocityDirection;

    private readonly float flatZiplineDeacceleration = -2;
    
    public override bool IsDone()
    {
        if (!attachedZipline) return true;
        if (ziplineDirection == Vector3.zero && rb.linearVelocity.magnitude <= 0)
        {
            return true;
        }
        
        if (!attachedZipline.IsPointOnZipline(transform.position + Vector3.up * transform.localScale.y))
        {
            return true;
        }
        
        return ActionCompleted;
    }

    public override void OnBegin(bool bFirstTime)
    {
        dataRecord.isOnZipline = true;
        if (!bFirstTime) return;
        
        //TODO: Fix zipline bug where if you enter at the very edge it gets cancelled because the player is "not on the zipline" anymore
        // to do this, if the point where the player would be attached to the zipline is equal to one of the attach locations, move it in a bit

        InputManager.Instance.OnCrouchEvent += DropFromZipline;
        
        dataRecord.CanJump = true;
        
        // attach the player to the zipline
        Vector3 attachLocation = attachedZipline.GetClosestPointOnZipline(transform.position);
        attachLocation -= Vector3.up * transform.localScale.y;
        transform.position = attachLocation;

        // get the ziplines direction
        ziplineDirection = attachedZipline.GetZiplineDirection();
        
        // calculate player carryover speed from outside zipline onto the zipline
        rb.linearVelocity = CalculateCarryOverVelocity();

        // calculate acceleration on the zipline based on the vertical angle of it
        if (ziplineDirection == Vector3.zero)
        {
            ziplineAngleAcceleration = flatZiplineDeacceleration;
        }
        else
        {
            //TODO: Fix sloped acceleration calculation, currently does not work as intended
            Vector3 flatZiplineDirection = new Vector3(ziplineDirection.x, 0, ziplineDirection.z);
            float angleDifference = 1 - Vector3.Dot(ziplineDirection, flatZiplineDirection);
            
            float velocityDirectionDot = Vector3.Dot(velocityDirection, ziplineDirection);
            ziplineAngleAcceleration = velocityDirectionDot < 0 ? angleDifference : -angleDifference;
            ziplineAngleAcceleration *= data.defaultGravity.magnitude;
        }
        
        // set the gravity
        gravityUponEntering = Physics.gravity;
        Physics.gravity = Vector3.zero;
    }

    public override void OnEnd()
    {
        InputManager.Instance.OnCrouchEvent -= DropFromZipline;
        
        dataRecord.CanJump = false;
        Physics.gravity = gravityUponEntering;

        attachedZipline.isInUse = false;
        dataRecord.isOnZipline = false;
    }

    public override void OnUpdate(float deltaTime)
    {
        //Debug.Log($"Players velocity: {rb.linearVelocity.magnitude}");
        //Debug.Log($"Player acceleration: {ziplineAngleAcceleration}");
        rb.linearVelocity += velocityDirection * (ziplineAngleAcceleration * deltaTime);
        
        if (ziplineDirection == Vector3.zero && rb.linearVelocity.magnitude <= data.ziplineAutoDropVelocity)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private Vector3 CalculateCarryOverVelocity()
    {
        Vector3 nonZeroZiplineDirection = attachedZipline.GetZiplineDirectionNonZero();
            
        float dot = Vector3.Dot(rb.linearVelocity.normalized,  nonZeroZiplineDirection);
        float absoluteDot = Mathf.Abs(dot);

        Vector3 flatPlayerVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        velocityDirection = dot > 0 ? nonZeroZiplineDirection : -nonZeroZiplineDirection;
        Vector3 resultingVelocity = velocityDirection * (flatPlayerVelocity.magnitude * absoluteDot);
        
        if (ziplineDirection != Vector3.zero)
        {
            Vector3 verticalZiplineDirection = new Vector3(0, nonZeroZiplineDirection.y, 0);
            Vector3 verticalVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            float verticalSimilarity = Vector3.Dot(verticalZiplineDirection, verticalVelocity);
            resultingVelocity += velocityDirection * (rb.linearVelocity.y * verticalSimilarity);
        }

        return resultingVelocity;
    }

    private void DropFromZipline(InputValue value)
    {
        if (value.isPressed) CompleteAction();
    }
}