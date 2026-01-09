using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [HideInInspector] public Rigidbody rb;
    
    [SerializeField] private PlayerDataSO playerDataObject;
    private PlayerData _playerData;
    
    private readonly Dictionary<Type, BaseState> _states = new();
    
    private BaseState _currentState;
    
    private TMP_Text _horizontalVelocityText;
    private TMP_Text _forwardVelocityText;
    private TMP_Text _strafeVelocityText;

    private bool _isDisabled;
    
    /*#region InputPassThrough
    private void OnMove(InputValue value) => _currentState?.OnMove(value);
    private void OnJump(InputValue value) => _currentState?.OnJump(value);
    private void OnCrouch(InputValue value) => _currentState?.OnCrouch(value);
    private void OnSlam(InputValue value) => _currentState?.OnSlam(value);
    private void OnInteract(InputValue value) => _currentState?.OnInteract(value);
    private void OnPrimaryAction(InputValue value) => _currentState?.OnPrimaryAction(value);
    private void OnSecondaryAction(InputValue value) => _currentState?.OnSecondaryAction(value);
    #endregion*/

    #region EventBinding
    private void BindEvents()
    {
        InputManager inputManager = InputManager.TryGetInstance();
        if (inputManager is null) return;
        
        inputManager.OnJumpEvent               += _currentState.OnJump;
        inputManager.OnCrouchEvent             += _currentState.OnCrouch;
        inputManager.OnSlamEvent               += _currentState.OnSlam;
        //inputManager.OnInteractEvent           += _currentState.OnInteract;
        inputManager.OnInteractEvent           += Interact;
        inputManager.OnPrimaryActionEvent      += _currentState.OnPrimaryAction;
        inputManager.OnSecondaryActionEvent    += _currentState.OnSecondaryAction;
    }

    private void UnbindEvents()
    {
        InputManager inputManager = InputManager.TryGetInstance();
        if (inputManager is null) return;
        
        inputManager.OnJumpEvent               -= _currentState.OnJump;
        inputManager.OnCrouchEvent             -= _currentState.OnCrouch;
        inputManager.OnSlamEvent               -= _currentState.OnSlam;
        //inputManager.OnInteractEvent           -= _currentState.OnInteract;
        inputManager.OnInteractEvent           -= Interact;
        inputManager.OnPrimaryActionEvent      -= _currentState.OnPrimaryAction;
        inputManager.OnSecondaryActionEvent    -= _currentState.OnSecondaryAction;
    }
    #endregion EventBinding
    
    #region StateTransitions & Input PassThrough

    /*private void OnJump(InputValue value)
    {
        //check for wallrunning or walljumping first
    }*/
    #endregion StateTransitions & Input PassThrough

    #region PlayerEnabling
    private void EnablePlayer()
    {
        _isDisabled = false;
        SwitchState<DefaultState>();
        
        BindEvents();
        
        gameObject.SetActive(true);
    }
    
    private void DisablePlayer()
    {
        UnbindEvents();
        
        _isDisabled = true;
        // set state to an idle type state
        
        gameObject.SetActive(false);
    }

    private void Start() => EnablePlayer();
    private void OnDisable() => DisablePlayer();
    #endregion PlayerEnabling

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _playerData = playerDataObject.playerData;
        _playerData.defaultPlayerScaleY = transform.localScale.y;

        _horizontalVelocityText = GameObject.Find("HorizontalVelocity Text").GetComponent<TMP_Text>();
        _forwardVelocityText = GameObject.Find("ForwardVelocity Text").GetComponent<TMP_Text>();
        _strafeVelocityText = GameObject.Find("StrafeVelocity Text").GetComponent<TMP_Text>();

        EnablePlayer();
    }

    public void SwitchState<T>() where T : BaseState, new()
    {
        if (_currentState?.GetType() == typeof(T) || _isDisabled) return;

        _currentState?.DisableState();
        if (_currentState != null) UnbindEvents();

        if(_states.ContainsKey(typeof(T)))
        {
            _currentState = _states[typeof(T)];
        }
        else
        {
            T newState = new T();
            _states.Add(typeof(T), newState);
            _currentState = newState;
        }
        
        BindEvents();

        _currentState?.EnableState(this, playerDataObject.playerData);
    }

    private void FixedUpdate()
    {
        if (_isDisabled) return;

        GroundCheck();
        CheckVelocityCap();

        _currentState?.UpdateState(Time.fixedDeltaTime);

        Vector2 horizontalVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
        _horizontalVelocityText.text = "Horizontal Velocity: " + horizontalVelocity.magnitude;
        _forwardVelocityText.text = "Forward Velocity: " + _playerData.forwardVelocity;
        _strafeVelocityText.text = "Strafe Velocity: " + _playerData.strafeVelocity;
    }

    private void GroundCheck()
    {
        //make a better ground check

        //spherecast downwards at player y 0.4 with radius 0.5 to have the player size but placed a bit under it
    }

    private void Interact(InputValue value)
    {
        Ray interactionRay = new Ray(transform.position, transform.forward);
        Physics.Raycast(interactionRay, out RaycastHit hit, 50f);

        if (hit.collider is null) return;

        //ZiplinePoint zipPoint = hit.transform.GetComponentInParent<ZiplinePoint>();
        // TODO: when replacing the zipline point meshes with actual models, remove .parent here (hard coded due to currently using multiple objects to make a zipline point)
        if (hit.transform.parent.TryGetComponent(out ZiplinePoint zipPoint))
        {
            Debug.Log("Found a zipline point");
            SwitchState<ZiplineState>();
            zipPoint.Owner.AttachPlayer(this);
        }
        else if (hit.transform.TryGetComponent(out Zipline zipline))
        {
            Debug.Log("Found a zipline");
            SwitchState<ZiplineState>();
            zipline.AttachPlayer(this);
        }
        else
        {
            Debug.Log("Found " + hit.transform.name);
        }
    }

    #region CollisionHandling
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _playerData.isGrounded = true;
            _playerData.isSlamming = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _playerData.isGrounded = false;
        }
    }
    #endregion CollisionHandling

    private void CheckVelocityCap()
    {
        if(rb.linearVelocity.magnitude > _playerData.velocityHardCap)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * _playerData.velocityHardCap;
        }
    }
}