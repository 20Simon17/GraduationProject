using UnityEngine;

public interface IHoldInteractable : IInteractable
{
    public float InteractionTime { get; }
    public void StartHoldInteract(GameObject interactor);
    public void StopHoldInteract();
}