using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private bool animateMenuOpening;
    private bool animateMenuClosing;
    private bool pauseMenuClosed = true;

    [SerializeField] private float buttonOneActivation;
    [SerializeField] private float buttonTwoActivation;
    [SerializeField] private float buttonThreeActivation;
    [SerializeField] private float buttonFourActivation;
    [SerializeField] private float titleActivation;

    [SerializeField] private GameObject imagesHolder;
    [SerializeField] private float imagesScaleTime;
    [SerializeField] private float smallImagesFillTime;
    [SerializeField] private float bigImageFillTime;

    [SerializeField] private GameObject textHolder;
    [SerializeField] private GameObject buttonsHolder;

    private float imageTime;
    
    private Image[] images;
    private int currentImage;

    private bool doOnce;
    
    private void Start()
    {
        //InputManager.Instance.OnPauseEvent += TogglePauseMenu;
        
        Image[] tempImages = imagesHolder.GetComponentsInChildren<Image>();
        images = new Image[tempImages.Length];
        
        for (int i = 0; i < tempImages.Length; i++)
        {
            int reverseIndex = i + 1;
            images[i] = tempImages[^reverseIndex];
        }
    }

    private void OnDisable()
    {
        //InputManager.Instance.OnPauseEvent += TogglePauseMenu;
    }

    // I should maybe separate this from unity's update and handle it through pause menu actions update
    private void Update()
    {
        if (animateMenuOpening)
        {
            AnimateOpening();
        }
        else if (animateMenuClosing)
        {
            AnimateClosing();
        }
    }

    private void AnimateOpening()
    {
        imageTime += Time.deltaTime;
        if (currentImage < images.Length)
        {
            if (images[currentImage].fillAmount < 1)
            {
                float progress = currentImage == images.Length - 1 ? GetBigFillProgress(imageTime) : GetSmallFillProgress(imageTime);
                float newFill = Mathf.Lerp(0, 1, progress);
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
            float progress = GetScaleProgress(imageTime);
            float newScaleY = Mathf.Lerp(0.01f, 1, progress);
            imagesHolder.transform.localScale = new Vector3(1, newScaleY, 1);

            if (newScaleY >= buttonOneActivation)
            {
                buttonsHolder.transform.GetChild(0).gameObject.SetActive(true);
            }
                
            if (newScaleY >= buttonTwoActivation)
            {
                buttonsHolder.transform.GetChild(1).gameObject.SetActive(true);
            }
                
            if (newScaleY >= buttonThreeActivation)
            {
                buttonsHolder.transform.GetChild(2).gameObject.SetActive(true);
            }
                
            if (newScaleY >= buttonFourActivation)
            {
                buttonsHolder.transform.GetChild(3).gameObject.SetActive(true);
            }
                
            if (newScaleY >= titleActivation)
            {
                textHolder.SetActive(true);
            }
        }
        else
        {
            textHolder.SetActive(true);
            animateMenuOpening = false;
            imageTime = 0;
        }
    }

    private void AnimateClosing()
    {
        imageTime += Time.deltaTime;
        if (imagesHolder.transform.localScale.y > 0.01f)
        {
            float progress = GetScaleProgress(imageTime);
            float newScaleY = Mathf.Lerp(1, 0.01f, progress);
            imagesHolder.transform.localScale = new Vector3(1, newScaleY, 1);
            
            if (newScaleY <= buttonFourActivation)
            {
                buttonsHolder.transform.GetChild(3).gameObject.SetActive(false);
            }
            
            if (newScaleY <= buttonThreeActivation)
            {
                buttonsHolder.transform.GetChild(2).gameObject.SetActive(false);
            }
            
            if (newScaleY <= buttonTwoActivation)
            {
                buttonsHolder.transform.GetChild(1).gameObject.SetActive(false);
            }
            
            if (newScaleY <= buttonOneActivation)
            {
                buttonsHolder.transform.GetChild(0).gameObject.SetActive(false);
            }
            
            if (newScaleY <= titleActivation)
            {
                textHolder.SetActive(false);
            }
        }
        else if (currentImage >= 0)
        {
            if (!doOnce)
            {
                imageTime = 0;
                doOnce = true;
            }
            if (images[currentImage].fillAmount > 0)
            {
                float progress = currentImage == images.Length - 1 ? GetBigFillProgress(imageTime) : GetSmallFillProgress(imageTime);
                float newFill = Mathf.Lerp(1, 0, progress);
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
            doOnce = false;
            
            buttonsHolder.SetActive(false);
            imagesHolder.SetActive(false);
        }
    }

    private float GetSmallFillProgress(float time)
    {
        return time / (smallImagesFillTime / (images.Length - 1));
    }
    
    private float GetBigFillProgress(float time)
    {
        return time / bigImageFillTime;
    }
    
    private float GetScaleProgress(float time)
    {
        return time / imagesScaleTime;
    }

    // private void TogglePauseMenu(InputValue value)
    // {
    //     if (!value.isPressed) return;
    //
    //     if (pauseMenuClosed)
    //     {
    //         Pause();
    //     }
    //     else
    //     {
    //         Resume();
    //     }
    // }

    // private void Pause()
    // {
    //     Cursor.lockState = CursorLockMode.None;
    //     imagesHolder.transform.localScale = new Vector3(1, 0.01f, 1);
    //     pauseMenuClosed = false;
    //     animateMenuOpening = true;
    //     imagesHolder.SetActive(true);
    //     buttonsHolder.SetActive(true);
    //     currentImage = 0;
    //     
    //     GameManager.Instance.Pause();
    // }

    // public void Resume()
    // {
    //     imageTime = 0;
    //     Cursor.lockState = CursorLockMode.Locked;
    //         
    //     animateMenuOpening = false;
    //     animateMenuClosing = true;
    //     
    //     currentImage = images.Length - 1;
    //     
    //     GameManager.Instance.Resume();
    // }

    public void AnimateMenuOpening()
    {
        imagesHolder.transform.localScale = new Vector3(1, 0.01f, 1);
        
        pauseMenuClosed = false;
        animateMenuOpening = true;
        
        imagesHolder.SetActive(true);
        buttonsHolder.SetActive(true);
        
        currentImage = 0;
    }
    
    public void AnimateMenuClosing()
    {
        imageTime = 0;
            
        animateMenuOpening = false;
        animateMenuClosing = true;
        
        currentImage = images.Length - 1;
    }
}
