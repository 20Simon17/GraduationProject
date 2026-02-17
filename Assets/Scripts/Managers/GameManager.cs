
public class GameManager : Singleton<GameManager>
{
    PlayerActionStack playerActionStack;
    CameraActionStack cameraActionStack;
    //UIActionStack uiActionStack

    public bool gameIsPaused = false;
    public bool IsGamePaused => gameIsPaused;
    
    private void Start()
    {
        playerActionStack = FindFirstObjectByType<PlayerActionStack>();
        cameraActionStack = FindFirstObjectByType<CameraActionStack>();
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
        gameIsPaused = true;
    }

    public void Resume()
    {
        InputManager.Instance.Resume();
        gameIsPaused = false;
    }
}