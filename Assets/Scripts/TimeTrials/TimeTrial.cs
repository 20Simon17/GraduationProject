using System.Globalization;
using TMPro;
using UnityEngine;

public class TimeTrial : MonoBehaviour, IHoldInteractable
{
    public float InteractionDuration { get => interactionDuration; set => interactionDuration = value; }
    [SerializeField] private float interactionDuration;
    
    private MeshRenderer meshRenderer;
    private Collider selfCollider;

    private bool countDownActive;
    private float countDownTime;

    private bool timeTrialActive;
    private float timeElapsed;

    [SerializeField] private float playerBestTime;

    private GameObject playerObject;
    private PlayerData playerData;
    
    [SerializeField] private TMP_Text countDownText;
    [SerializeField] private TMP_Text timeTrialTimerText;

    [SerializeField] private GameObject endObjectPrefab;
    
    [SerializeField] private TimeTrialData timeTrialData;

    private GameObject spawnedEndObject;

    #region Interactions
    public void Interact(GameObject interactor)
    {
        playerObject = interactor;
        
        StartTimeTrial();
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
        
        playerObject.transform.position = transform.position;
        playerObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        
        playerData = playerObject.GetComponent<PlayerData>();
        playerData.dataRecord.isInTimeTrial = true;
        
        spawnedEndObject = Instantiate(endObjectPrefab);
        spawnedEndObject.transform.position = timeTrialData.EndLocation;

        spawnedEndObject.GetComponent<TimeTrialEnding>().owner = this;
        
        DoTimeTrialCountdown();
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

        playerData.dataRecord.isInTimeTrial = true;
        // Do I have to clear my reference to the player data here? If you enter multiple time trials, would it cause more memory usage?
        
        if (completed && (playerBestTime == 0 || timeElapsed < playerBestTime))
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
        playerBestTime = newBest;
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
}
