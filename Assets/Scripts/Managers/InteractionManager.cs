using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{
    private Transform cameraTransform;
    private PlayerDataRecord playerData;

    [SerializeField] private GameObject interactionUI;
    [SerializeField] private Image interactionFillImage;

    private GameObject interactionObject;
    private float timeOfInteraction;
    private bool isInteracting;
    private float timeOfPause;
    private bool isPaused;
    private IHoldInteractable interactionHoldInterface;

    public LayerMask interactionLayerMask;

    private GameObject previousLookingAt;

    private float pauseTime;
    
    void Start()
    {
        cameraTransform = FindAnyObjectByType<CameraActionStack>().transform;
        playerData = GetComponent<PlayerData>().dataRecord;
        
        InputManager.Instance.OnInteractEvent += Interact;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnInteractEvent -= Interact;
    }

    private void FixedUpdate()
    {
        //HandleLookingAt();
        
        if (!isPaused && isInteracting)
        {
            RaycastHit hit = GetInteractingWith();
            if (!hit.collider || hit.transform.gameObject != interactionObject)
            {
                StopHoldInteraction();
                return;
            }
            
            float interactionDuration = GetInteractionDuration();
            float progress = interactionDuration / interactionHoldInterface.InteractionDuration;
            interactionFillImage.fillAmount = progress;
            
            if (progress >= 1)
            {
                interactionHoldInterface.Interact(gameObject);
                StopHoldInteraction();
            }
        }
    }

    private void Interact(InputValue value)
    {
        RaycastHit hit = GetInteractingWith();
        if (!hit.transform || !hit.transform.TryGetComponent(out IInteractable interactable)) return;

        if (interactable is IHoldInteractable holdInteractable)
        {
            if (interactable is TimeTrial && playerData.isInTimeTrial)
            {
                Debug.Log("TimeTrial can't be interacted with when you're in a time trial");
                return;
            }
            if (value.isPressed)
            {
                isInteracting = true;
                interactionUI.SetActive(true);
                interactionObject = hit.transform.gameObject;
                interactionHoldInterface = holdInteractable;
                timeOfInteraction = Time.time;
                holdInteractable.StartHoldInteract(gameObject);
            }
            else StopHoldInteraction();
            return;
        }

        if (value.isPressed) interactable.Interact(gameObject);
    }
    
    private RaycastHit GetInteractingWith()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Physics.Raycast(ray, out RaycastHit hit, playerData.dataStruct.maxInteractionDistance, interactionLayerMask);
        return hit;
    }

    private void HandleLookingAt()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Physics.Raycast(ray, out RaycastHit hit, playerData.dataStruct.maxInteractionDistance, interactionLayerMask);

        if (hit.collider)
        {
            if (!previousLookingAt)
            {
                previousLookingAt = hit.transform.gameObject;
                previousLookingAt.GetComponent<IInteractable>()?.ToggleLookAt(gameObject, true);
            }
            else if (previousLookingAt != hit.transform.gameObject)
            {
                previousLookingAt.GetComponent<IInteractable>()?.ToggleLookAt(gameObject, false);
                hit.transform.GetComponent<IInteractable>()?.ToggleLookAt(gameObject, true);
                previousLookingAt = hit.transform.gameObject;
            }
        }
        else if (previousLookingAt)
        {
            previousLookingAt.GetComponent<IInteractable>()?.ToggleLookAt(gameObject, false);
            previousLookingAt = null;
        }
    }

    private void StopHoldInteraction()
    {
        isInteracting = false;
        interactionUI.SetActive(false);
        interactionFillImage.fillAmount = 0;
        interactionObject = null;
        timeOfInteraction = 0;
        interactionHoldInterface?.StopHoldInteract();
        interactionHoldInterface = null;
        pauseTime = 0;
    }

    private float GetInteractionDuration()
    {
        return Time.time - (timeOfInteraction + pauseTime);
    }

    public void Pause()
    {
        StopHoldInteraction();
        
        if (!isInteracting) return;

        isPaused = true;
        timeOfPause = Time.time;
    }

    public void Resume()
    {
        if (!isPaused) return;
        
        isPaused = false;
        pauseTime += Time.time - timeOfPause;
    }
}