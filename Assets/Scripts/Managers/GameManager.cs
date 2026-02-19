
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private PlayerActionStack playerActionStack;
    private CameraActionStack cameraActionStack;
    private UserInterfaceActionStack userInterfaceActionStack;
    private InteractionManager interactionManager;

    public bool gameIsPaused = false;
    public bool IsGamePaused => gameIsPaused;
    
    private void Start()
    {
        playerActionStack = FindFirstObjectByType<PlayerActionStack>();
        cameraActionStack = FindFirstObjectByType<CameraActionStack>();
        userInterfaceActionStack = FindFirstObjectByType<UserInterfaceActionStack>();
        interactionManager = playerActionStack.GetComponent<InteractionManager>();
    }

    private void Update()
    {
        userInterfaceActionStack.UpdateActionStack();
        
        if (!gameIsPaused)
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