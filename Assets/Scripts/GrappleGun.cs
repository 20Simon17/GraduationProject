using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]

// Massive credit to "Lewis Fiford" on youtube, he went through how to recreate the spiderman swing
// Video link: https://youtu.be/R52qmIler-E

public class GrappleGun : ItemBase
{
    //TODO: Pass through all FindObjectsByType variables into these scripts, avoid them as much as possible
    
    private PlayerActionStack player;
    private Rigidbody playerRb;
    private CameraActionStack playerCamera;
    private LineRenderer lineRenderer;
    private Vector3 lineRendererAttachPoint;
    
    private bool gameIsQuitting;
    private bool gameIsPaused;
    private bool eventsAreBound;
    private bool referencesAreSet;

    private bool IsActive => isSwinging || isPulling;
    [SerializeField] private bool isSwinging;
    [SerializeField] private bool isPulling;
    
    [SerializeField] private Vector3 attachPoint;
    
    [Header("General")]
    [SerializeField] private LayerMask grappleLayerMask;
    [SerializeField] private float grappleCooldown = 0.5f;
    [SerializeField] private float grappleRange;

    [Header("Pull Grapple")]
    [SerializeField] private int maxPullGrapples = 1;
    [SerializeField] private float pullForce = 20;
    [SerializeField] private float pullDetachDistance = 2;
    private int pullGrapples = 0;

    [Header("Swing Grapple")]
    [SerializeField] private int maxSwingGrapples = 1;
    [SerializeField] private float minSwingVelocity = 0;
    [SerializeField] private float maxSwingVelocity = 100;
    [SerializeField] private float swingForceDivision = 1;
    [SerializeField] private float forwardVelocityAddition = 20;
    private int swingGrapples = 0;

    [Header("Grapple Line")]
    [SerializeField] private GameObject gunTip;
    [SerializeField] private bool showTrueAttachPoint = false;

    [Header("Grapple Indicator")]
    [SerializeField] private Image grapplePointIcon;
    [SerializeField] private float predictionSphereCastRadius = 1f;
    private Transform predictionPoint;
    private RaycastHit predictionHit;
    private bool disableIcon = false;

    private Action OnGrappleFinished;

    private void GetReferences()
    {
        player = FindFirstObjectByType<PlayerActionStack>();
        playerRb = player.GetComponent<Rigidbody>();
        playerCamera = FindFirstObjectByType<CameraActionStack>();
        referencesAreSet = true;
        
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;

        predictionPoint = new GameObject("Grapple Prediction Point").transform;
    }

    public override void EquipItem()
    {
        base.EquipItem();
        if (!referencesAreSet) GetReferences();
        BindEvents();
        gameObject.SetActive(true);
    }

    public override void UnequipItem()
    {
        base.UnequipItem();
        UnbindEvents();
        gameObject.SetActive(false);
    }

    private void BindEvents()
    {
        if (eventsAreBound) return;
        eventsAreBound = true;
        
        Application.quitting += QuitGame;
        player.OnGroundedEvent += OnGrounded;
        OnGrappleFinished += DisableLineRenderer;

        InputManager.Instance.OnPrimaryActionEvent += PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent += SecondaryAction;

        GameManager.Instance.OnGamePausedEvent += Pause;
        GameManager.Instance.OnGameResumedEvent += Resume;
    }

    private void UnbindEvents()
    {
        Application.quitting -= QuitGame;
        if (gameIsQuitting) return;
        
        eventsAreBound = false;
        player.OnGroundedEvent -= OnGrounded;
        OnGrappleFinished -= DisableLineRenderer;

        InputManager.Instance.OnPrimaryActionEvent -= PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent -= SecondaryAction;
        
        GameManager.Instance.OnGamePausedEvent -= Pause;
        GameManager.Instance.OnGameResumedEvent -= Resume;
    }


    private void OnDisable() => UnbindEvents();
    private void QuitGame() => gameIsQuitting = true;
    private void Pause() => gameIsPaused = true;
    private void Resume() => gameIsPaused = false;
    private void DisableLineRenderer() => lineRenderer.enabled = false;

    private void OnGrounded()
    {
        swingGrapples = 0;
        pullGrapples = 0;

        if (isSwinging) HandleSwingGrapple(false);
    }
    
