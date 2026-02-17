using UnityEngine;

public interface IHoldInteractable : IInteractable
{
    public float InteractionDuration { get; set; }
    public void StartHoldInteract(GameObject interactor);
    public void StopHoldInteract();
}