using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    public Image[] images;
    private int currentImage;
    
    private void Start()
    {
        InputManager.Instance.OnPauseEvent += TogglePauseMenu;
        
        Image[] tempImages = imagesHolder.GetComponentsInChildren<Image>();
        images = new Image[tempImages.Length];
        
        for (int i = 0; i < tempImages.Length; i++)
        {
            int reverseIndex = i + 1;
            images[i] = tempImages[^reverseIndex];
        }
    }

    private void Update()
    {
        if (animateMenuOpening)
        {
            imageTime += Time.deltaTime;
            if (currentImage < images.Length)
            {
                if (images[currentImage].fillAmount < 1)
                {
                    float newFill = Mathf.Lerp(0, 1, GetDividedProgress(imageTime));
                    images[currentImage].fillAmount = newFill;
                }
                else
                {
                    currentImage++;
                    imageTime = 0;
                }
            }
            else if (imagesHolder.transform.localScale.y < 1)
            {
                float newScaleY = Mathf.Lerp(0.01f, 1, GetTotalProgress(imageTime));
                imagesHolder.transform.localScale = new Vector3(1, newScaleY, 1);
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
                float newScaleY = Mathf.Lerp(1, 0.01f, GetTotalProgress(imageTime));
                imagesHolder.transform.localScale = new Vector3(1, newScaleY, 1);
                //reset imageTime to 0 here when this is done
            }
            else if (currentImage >= 0)
            {
                if (currentImage == 3 && GetDividedProgress(imageTime) >= 1 && images[currentImage].fillAmount > 0.3)
                {
                    imageTime = 0;
                }
                else if (images[currentImage].fillAmount > 0)
                {
                    float newFill = Mathf.Lerp(1, 0, GetDividedProgress(imageTime));
                    images[currentImage].fillAmount = newFill;
                }
                else
                {
                    currentImage--;
                    imageTime = 0;
                }
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

    private float GetDividedProgress(float time)
    {
        return time / (imagesScaleTime / images.Length);
    }

    private float GetTotalProgress(float time)
    {
        return time / imagesScaleTime;
    }

    private void TogglePauseMenu(InputValue value)
    {
        if (!value.isPressed) return;

        if (pauseMenuClosed)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    private void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        imagesHolder.transform.localScale = new Vector3(1, 0.01f, 1);
        pauseMenuClosed = false;
        animateMenuOpening = true;
        imagesHolder.SetActive(true);
        currentImage = 0;
        
        GameManager.Instance.Pause();
    }

    public void Resume()
    {
        imageTime = 0;
        Cursor.lockState = CursorLockMode.Locked;
        textHolder.SetActive(false);
        buttonsHolder.SetActive(false);
            
        animateMenuOpening = false;
        animateMenuClosing = true;
        
        currentImage = images.Length - 1;
        
        GameManager.Instance.Resume();
    }
}
