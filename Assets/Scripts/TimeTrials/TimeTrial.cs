using System.Globalization;
using TMPro;
using UnityEngine;

public class TimeTrial : MonoBehaviour, IHoldInteractable
{
    // TODO: might want to have only 1 ending object and use it only on the active trial
    public Vector3 endLocation; 
    
    // TODO: Add some way to showcase where the time trial starts and ends
    // make the world space ui not be toggled when looked at, make it a static board object or something similar next to the trial
    
    public float InteractionDuration { get => interactionDuration; set => interactionDuration = value; }
    [SerializeField] private float interactionDuration;
    
    private MeshRenderer meshRenderer;
    private Collider selfCollider;

    private bool countDownActive;
    private float countDownTime;

    private bool timeTrialActive;
    private float timeElapsed;

    private bool displayEnabled;

    private GameObject playerObject;
    private PlayerData playerData;
    
    [SerializeField] private TMP_Text countDownText;
    [SerializeField] private TMP_Text timeTrialTimerText;

    [SerializeField] private GameObject endObjectPrefab;
    
    [Space(10)]
    public TimeTrialData timeTrialData;

    private GameObject spawnedEndObject;

    #region Interactions
    public void Interact(GameObject interactor)
    {
        playerObject = interactor;
        
        StartTimeTrial();
    }

    public void ToggleLookAt(GameObject interactor, bool newToggle)
    {
        if (newToggle)
        {
            EnableDisplay();
        }
        else
        {
            DisableDisplay();
        }
    }

    public void StartHoldInteract(GameObject interactor)
    {
    }

    public void StopHoldInteract()
    {
    }
    #endregion

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        selfCollider = GetComponent<Collider>();
        
        TimeTrialManager.Instance.AddTimeTrial(this);
    }

    private void OnDisable()
    {
        TimeTrialManager.Instance.RemoveTimeTrial(this);
    }

    private void FixedUpdate()
    {
        if (countDownActive && !GameManager.Instance.IsGamePaused)
        {
            countDownTime -= Time.fixedDeltaTime;
            countDownText.text = Mathf.Floor(countDownTime).ToString(CultureInfo.CurrentCulture);

            if (countDownTime <= 0.1)
            {
                countDownActive = false;
                countDownText.gameObject.SetActive(false);

                timeTrialActive = true;
                ToggleTimeTrialUI(true);
                
                FindAnyObjectByType<PlayerActionStack>().CompleteCurrentAction();
            }
        }

        //perhaps move this to void update instead for greater accuracy?
        //or use Time.time and get the difference + pause buffer and see if that is even more accurate (more performant too)
        if (timeTrialActive && !GameManager.Instance.IsGamePaused)
        {
            timeElapsed += Time.fixedDeltaTime;

            string timeText = TimeTrialManager.Instance.TimeToString(timeElapsed);
            timeTrialTimerText.text = timeText;
            
            if (timeElapsed > 3600) EndTimeTrial(false);
        }
    }

    public void StartTimeTrial()
    {
        TimeTrialManager.Instance.HideAllTimeTrials();

        Vector3 timeTrialDirection = endLocation - transform.position;
        
        playerObject.transform.position = transform.position;
        playerObject.transform.LookAt(endLocation);
        playerObject.transform.eulerAngles = new Vector3(0, playerObject.transform.eulerAngles.y, 0);
        playerObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        
        playerData = playerObject.GetComponent<PlayerData>();
        playerData.dataRecord.isInTimeTrial = true;
        
        spawnedEndObject = Instantiate(endObjectPrefab);
        spawnedEndObject.transform.position = endLocation;

        spawnedEndObject.GetComponent<TimeTrialEnding>().owner = this;
        
        DoTimeTrialCountdown();
        DisableDisplay();
    }

    private void DoTimeTrialCountdown()
    {
        countDownTime = 4;
        countDownText.text = "3";
        countDownText.gameObject.SetActive(true);
        countDownActive = true;

        PlayerActionStack player = FindAnyObjectByType<PlayerActionStack>();
        player.ClearAllActions();
        player.AddWaitingAction();
    }

    public void EndTimeTrial(bool completed)
    {
        TimeTrialManager.Instance.ShowAllTimeTrials();
        
        ToggleTimeTrialUI(false);
        timeTrialActive = false;
        Destroy(spawnedEndObject);

        playerData.dataRecord.isInTimeTrial = false;
        // Do I have to clear my reference to the player data here? If you enter multiple time trials, would it cause more memory usage?
        
        if (completed && (timeTrialData.playerPersonalBest == 0 || timeElapsed < timeTrialData.playerPersonalBest))
        {
            //Debug.Log("New best time!");
            HandleNewBest(timeElapsed);
        }
        
        timeElapsed = 0;
    }

    private void ToggleTimeTrialUI(bool newActiveState)
    {
        timeTrialTimerText.gameObject.SetActive(newActiveState);
        timeTrialTimerText.text = "00:00";
    }

    private void HandleNewBest(float newBest)
    {
        timeTrialData.playerPersonalBest = newBest;
    }

    public void HideTimeTrial()
    {
        meshRenderer.enabled = false;
        selfCollider.enabled = false;
    }

    public void ShowTimeTrial()
    {
        meshRenderer.enabled = true;
        selfCollider.enabled = true;
    }

    private void EnableDisplay()
    {
        displayEnabled = true;
        GameObject displayObject = TimeTrialManager.Instance.RequestTimeTrialDisplay(this);
        displayObject.transform.position = transform.position + Vector3.up * 3;
        displayObject.SetActive(true);
    }

    private void DisableDisplay()
    {
        displayEnabled = false;
        TimeTrialManager.Instance.DisableTimeTrialDisplay();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !displayEnabled)
        {
            EnableDisplay();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && displayEnabled)
        {
            DisableDisplay();
        }
    }
}
