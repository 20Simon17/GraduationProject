using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class CharacterMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 movementVector;
    
    [SerializeField]
    private float maxVelocity = 10f;
    
    [SerializeField]
    private float accelerationForce = 10f;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    private void FixedUpdate()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        if (movementVector == Vector2.zero)
        {
            
        }
        else if (rb.linearVelocity.magnitude < maxVelocity)
        {
            rb.AddForce(transform.forward * (movementVector.y * accelerationForce));
            rb.AddForce(transform.right * (movementVector.x * accelerationForce));
        }
    }
    
    private void OnMove(InputValue value)
    {
        movementVector = value.Get<Vector2>();
    }
}