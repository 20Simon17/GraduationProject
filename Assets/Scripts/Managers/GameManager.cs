
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public delegate void OnGamePausedDelegate();
    public OnGamePausedDelegate OnGamePausedEvent;
    
    public delegate void OnGameResumedDelegate();
    public OnGameResumedDelegate OnGameResumedEvent;
    
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
        
        OnGamePausedEvent?.Invoke();
    }

    public void Resume()
    {
        InputManager.Instance.Resume();
        playerActionStack.Resume();
        interactionManager.Resume();
        gameIsPaused = false;
        
        OnGameResumedEvent?.Invoke();
    }
}