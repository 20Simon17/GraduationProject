using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    private float verticalScaleMin = 0.01f;

    public bool animateMenuOpening = false;
    public bool animateMenuClosing = false;
    private bool pauseMenuClosed = true;

    [SerializeField] private GameObject imagesHolder;
    [SerializeField] private float imagesScaleTime;
    private float imageTime = 0;

    [SerializeField] private GameObject textHolder;
    [SerializeField] private GameObject buttonsHolder;
    
    private void Start()
    {
        InputManager.Instance.OnPauseEvent += TogglePauseMenu;
    }

    private void Update()
    {
        if (animateMenuOpening)
        {
            imageTime += Time.deltaTime;
            if (imagesHolder.transform.localScale.x < 1)
            {
                float progress = imageTime / (imagesScaleTime / 2);
                float newScaleX = Mathf.Lerp(0, 1, progress);
                imagesHolder.transform.localScale = new Vector3(newScaleX, imagesHolder.transform.localScale.y, 1);
            }
            else if (imagesHolder.transform.localScale.y < 1)
            {
                float progress = (imageTime - imagesScaleTime / 2)/ (imagesScaleTime / 2);
                float newScaleY = Mathf.Lerp(0.01f, 1, progress);
                imagesHolder.transform.localScale = new Vector3(imagesHolder.transform.localScale.x, newScaleY, 1);
            }
            else
            {
                textHolder.SetActive(true);
                buttonsHolder.SetActive(true);
                animateMenuOpening = false;
                imageTime = 0;
            }
        }

        else if (animateMenuClosing)
        {
            imageTime += Time.deltaTime;
            if (imagesHolder.transform.localScale.y > 0.01f)
            {
                float progress = imageTime / (imagesScaleTime / 2);
                float newScaleY = Mathf.Lerp(1, 0.01f, progress);
                imagesHolder.transform.localScale = new Vector3(imagesHolder.transform.localScale.x, newScaleY, 1);
            }
            else if (imagesHolder.transform.localScale.x > 0)
            {
                float progress = (imageTime - imagesScaleTime / 2) / (imagesScaleTime / 2);
                float newScaleX = Mathf.Lerp(1, 0, progress);
                imagesHolder.transform.localScale = new Vector3(newScaleX, imagesHolder.transform.localScale.y, 1);
            }
            else
            {
                animateMenuClosing = false;
                imageTime = 0;
                pauseMenuClosed = true;
                
                imagesHolder.SetActive(false);
            }
        }
    }

    private void TogglePauseMenu(InputValue value)
    {
        if (!value.isPressed) return;

        if (pauseMenuClosed)
        {
            Cursor.lockState = CursorLockMode.None;
            imagesHolder.transform.localScale = new Vector3(0, 0.01f, 1);
            pauseMenuClosed = false;
            animateMenuOpening = true;
            imagesHolder.SetActive(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            textHolder.SetActive(false);
            buttonsHolder.SetActive(false);
            
            animateMenuOpening = false;
            animateMenuClosing = true;
        }
    }
}
