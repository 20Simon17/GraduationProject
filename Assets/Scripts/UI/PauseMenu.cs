using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject imagesHolder;
    [SerializeField] private GameObject buttonsHolder;
    
    [Header("Durations")]
    [SerializeField] private float imagesScaleTime;
    [SerializeField] private float smallImagesFillTime;
    [SerializeField] private float bigImageFillTime;
    [SerializeField] private float buttonTogglingRate;
    
    private bool animateMenuOpening;
    private bool animateMenuClosing;
    private float animationTime;

    private bool animateButtonsAppearing;
    private bool animateButtonsDisappearing;
    private float timeOfButtonToggling;
    
    private Image[] images;
    private int currentImage;

    private bool doOnce;

    private void Start()
    {
        Image[] tempImages = imagesHolder.GetComponentsInChildren<Image>();
        images = new Image[tempImages.Length];

        for (int i = 0; i < tempImages.Length; i++)
        {
            int reverseIndex = i + 1;
            images[i] = tempImages[^reverseIndex];
        }
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

        if (animateButtonsAppearing)
        {
            AnimateButtonsAppearing();
        }
        else if (animateButtonsDisappearing)
        {
            AnimateButtonsDisappearing();
        }
    }

    private void AnimateOpening()
    {
        animationTime += Time.deltaTime;
        if (currentImage < images.Length)
        {
            if (images[currentImage].fillAmount < 1)
            {
                float progress = currentImage == images.Length - 1 ? GetBigFillProgress(animationTime) : GetSmallFillProgress(animationTime);
                float newFill = Mathf.Lerp(0, 1, progress);
                images[currentImage].fillAmount = newFill;
            }
            else
            {
                currentImage++;
                animationTime = 0;
            }
        }
        else if (imagesHolder.transform.localScale.y < 1)
        {
            float progress = GetScaleProgress(animationTime);
            float newScaleY = Mathf.Lerp(0.01f, 1, progress);
            imagesHolder.transform.localScale = new Vector3(1, newScaleY, 1);
        }
        else
        {
            animateMenuOpening = false;
            animationTime = 0;
        }
    }

    private void AnimateClosing()
    {
        animationTime += Time.deltaTime;
        if (imagesHolder.transform.localScale.y > 0.01f)
        {
            float progress = GetScaleProgress(animationTime);
            float newScaleY = Mathf.Lerp(1, 0.01f, progress);
            imagesHolder.transform.localScale = new Vector3(1, newScaleY, 1);
        }
        else if (currentImage >= 0)
        {
            if (!doOnce)
            {
                animationTime = 0;
                doOnce = true;
            }
            if (images[currentImage].fillAmount > 0)
            {
                float progress = currentImage == images.Length - 1 ? GetBigFillProgress(animationTime) : GetSmallFillProgress(animationTime);
                float newFill = Mathf.Lerp(1, 0, progress);
                images[currentImage].fillAmount = newFill;
            }
            else
            {
                currentImage--;
                animationTime = 0;
            }
        }
        else
        {
            animateMenuClosing = false;
            animationTime = 0;
            doOnce = false;
        }
    }
    
    private void AnimateButtonsAppearing()
    {
        if (Time.time - timeOfButtonToggling > buttonTogglingRate)
        {
            timeOfButtonToggling += buttonTogglingRate;

            for (int i = 0; i < buttonsHolder.transform.childCount; i++)
            {
                if (buttonsHolder.transform.GetChild(i).gameObject.activeSelf) continue;
                buttonsHolder.transform.GetChild(i).gameObject.SetActive(true);
                return;
            }
            
            animateButtonsAppearing = false;
        }
    }

    private void AnimateButtonsDisappearing()
    {
        if (Time.time - timeOfButtonToggling > buttonTogglingRate)
        {
            timeOfButtonToggling += buttonTogglingRate;
            
            for (int i = 0; i < buttonsHolder.transform.childCount; i++)
            {
                if (!buttonsHolder.transform.GetChild(i).gameObject.activeSelf) continue;
                buttonsHolder.transform.GetChild(i).gameObject.SetActive(false);
                return;
            }
            
            animateButtonsDisappearing = false;
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

    public void OpenMenu()
    {
        animateMenuOpening = true;
        imagesHolder.transform.localScale = new Vector3(1, 0.01f, 1);
        currentImage = 0;
    }
    
    public void CloseMenu()
    {
        animateMenuOpening = false;
        animateMenuClosing = true;
        animationTime = 0;
        currentImage = images.Length - 1;
    }

    public void ShowButtons()
    {
        timeOfButtonToggling = Time.time;
        
        animateButtonsAppearing = true;
        animateButtonsDisappearing = false;
    }

    public void HideButtons()
    {
        timeOfButtonToggling = Time.time;
        
        animateButtonsAppearing = false;
        animateButtonsDisappearing = true;
    }
}