using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class CharacterMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 movementVector;
    private Vector3 defaultGravity;
    private float defaultPlayerScaleY;
    
    private float jumpForceScaling = 100f;
    
    [Header("Debugging - Values")] 
    [SerializeField] private float timeAtLastSlide;
    [SerializeField] private float timeAtLastSlam;
    [SerializeField] private float currentSpeed;
    
    //TODO: convert all of these to properties and set "disableMovement" to true in the setters?
    [Header("Debugging - Bools")] 
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool canJump;
    [SerializeField] private bool isSliding;
    [SerializeField] private bool isSlamming;

    private bool CanJump
    {
        get => isGrounded || canJump;
        set => canJump = value;
    }
    
    [Header("Ground")]
    [SerializeField] private float counterForceSpeedThreshold = 3.0f;
    [SerializeField] private float defaultFriction = 0.6f;
    
    /*[Header("Air")]
    [SerializeField] private float airGravityMultiplier = 1.0f;*/

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7.5f;
    [SerializeField] private float slamJumpTimeFrame = 0.2f;
    [SerializeField] private float slideJumpTimeFrame = 0.2f;
    
    [Header("Slide")]
    [SerializeField] private float slideSpeedBoost = 1.2f;
    [SerializeField] private float slideSpeed = 9.0f;
    [SerializeField] private float slideJumpForceMultiplier = 0.85f;
    [SerializeField] private float slideJumpSpeedMultiplier = 1.15f;
    [SerializeField] private float slideFriction = 0.05f;
    [SerializeField] private float slidePlayerScaleY = 0.5f;

    [Header("Ground Slam")]
    [SerializeField] private float groundSlamForce = 10f;
    [SerializeField] private float slamJumpForceMultiplier = 1.3f;
    [SerializeField] private float groundSlamGravityMultiplier = 3.0f;
    
    [Header("Movement")]
    [SerializeField] private PhysicsMaterial physicsMaterial;
    [SerializeField] private float maxVelocity = 10f;
    [SerializeField] private float accelerationForce = 50f;
    [SerializeField] private float decelerationForce = 50f;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        defaultGravity = Physics.gravity;
        defaultPlayerScaleY = transform.localScale.y;
    }
    
    private void FixedUpdate()
    {
        if (!isSliding) UpdateMovement();
        UpdateGravity();

        currentSpeed = rb.linearVelocity.magnitude;
        if (isGrounded && isSlamming)
        {
            isSlamming = false;
            timeAtLastSlam = Time.time;
        }
    }
    
    private void UpdateMovement()
    {
        if (movementVector == Vector2.zero && rb.linearVelocity.magnitude > counterForceSpeedThreshold && isGrounded)
        {
            Vector3 counterForce = new Vector3(-rb.linearVelocity.x, 0, -rb.linearVelocity.z).normalized;
            rb.AddForce(counterForce * decelerationForce);
        }
        else if (rb.linearVelocity.magnitude < maxVelocity)
        {
            rb.AddForce(transform.forward * (movementVector.y * accelerationForce));
            rb.AddForce(transform.right * (movementVector.x * accelerationForce));
        }
    }

    private void UpdateGravity()
    {
        if (!isGrounded && isSlamming)
        {
            Physics.gravity = defaultGravity * groundSlamGravityMultiplier;
        }
        else
        {
            Physics.gravity = defaultGravity;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            isSlamming = false;
            timeAtLastSlam = Time.time;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
    
    private void OnMove(InputValue value)
    {
        movementVector = value.Get<Vector2>();
    }
    
    private void OnJump(InputValue value) //should I combine both slide and slam jump into 1 with reduced effect
    {
        if (value.isPressed && CanJump)
        {
            float now = Time.time;
            if (timeAtLastSlide != 0 && timeAtLastSlide < timeAtLastSlam)   
            {
                if (now - timeAtLastSlide <= slideJumpTimeFrame)
                {
                    timeAtLastSlide = 0;
                    
                    rb.AddForce(transform.forward * slideJumpSpeedMultiplier, ForceMode.Force); //Little speed boost when jumping from slide
                    rb.AddForce(transform.up * jumpForce * slideJumpForceMultiplier * jumpForceScaling, ForceMode.Force); //Weaker jump when sliding
                    CanJump = false;
                    return;
                }
            }
            else
            {
                if (now - timeAtLastSlam <= slamJumpTimeFrame && timeAtLastSlam != 0)
                {
                    timeAtLastSlam = 0;
                    
                    rb.AddForce(transform.up * jumpForce * slamJumpForceMultiplier * jumpForceScaling, ForceMode.Force); //Higher jump when jumping from slam
                    CanJump = false;
                    return;
                }
            }
            
            float slideJumpMultiplier = isSliding ? slideJumpForceMultiplier : 1f;
            rb.AddForce(transform.up * jumpForce * jumpForceScaling * slideJumpMultiplier, ForceMode.Force);
            CanJump = false;
        }
    }

    private void OnCrouch(InputValue value)  //TODO: slide towards players forward or based on movement input?
    {
        //TODO: probably need to implement a slide cooldown to prevent spamming and infinite speed
        if (value.isPressed)
        {
            isSliding = true;
            physicsMaterial.dynamicFriction = slideFriction;
            transform.localScale = new Vector3(transform.localScale.x, slidePlayerScaleY, transform.localScale.z);

            if (!isGrounded) return;
            
            rb.AddForce(-transform.up * 100, ForceMode.Impulse); //Send the player downwards to stick to the ground
            timeAtLastSlide = Time.time;
            
            if (rb.linearVelocity.magnitude < maxVelocity)
            {
                rb.linearVelocity = transform.forward * slideSpeed;
            }
            else
            {
                rb.linearVelocity = transform.forward * rb.linearVelocity.magnitude * slideSpeedBoost;
            }
        }
        else
        {
            isSliding = false;
            physicsMaterial.dynamicFriction = defaultFriction;
            transform.localScale = new Vector3(transform.localScale.x, defaultPlayerScaleY, transform.localScale.z);
        }
    }
    
    private void OnSlam(InputValue value)
    {
        if (value.isPressed && !isSlamming)
        {
            //TODO: check why player loses speed upon landing after a ground slam
            isSlamming = true;
            rb.AddForce(-transform.up * groundSlamForce * 100, ForceMode.Force);
        }
    }
    
    private void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            CanJump = true;
            
            //TODO: Interaction stuff
            //probably make it be "look at interactable object" rather than proximity based
            //if nearby zipline, attach to it
            //if nearby door, open it
            //if nearby item, pick it up
        }
    }
    
    
    //TODO: NOTES
    //Cancel action keybind - shift ? -> cancel zipline riding, cancel wallrun without jumping
    //Coyote time probably
    //Zipline speed gain/loss based on DotProduct of the angle between the players down and the zipline direction * gravity? (sounds correct)
    //when pull grappling, if the player is grounded && pull point is below player, apply the force to the players forward instead of towards the pull point.
}