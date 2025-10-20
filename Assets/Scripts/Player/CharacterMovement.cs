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
    
    //TODO: convert all of these to properties and set "disableMovement" to true in the setters?
    [Header("Debugging")] 
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool canJump;
    [SerializeField] private bool isSliding;

    private bool CanJump
    {
        get => isGrounded || canJump;
        set => canJump = value;
    }
    
    [Header("Ground")]
    [SerializeField] private float counterForceThreshold = 3.0f;
    
    [Header("Air")]
    [SerializeField] private float airGravityMultiplier = 1.0f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7.5f;
    
    [Header("Slide")]
    [SerializeField] private float slideSpeedBoost = 1.2f;
    [SerializeField] private float slideSpeed = 9.0f;

    [Header("Ground Slam")]
    [SerializeField] private float groundSlamForce = 10f;
    
    [Header("Movement")]
    [SerializeField] private float maxVelocity = 10f;
    [SerializeField] private float accelerationForce = 50f;
    [SerializeField] private float decelerationForce = 50f;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        defaultGravity = Physics.gravity;
    }
    
    private void FixedUpdate()
    {
        GroundCheck();
        if (!isSliding) UpdateMovement();
        UpdateGravity();
    }
    
    private void UpdateMovement()
    {
        if (movementVector == Vector2.zero && rb.linearVelocity.magnitude > counterForceThreshold)
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

    private void GroundCheck()
    {
        Ray groundRay = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(groundRay, out RaycastHit hitInfo, 1.1f))
        {
            if (hitInfo.collider.CompareTag("Ground"))
            {
                isGrounded = true;
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    private void UpdateGravity()
    {
        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            Physics.gravity = defaultGravity * airGravityMultiplier;
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
    
    private void OnJump(InputValue value)
    {
        if (value.isPressed && CanJump)
        {
            rb.AddForce(transform.up * jumpForce * 100, ForceMode.Force);
            CanJump = false;
        }
    }

    private void OnCrouch(InputValue value)  //TODO: slide towards players forward or based on movement input?
    {
        //TODO: probably need to implement a slide cooldown to prevent spamming and infinite speed
        if (value.isPressed)
        {
            isSliding = true;
            //add physics material with ~0.05 friction to player collider - or potentially edit the properties of the existing physics material

            if (!isGrounded) return;
            
            if (rb.linearVelocity.magnitude < slideSpeed)
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
            //remove physics material from player collider
        }
    }
    
    private void OnSlam(InputValue value)
    {
        if (value.isPressed)
        {
            rb.AddForce(-transform.up * groundSlamForce * 100, ForceMode.Force);
        }
    }
    
    private void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            CanJump = true;
        }
    }
}