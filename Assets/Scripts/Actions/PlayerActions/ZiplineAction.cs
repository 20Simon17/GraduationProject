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
    private Vector3 previousVelocityDirection;

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
        rb.linearVelocity = CalculateEntryVelocity();

        // calculate acceleration on the zipline based on the vertical angle of it
        if (ziplineDirection == Vector3.zero)
        {
            ziplineAngleAcceleration = flatZiplineDeacceleration;
        }
        else
        {
            float vDot = Vector3.Dot(ziplineDirection, Vector3.down);
            float hDot = Vector3.Dot(velocityDirection, ziplineDirection);
            ziplineAngleAcceleration = data.defaultGravity.magnitude * vDot * (hDot > 0 ? 1 : -1);
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
        rb.linearVelocity += velocityDirection * (ziplineAngleAcceleration * deltaTime);
        
        if (ziplineDirection == Vector3.zero && rb.linearVelocity.magnitude <= data.ziplineAutoDropVelocity)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private Vector3 CalculateEntryVelocity()
    {
        Vector3 zipDir = attachedZipline.GetZiplineDirectionNonZero();
        
        Vector3 flatZipDir = new Vector3(zipDir.x, 0, zipDir.z);
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        int dir = Vector3.Dot(flatZipDir, flatVelocity) > 0 ? 1 : -1;
        velocityDirection = dir > 0 ? zipDir : -zipDir;
        
        float keptVelocity = Vector3.Dot(zipDir, rb.linearVelocity.normalized);
        if (dir == -1) return zipDir * (rb.linearVelocity.magnitude * keptVelocity);
        else return zipDir * (rb.linearVelocity.magnitude * (0.5f + 0.5f * keptVelocity));
    }

    private void DropFromZipline(InputValue value)
    {
        if (value.isPressed) CompleteAction();
    }
}