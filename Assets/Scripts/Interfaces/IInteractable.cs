using UnityEngine;

public interface IInteractable
{
    public void Interact(GameObject interactor);

    public void ToggleLookAt(GameObject interactor, bool newToggle);
}