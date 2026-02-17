using UnityEngine;

public class TimeTrial : MonoBehaviour, IHoldInteractable
{
    public float InteractionDuration { get; set; }
    private bool isInteracting;
    private float interactionStart;
    private GameObject currentInteractor;

    private void FixedUpdate()
    {
        if (isInteracting && Time.time - interactionStart > InteractionDuration)
        {
            Interact(currentInteractor);
        }
    }
    
    public void Interact(GameObject interactor)
    {
        StartTimeTrial();
    }


    public void StartHoldInteract(GameObject interactor)
    {
        isInteracting = true;
        interactionStart = Time.time;
        currentInteractor = interactor;
    }

    public void StopHoldInteract()
    {
        isInteracting = false;
        currentInteractor = null;
    }

    private void StartTimeTrial()
    {
        
    }

    private void DoTimeTrialCountdown()
    {
        
    }

    private void EndTimeTrial()
    {
        
    }

    private void ToggleTimeTrialUI(bool newActiveState)
    {
        
    }

    private void HandleNewBest()
    {
        
    }
}
