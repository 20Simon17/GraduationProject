
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    PlayerActionStack playerActionStack;
    CameraActionStack cameraActionStack;
    //UIActionStack uiActionStack
    InteractionManager interactionManager;

    public bool gameIsPaused = false;
    public bool IsGamePaused => gameIsPaused;
    
    private void Start()
    {
        playerActionStack = FindFirstObjectByType<PlayerActionStack>();
        cameraActionStack = FindFirstObjectByType<CameraActionStack>();
        interactionManager = playerActionStack.GetComponent<InteractionManager>();
    }

    private void Update()
    {
        if (gameIsPaused)
        {
            //uiActionStack.Update();
        }
        else
        {
            playerActionStack.UpdateActionStack();
            cameraActionStack.UpdateActionStack();
        }
    }

    public void Pause()
    {
        InputManager.Instance.Pause();
        playerActionStack.Pause();
        interactionManager.Pause();
        gameIsPaused = true;
    }

    public void Resume()
    {
        InputManager.Instance.Resume();
        playerActionStack.Resume();
        interactionManager.Resume();
        gameIsPaused = false;
    }
}