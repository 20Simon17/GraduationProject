using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField]
    private GameObject playerObject;
    
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
        
        if (playerObject == null)
        {
            playerObject = transform.parent.gameObject;
        }
        
        transform.rotation = playerObject.transform.rotation;
    }
    
    private void OnLook(InputValue value)
    {
        Vector2 lookVector = value.Get<Vector2>();
        playerObject.transform.Rotate(Vector3.up, lookVector.x * mouseSensitivity);
        
        cameraVerticalRotation += -lookVector.y * mouseSensitivity;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, clampAngleMin, clampAngleMax);
        transform.localEulerAngles = new Vector3(cameraVerticalRotation, 0f, 0f);
    }
}