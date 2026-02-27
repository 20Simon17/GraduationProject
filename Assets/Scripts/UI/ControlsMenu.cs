using UnityEngine;

public class ControlsMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject imagesHolder;
    [SerializeField] private GameObject textHolder;
    [SerializeField] private GameObject buttonsHolder;
    
    [Header("Durations")]
    
    private bool animateMenuOpening;
    private bool animateMenuClosing;
    
    private float animationTime;
    
    void Start()
    {
        
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
    }

    private void AnimateOpening()
    {
        animationTime += Time.deltaTime;
    }

    private void AnimateClosing()
    {
        animationTime += Time.deltaTime;
    }
    
    public void OpenMenu()
    {
        imagesHolder.SetActive(true);
        textHolder.SetActive(true);
        buttonsHolder.SetActive(true);
    }
    
    public void CloseMenu()
    {
        imagesHolder.SetActive(false);
        textHolder.SetActive(false);
        buttonsHolder.SetActive(false);
    }
    
    public void AnimateButtonsAppearing()
    {
        // button.setactive(true) one after the other
    }

    public void AnimateButtonsDisappearing()
    {
        
    }
}
