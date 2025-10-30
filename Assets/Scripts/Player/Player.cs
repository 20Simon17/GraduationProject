using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [HideInInspector] public Rigidbody rb;
    
    [SerializeField] private PlayerDataSO playerDataObject;
    private PlayerData _playerData;
    
    private readonly Dictionary<Type, BaseState> _states = new();
    
    private BaseState _currentState;
    
    #region InputPassThrough
    private void OnMove(InputValue value) => _currentState?.OnMove(value);
    private void OnJump(InputValue value) => _currentState?.OnJump(value);
    private void OnCrouch(InputValue value) => _currentState?.OnCrouch(value);
    private void OnSlam(InputValue value) => _currentState?.OnSlam(value);
    private void OnInteract(InputValue value) => _currentState?.OnInteract(value);
    private void OnPrimaryAction(InputValue value) => _currentState?.OnPrimaryAction(value);
    private void OnSecondaryAction(InputValue value) => _currentState?.OnSecondaryAction(value);
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _playerData = playerDataObject.playerData;
        _playerData.defaultPlayerScaleY = transform.localScale.y;
    }

    private void Start()
    {
        SwitchState<DefaultState>();
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
        GroundCheck();
        CheckVelocityCap();
        
        _currentState?.UpdateState(Time.fixedDeltaTime);
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