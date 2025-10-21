using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrappleGun : MonoBehaviour
{
    private CharacterMovement characterMovement;
    private PlayerCamera playerCamera;
    
    public bool isEquipped = false;
    
    [Header("General Settings")]
    [SerializeField] private float grappleRange = 50f;

    [Header("Pull Grapple Settings")] 
    [SerializeField] private float pullStrength = 100f;
    
    [SerializeField] private bool swapPrimarySecondary = false; //TODO: Add keybind to toggle this setting in-game + visual for what keybind does what

    private void Awake()
    {
        characterMovement = FindFirstObjectByType<CharacterMovement>();
        playerCamera = FindFirstObjectByType<PlayerCamera>();
    }

    private void PrimaryAction()
    {
        if (swapPrimarySecondary)
        {
            PullGrapple(); //Swapped primary action
        }
        else
        {
            SwingGrapple(); //True primary action
        }
    }

    private void SecondaryAction()
    {
        if (swapPrimarySecondary)
        {
            SwingGrapple(); //Swapped secondary action
        }
        else
        {
            PullGrapple(); //True secondary action
        }
    }

    private void SwingGrapple()
    {
        
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
            float pullForceMagnitude = 100f; // Adjust this value for stronger/weaker pull
            Vector3 pullForce = directionToGrapplePoint * pullForceMagnitude;
            characterMovement.GetComponent<Rigidbody>().AddForce(pullForce, ForceMode.VelocityChange);
        }
    }
}