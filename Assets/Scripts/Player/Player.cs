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
    
    //[SerializeField] private InputManager inputManager;

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
        inputManager.OnInteractEvent           += _currentState.OnInteract;
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
        inputManager.OnInteractEvent           -= _currentState.OnInteract;
        inputManager.OnPrimaryActionEvent      -= _currentState.OnPrimaryAction;
        inputManager.OnSecondaryActionEvent    -= _currentState.OnSecondaryAction;
    }
    #endregion

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
        if (_currentState?.GetType() == typeof(T)) return;
        
        _currentState?.DisableState();
        
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
    }
    
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

    private void CheckVelocityCap()
    {
        if(rb.linearVelocity.magnitude > _playerData.velocityHardCap)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * _playerData.velocityHardCap;
        }
    }
}