using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Massive credit to "Lewis Fiford" on youtube, he went through how to recreate the spiderman swing
// Video link: https://youtu.be/R52qmIler-E

public class GrappleGunRefactor : MonoBehaviour
{
    private PlayerActionStack player;
    private Rigidbody playerRb;
    private CameraActionStack playerCamera;

    private Transform predictionPoint;
    
    private bool gameIsQuitting;

    private bool IsActive => isSwinging || isPulling;
    [SerializeField] private bool isSwinging;
    [SerializeField] private bool isPulling;

    [SerializeField] private LayerMask grappleLayerMask;

    [SerializeField] private float pullForce = 20;
    [SerializeField] private float pullDetachDistance = 2;

    [SerializeField] private Vector3 attachPoint;

    [SerializeField] private float grappleRange;

    [SerializeField] private float grappleCooldown = 0.5f;

    [SerializeField] private int maxSwingGrapples = 1;
    [SerializeField] private int maxPullGrapples = 1;

    private int swingGrapples = 0;
    private int pullGrapples = 0;

    [SerializeField] private float minSwingVelocity = 0;
    [SerializeField] private float maxSwingVelocity = 100;
    [SerializeField] private float swingForceDivision = 1;
    [SerializeField] private float forwardVelocityAddition = 20;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerActionStack>();
        playerRb = player.GetComponent<Rigidbody>();
        playerCamera = FindFirstObjectByType<CameraActionStack>();
        
        BindEvents();
    }

    private void BindEvents()
    {
        Application.quitting += QuitGame;
        player.OnGroundedEvent += RefreshGrapples;
        InputManager.Instance.OnPrimaryActionEvent += PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent += SecondaryAction;
    }

    private void OnDisable()
    {
        Application.quitting -= QuitGame;
        if (gameIsQuitting) return;
        
        player.OnGroundedEvent -= RefreshGrapples;
        InputManager.Instance.OnPrimaryActionEvent -= PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent -= SecondaryAction;
    }
    
    private void QuitGame() => gameIsQuitting = true;

    private void RefreshGrapples()
    {
        swingGrapples = 0;
        pullGrapples = 0;
    }
    
    private RaycastHit? GetLookAtHit()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, grappleRange, grappleLayerMask)) return hit;
        else return null;
    }

    private void FixedUpdate()
    {
        if (isPulling)
        {
            Vector3 direction = (attachPoint - player.transform.position).normalized;
            playerRb.linearVelocity = direction * pullForce;

            if (Vector3.Distance(attachPoint, player.transform.position) < pullDetachDistance)
            {
                HandlePullGrapple(false);
            }
        }
        else if (isSwinging)
        {
            Vector3 swingArcForce = CalculateSwingArcForce(playerRb.linearVelocity, player.transform.position, attachPoint);
            Vector3 extraForwardForce = player.transform.forward.normalized * forwardVelocityAddition;
            Vector3 totalForce = swingArcForce + extraForwardForce;
            
            playerRb.AddForce(totalForce, ForceMode.Acceleration);
        }
    }

    private Vector3 CalculateSwingArcForce(Vector3 inVelocity, Vector3 inPosition, Vector3 inAttachmentPoint)
    {
        if (swingForceDivision == 0)
        {
            Debug.LogError("SwingForceDivision is 0. Division by zero is not allowed, exiting.");
            HandleSwingGrapple(false);
            return Vector3.zero;
        }
        
        Vector3 swingDirection = inPosition - inAttachmentPoint;
       
        Vector3 clampedVelocity = inVelocity;
        if (inVelocity.magnitude < minSwingVelocity)
        {
            clampedVelocity = inVelocity.normalized * minSwingVelocity;
        }
        else if (inVelocity.magnitude > maxSwingVelocity)
        {
            clampedVelocity = inVelocity.normalized * maxSwingVelocity;
        }
            
        float dot = Vector3.Dot(clampedVelocity, swingDirection);
        Vector3 swingArcForce = swingDirection.normalized * (dot * -2) / swingForceDivision;
        return swingArcForce;
    }
    
    private void HandleSwingGrapple(bool isStart)
    {
        if (isStart && !IsActive && swingGrapples < maxSwingGrapples &&
            playerRb.linearVelocity.y < 0 && !player.dataRecord.isGrounded)
        {
            RaycastHit? checkHit = GetLookAtHit();
            if (!checkHit.HasValue) return;
            
            RaycastHit hit = checkHit.Value;
            if (hit.point != attachPoint && hit.point != Vector3.zero)
            {
                isSwinging = true;
                swingGrapples++;
                
                attachPoint = hit.point;
                
                player.AddWaitingAction();
            }
            return;
        }
        else
        {
            isSwinging = false;
            player.CompleteCurrentAction();
        }
        // continuously add force towards the point in some way to make it feel like a swing?
    }

    private void HandlePullGrapple(bool isStart)
    {
        if (isStart && !IsActive && pullGrapples < maxPullGrapples)
        {
            isPulling = true;
            pullGrapples++;
            
            RaycastHit? checkHit = GetLookAtHit();
            if (!checkHit.HasValue) return;
            
            RaycastHit hit = checkHit.Value;
            if (hit.point != attachPoint && hit.point != Vector3.zero)
            {
                isPulling = true;
                pullGrapples++;
                
                attachPoint = hit.point;
                
                player.AddWaitingAction();
            }
            return;
        }
        else
        {
            isPulling = false;
            player.CompleteCurrentAction();
        }
    }
    
    private void PrimaryAction(InputValue value)
    {
        HandleSwingGrapple(value.isPressed);
    }

    private void SecondaryAction(InputValue value)
    {
        HandlePullGrapple(value.isPressed);
    }
}
