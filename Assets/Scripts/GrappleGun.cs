using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GrappleGun : MonoBehaviour
{
    private PlayerActionStack player;
    private CameraActionStack playerCamera;
    private Camera playerCameraComponent;
    private bool disableIcon = false;

    private RaycastHit predictionHit;
    [SerializeField] private Transform predictionPoint;
    
    private Vector3 currentGrapplePosition;

    private Vector3 swingPoint;
    private SpringJoint joint;
    
    public bool isEquipped = false;

    [Header("References")]
    [SerializeField] private Image grapplePointIcon;
    [SerializeField] private float minIconScale = 0.75f;
    [SerializeField] private Transform gunTip;
    
    [Header("General Settings")]
    [SerializeField] private LineRenderer grappleLineRenderer;
    [SerializeField] private float grappleRange = 50f;
    [SerializeField] private float predictionSphereCastRadius = 1f;
    [SerializeField] private bool swapPrimarySecondary = false; //TODO: Add keybind to toggle this setting in-game + visual for what keybind does what

    [Header("Pull Grapple Settings")] 
    [SerializeField] private float pullStrength = 100f;
    
    [Header("Swing Grapple Settings")]
    [SerializeField] private float springJointSpring = 4.5f;
    [SerializeField] private float springJointDamper = 7f;
    [SerializeField] private float springJointMassScale = 4.5f;
    [SerializeField] private float springJointMinDistance = 0.25f;
    [SerializeField] private float springJointMaxDistance = 0.8f;

    private void Awake()
    {
        player = FindFirstObjectByType<PlayerActionStack>();
        playerCamera = FindFirstObjectByType<CameraActionStack>();
        grappleLineRenderer = GetComponent<LineRenderer>();
        playerCameraComponent = playerCamera.GetComponent<Camera>();
    }

    private void Start()
    {
        InputManager.Instance.OnPrimaryActionEvent += OnPrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent += OnSecondaryAction;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnPrimaryActionEvent -= OnPrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent -= OnSecondaryAction;
    }

    private void PrimaryAction(bool startAction)
    {
        if (startAction) StartSwingGrapple();
        else StopSwingGrapple();
    }

    private void SecondaryAction()
    {
        PullGrapple();
    }
    
    private void OnPrimaryAction(InputValue value)
    {
        if (!isEquipped) return;

        if (value.isPressed)
        {
            if (!swapPrimarySecondary)
            {
                PrimaryAction(true);
                return;
            }
            SecondaryAction();
        }
        else if (!swapPrimarySecondary)
        {
            PrimaryAction(false);
        }
    }
    
    private void OnSecondaryAction(InputValue value)
    {
        if(!isEquipped) return;

        if (value.isPressed)
        {
            if (!swapPrimarySecondary)
            {
                SecondaryAction();
                return;
            }
            PrimaryAction(true);
        }
        else if (swapPrimarySecondary)
        {
            PrimaryAction(false);
        }
    }

    private void Update()
    {
        CheckForSwingPoints();
    }

    private void LateUpdate()
    {
        DrawRope();
        DrawIndicator();
    }

    private void DrawRope()
    {
        if (!joint) return;
        
        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);
        
        grappleLineRenderer.SetPosition(0, gunTip.position);
        grappleLineRenderer.SetPosition(1, currentGrapplePosition);
    }

    private void DrawIndicator()
    {
        if (disableIcon)
        {
            grapplePointIcon.enabled = false;
            return;
        }
        
        float distanceToCamera = Vector3.Distance(predictionPoint.position, playerCamera.transform.position);
        
        if (predictionPoint.position == Vector3.zero || distanceToCamera > grappleRange)
        {
            if (grapplePointIcon.enabled)
            {
                grapplePointIcon.enabled = false;
            }
        }
        else
        {
            if (IsObjectVisible(predictionPoint))
            {
                if (!grapplePointIcon.enabled)
                {
                    grapplePointIcon.enabled = true;
                }
            }
            else if (grapplePointIcon.enabled)
            {
                grapplePointIcon.enabled = false;
            }
            
            grapplePointIcon.transform.position = playerCameraComponent.WorldToScreenPoint(predictionPoint.position);
            
            float lerpValue = distanceToCamera / grappleRange;
            float scaleValue = Mathf.Lerp(1f, minIconScale, lerpValue);
            grapplePointIcon.rectTransform.sizeDelta = new Vector2(64 * scaleValue, 64 * scaleValue);
        }
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
            //predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
            disableIcon = false;
        }
        else
        {
            //predictionPoint.gameObject.SetActive(false);
            disableIcon = true;
        }
        
        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }
    
    private void StartSwingGrapple()
    {
        if (predictionHit.point == Vector3.zero) return;
        
        swingPoint = predictionHit.point;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;
            
        float distanceFromPoint = Vector3.Distance(player.transform.position, swingPoint);
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
            Vector3 directionToGrapplePoint = (grapplePoint - player.transform.position).normalized;
            float distanceToGrapplePoint = Vector3.Distance(player.transform.position, grapplePoint);
            
            // Apply a force towards the grapple point
            Vector3 pullForce = directionToGrapplePoint * pullStrength * 100;
            player.GetComponent<Rigidbody>().AddForce(pullForce, ForceMode.Force);
        }
    }
    
    private bool IsObjectVisible(Transform targetObject)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCameraComponent);

        foreach (var plane in planes)
        {
            if (plane.GetDistanceToPoint(targetObject.position) < 0f)
            {
                return false;
            }
        }

        return true;
    }
}