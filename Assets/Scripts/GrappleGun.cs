using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrappleGun : MonoBehaviour
{
    private CharacterMovement characterMovement;
    private PlayerCamera playerCamera;

    private RaycastHit predictionHit;
    [SerializeField] private Transform predictionPoint;
    
    private Vector3 currentGrapplePosition;

    private Vector3 swingPoint;
    private SpringJoint joint;
    
    public bool isEquipped = false;
    
    [Header("References")]
    [SerializeField] private Transform gunTip;
    
    [Header("General Settings")]
    [SerializeField] private LineRenderer grappleLineRenderer;
    [SerializeField] private float grappleRange = 50f;
    [SerializeField] private float predictionSphereCastRadius = 1f;

    [Header("Pull Grapple Settings")] 
    [SerializeField] private float pullStrength = 100f;
    
    [Header("Swing Grapple Settings")]
    [SerializeField] private float springJointSpring = 4.5f;
    [SerializeField] private float springJointDamper = 7f;
    [SerializeField] private float springJointMassScale = 4.5f;
    [SerializeField] private float springJointMinDistance = 0.25f;
    [SerializeField] private float springJointMaxDistance = 0.8f;
    
    [SerializeField] private bool swapPrimarySecondary = false; //TODO: Add keybind to toggle this setting in-game + visual for what keybind does what

    private void Awake()
    {
        characterMovement = FindFirstObjectByType<CharacterMovement>();
        playerCamera = FindFirstObjectByType<PlayerCamera>();
        grappleLineRenderer = GetComponent<LineRenderer>();
    }

    private void PrimaryAction()
    {
        if (swapPrimarySecondary)
        {
            PullGrapple(); //Swapped primary action
        }
        else
        {
            StartSwingGrapple(); //True primary action
        }
    }

    private void SecondaryAction()
    {
        if (swapPrimarySecondary)
        {
            StartSwingGrapple(); //Swapped secondary action
        }
        else
        {
            PullGrapple(); //True secondary action
        }
    }
    
    private void OnPrimaryAction(InputValue value)
    {
        if (isEquipped && value.isPressed)
        {
            PrimaryAction();
        }
        else StopSwingGrapple();
    }
    
    private void OnSecondaryAction(InputValue value)
    {
        if (isEquipped && value.isPressed)
        {
            SecondaryAction();
        }
    }

    private void Update()
    {
        CheckForSwingPoints();
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void DrawRope()
    {
        if (!joint) return;
        
        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);
        
        grappleLineRenderer.SetPosition(0, gunTip.position);
        grappleLineRenderer.SetPosition(1, currentGrapplePosition);
    }

    private void CheckForSwingPoints()
    {
        if (joint) return;

        RaycastHit sphereCastHit;
        Physics.SphereCast(playerCamera.transform.position, predictionSphereCastRadius,
                            playerCamera.transform.forward, out sphereCastHit, grappleRange);

        RaycastHit raycastHit;
        Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, 
                            out raycastHit, grappleRange);

        Vector3 realHitPoint;
        
        // Direct hit
        if(raycastHit.point != Vector3.zero)
            realHitPoint = raycastHit.point;
        
        // Predicted hit
        else if(sphereCastHit.point != Vector3.zero)
            realHitPoint = sphereCastHit.point;
        
        // No hit
        else realHitPoint = Vector3.zero;

        if (realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }
        
        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }
    
    private void StartSwingGrapple()
    {
        if (predictionHit.point == Vector3.zero) return;
        
        swingPoint = predictionHit.point;
        joint = characterMovement.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;
            
        float distanceFromPoint = Vector3.Distance(characterMovement.transform.position, swingPoint);
        joint.maxDistance = distanceFromPoint * springJointMaxDistance;
        joint.minDistance = distanceFromPoint * springJointMinDistance;
            
        joint.spring = springJointSpring;
        joint.damper = springJointDamper;
        joint.massScale = springJointMassScale;
            
        grappleLineRenderer.positionCount = 2; // Set line renderer to have 2 points
        currentGrapplePosition = gunTip.position; // Initialize current grapple position
    }

    private void StopSwingGrapple()
    {
        grappleLineRenderer.positionCount = 0; // Clear the line renderer
        Destroy(joint);
    }
    
    private void PullGrapple()
    {
        Ray cameraRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(cameraRay, out RaycastHit hitInfo, grappleRange))
        {
            Vector3 grapplePoint = hitInfo.point;
            Vector3 directionToGrapplePoint = (grapplePoint - characterMovement.transform.position).normalized;
            float distanceToGrapplePoint = Vector3.Distance(characterMovement.transform.position, grapplePoint);
            
            // Apply a force towards the grapple point
            Vector3 pullForce = directionToGrapplePoint * pullStrength * 100;
            characterMovement.GetComponent<Rigidbody>().AddForce(pullForce, ForceMode.Force);
        }
    }
}