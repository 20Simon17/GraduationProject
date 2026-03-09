using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrappleGunRefactor : MonoBehaviour
{
    private PlayerActionStack player;
    private CameraActionStack playerCamera;

    private Transform predictionPoint;
    
    private bool gameIsQuitting;

    private void Start()
    {
        BindEvents();
    }

    private void BindEvents()
    {
        Application.quitting += QuitGame;
        InputManager.Instance.OnPrimaryActionEvent += PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent += SecondaryAction;
    }

    private void OnDisable()
    {
        Application.quitting -= QuitGame;
        if (gameIsQuitting) return;
        
        InputManager.Instance.OnPrimaryActionEvent -= PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent -= SecondaryAction;
    }
    
    private void QuitGame() => gameIsQuitting = true;

    private void HandleSwingGrapple()
    {
        
    }

    private void HandlePullGrapple()
    {
        
    }
    
    private void PrimaryAction(InputValue value)
    {
        
    }

    private void SecondaryAction(InputValue value)
    {
        
    }
}
