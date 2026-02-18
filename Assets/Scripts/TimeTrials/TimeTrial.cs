using System.Globalization;
using TMPro;
using UnityEngine;

public class TimeTrial : MonoBehaviour, IHoldInteractable
{
    public float InteractionDuration { get => interactionDuration; set => interactionDuration = value; }
    [SerializeField] private float interactionDuration;

    private bool countDownActive;
    private float countDownTime;

    private bool timeTrialActive;
    private float timeElapsed;

    [SerializeField] private float playerBestTime;

    private GameObject playerObject;
    
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

        if (timeTrialActive)
        {
            timeElapsed += Time.fixedDeltaTime;

            string timeText = System.TimeSpan.FromSeconds(timeElapsed).ToString("mm':'ss'.'fff");
            timeTrialTimerText.text = timeText;
            
            if (timeElapsed > 3600) EndTimeTrial(false);
        }
    }

    public void StartTimeTrial()
    {
        playerObject.transform.position = transform.position;
        playerObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        
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
        string timeText = System.TimeSpan.FromSeconds(timeElapsed).ToString("mm':'ss'.'fff");
        Debug.Log($"Your time was: {timeText}");
        ToggleTimeTrialUI(false);
        timeTrialActive = false;
        timeElapsed = 0;
        Destroy(spawnedEndObject);
        
        if (completed && timeElapsed > playerBestTime)
        {
            HandleNewBest(timeElapsed);
        }
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
}
