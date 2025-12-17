using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [HideInInspector] public Camera cameraComponent;
    
    [SerializeField] private GameObject playerObject;
    private CharacterMovement characterMovement;
    
    [SerializeField]
    private float mouseSensitivity = 1f;

    [SerializeField] 
    private float clampAngleMin = -45f;
    
    [SerializeField] 
    private float clampAngleMax = 45f;

    private float cameraVerticalRotation = 0f;
    
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
     
        cameraComponent = GetComponent<Camera>();
        
        if (playerObject == null)
        {
            playerObject = transform.parent.gameObject;
        }
        
        characterMovement = playerObject.GetComponent<CharacterMovement>();
        
        transform.rotation = playerObject.transform.rotation;
    }
    
    private void OnLook(InputValue value)
    {
        Vector2 lookVector = value.Get<Vector2>();
        
        playerObject.transform.Rotate(Vector3.up, lookVector.x * mouseSensitivity);
        
        /*if (characterMovement.IsDisconnectedFromCamera)
        {
            transform.Rotate(Vector3.up, lookVector.x * mouseSensitivity); //rotate the camera and stop the player from rotating? maybe don't want this though
        }*/
        
        cameraVerticalRotation += -lookVector.y * mouseSensitivity;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, clampAngleMin, clampAngleMax);
        transform.localEulerAngles = new Vector3(cameraVerticalRotation, 0f, 0f);
    }
    
    public bool IsObjectVisible(Transform targetObject)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cameraComponent);

        foreach (var plane in planes)
        {
            if (plane.GetDistanceToPoint(targetObject.position) < 0f)
            {
                return false;
            }
        }

        return true;
    }

    public GameObject GetLookAtObject()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        return Physics.Raycast(ray, out RaycastHit hit, 100) ? hit.transform.gameObject : null;
    }

    public RaycastHit? GetLookAtHit()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        return Physics.Raycast(ray, out RaycastHit hit, 100) ? hit : null;
    }
}