    private RaycastHit? GetLookAtHit()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, grappleRange, grappleLayerMask)) return hit;
        else return null;
    }

    private void FixedUpdate()
    {
        if (gameIsPaused) return;
        
        CheckForSwingPoints();

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

    private void CheckForSwingPoints()
    {
        if (isSwinging || isPulling) return;

        Physics.SphereCast(playerCamera.transform.position, predictionSphereCastRadius,
                            playerCamera.transform.forward, out RaycastHit sphereCastHit, grappleRange);
        
        Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit raycastHit, grappleRange);
        
        Vector3 realHitPoint;
        
        // Direct hit
        if(raycastHit.point != Vector3.zero)
        {
            realHitPoint = raycastHit.point;
        }
        
        // Predicted hit
        else if(sphereCastHit.point != Vector3.zero)
        {
            realHitPoint = sphereCastHit.point;
        }
        
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

    private void LateUpdate()
    {
        DrawGrappleLine();
        DrawGrappleIndicator();
    }

    private void DrawGrappleLine()
    {
        if (!isSwinging && !isPulling) return;

        lineRenderer.SetPosition(0, gunTip.transform.position);
        lineRenderer.SetPosition(1, showTrueAttachPoint ? attachPoint : lineRendererAttachPoint);
    }

    private void DrawGrappleIndicator()
    {
        if (disableIcon || isSwinging || isPulling || (pullGrapples >= maxPullGrapples && swingGrapples >= maxSwingGrapples))
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
            if (playerCamera.IsObjectVisible(predictionPoint))
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
            
            grapplePointIcon.transform.position = playerCamera.WorldToScreenPoint(predictionPoint.position);
            
            float lerpValue = distanceToCamera / grappleRange;
            float scaleValue = Mathf.Lerp(1f, 0.75f, lerpValue);
            grapplePointIcon.rectTransform.sizeDelta = new Vector2(64 * scaleValue, 64 * scaleValue);
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

        float verticalDot = Vector3.Dot(swingDirection.normalized, Vector3.up);
        if (verticalDot > 0.5f)
        {
            // normal swing
        }
        else if (verticalDot > -0.5f)
        {
            // make the swing go around the corner
        }
        else
        {
            //cancel swing? no swinging below player?
        }
       
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
        //TODO: if the player aims "roughly" horizontally in front of itself and the dot between the players velocity and the hit.point
        // is roughly equal to 0, then assume the player wants to grapple around a corner instead of swing.
        // to be clear, if the aim location is in the players velocity's path, then perform a swing.
        
        if (isStart && !IsActive && swingGrapples < maxSwingGrapples &&
            playerRb.linearVelocity.y < 0 && !player.dataRecord.isGrounded && !player.dataRecord.isInTimeTrial)
        {
            RaycastHit? checkHit = GetLookAtHit();
            RaycastHit hit;

            if (checkHit.HasValue) hit = checkHit.Value;
            else if (predictionHit.point != Vector3.zero) hit = predictionHit;
            else return;
            
            if (hit.point != attachPoint && hit.point != Vector3.zero && hit.normal != Vector3.up)
            {
                isSwinging = true;
                swingGrapples++;
                
                attachPoint = hit.point;
                lineRendererAttachPoint = hit.point;
                lineRenderer.enabled = true;
                
                Vector3 flatPlayerVelocity = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
                float normalDot = Vector3.Dot(-hit.normal, flatPlayerVelocity.normalized);
                if (hit.normal != Vector3.down && normalDot < 0.9f && normalDot > -0.9f && playerRb.linearVelocity.magnitude > 2)
                {
                    //TODO: For this to work correctly, need the prediction point / assist points to work.
                    //If the player didn't directly hit anything, check a larger area (spherecollider with grapplingrange radius?) and use the closest point?
                    if (hit.normal == Vector3.right || hit.normal == Vector3.left)
                    {
                        attachPoint.x = playerRb.transform.position.x;
                    }
                    else if (hit.normal == Vector3.forward || hit.normal == Vector3.back)
                    {
                        attachPoint.z = playerRb.transform.position.z;
                    }
                }
                
                player.AddWaitingAction(ref OnGrappleFinished);
            }
        }
        else if (isSwinging)
        {
            isSwinging = false;
            OnGrappleFinished?.Invoke();
        }
    }

    private void HandlePullGrapple(bool isStart)
    {
        if (isStart && !IsActive && pullGrapples < maxPullGrapples  && !player.dataRecord.isInTimeTrial)
        {
            RaycastHit? checkHit = GetLookAtHit();
            RaycastHit hit;

            if (checkHit.HasValue) hit = checkHit.Value;
            else if (predictionHit.point != Vector3.zero) hit = predictionHit;
            else return;

            if (hit.point != attachPoint && hit.point != Vector3.zero)
            {
                isPulling = true;
                pullGrapples++;
                
                attachPoint = hit.point;
                lineRendererAttachPoint = hit.point;
                lineRenderer.enabled = true;
                
                player.AddWaitingAction(ref OnGrappleFinished);
            }
        }
        else if (isPulling)
        {
            isPulling = false;
            OnGrappleFinished?.Invoke();
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