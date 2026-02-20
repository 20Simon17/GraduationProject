using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject imagesHolder;
    [SerializeField] private GameObject textHolder;
    [SerializeField] private GameObject buttonsHolder;

    [Header("Durations")] 
    [SerializeField] private float backgroundFillTime;
    [SerializeField] private float backgroundScaleTime;
    [SerializeField] private float buttonTogglingRate;
    
    private bool animateMenuOpening;
    private bool animateMenuClosing;
    private float animationTime;

    private bool animateButtonsAppearing;
    private bool animateButtonsDisappearing;
    private float timeOfButtonToggling;

    private Image backgroundImage;

    public delegate void MenuOpenedDelegate();
    public MenuOpenedDelegate MenuOpenedEvent;
    
    
    void Start()
    {
        backgroundImage = imagesHolder.transform.GetChild(0).GetComponent<Image>();
    }
    
    void Update()
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
        if (backgroundImage.fillAmount < 1)
        {
            float progress = animationTime / backgroundFillTime;
            float newFill = Mathf.Lerp(0, 1, progress);
            backgroundImage.fillAmount = newFill;

            if (newFill >= 1)
            {
                animationTime = 0;
            }
        }
        else if (backgroundImage.transform.localScale.x < 1)
        {
            float progress = animationTime / backgroundScaleTime;
            float newScale = Mathf.Lerp(0.01f, 1, progress);
            backgroundImage.transform.localScale = new Vector3(newScale, 1, 1);
        }
        else
        {
            textHolder.SetActive(true);
            animateMenuOpening = false;
            animationTime = 0;
            
            MenuOpenedEvent?.Invoke();
        }
    }

    private void AnimateClosing()
    {
        animationTime += Time.deltaTime;
        if (backgroundImage.transform.localScale.x > 0.01f)
        {
            float progress = animationTime / backgroundScaleTime;
            float newScale = Mathf.Lerp(1, 0.01f, progress);
            backgroundImage.transform.localScale = new Vector3(newScale, 1, 1);
            
            if (Mathf.Approximately(newScale, 0.01f))
            {
                animationTime = 0;
            }
        }
        else if (backgroundImage.fillAmount > 0)
        {
            float progress = animationTime / backgroundFillTime;
            float newFill = Mathf.Lerp(1, 0, progress);
            backgroundImage.fillAmount = newFill;
        }
        else
        {
            animateMenuClosing = false;
            animationTime = 0;
            
            imagesHolder.SetActive(false);
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
            buttonsHolder.SetActive(false);
        }
    }
    
    public void OpenMenu()
    {
        backgroundImage.transform.localScale = new Vector3(0.01f, 1, 1);
        
        animateMenuOpening = true;
        animateMenuClosing = false;
        
        imagesHolder.SetActive(true);
        buttonsHolder.SetActive(true);
    }
    
    public void CloseMenu()
    {
        animationTime = 0;
        
        animateMenuClosing = true;
        animateMenuOpening = false;
        
        textHolder.SetActive(false);
    }
    
    public void ShowButtons()
    {
        timeOfButtonToggling = Time.time;
        
        animateButtonsAppearing = true;
        animateButtonsDisappearing = false;
        
        buttonsHolder.SetActive(true);
    }

    public void HideButtons()
    {
        timeOfButtonToggling = Time.time;
        
        animateButtonsAppearing = false;
        animateButtonsDisappearing = true;
    }
}
