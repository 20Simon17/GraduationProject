using UnityEngine;
using UnityEngine.UI;

public class TimeTrial : MonoBehaviour, IHoldInteractable
{
    public float InteractionDuration { get => interactionDuration; set => interactionDuration = value; }
    [SerializeField] private float interactionDuration;
    
    public void Interact(GameObject interactor)
    {
        StartTimeTrial();
    }

    public void StartHoldInteract(GameObject interactor)
    {
    }

    public void StopHoldInteract()
    {
    }

    private void StartTimeTrial()
    {
        Debug.Log("Started time trial");
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
