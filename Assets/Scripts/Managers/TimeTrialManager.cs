using System.Collections.Generic;
using UnityEngine;

public class TimeTrialManager : Singleton<TimeTrialManager>
{
    private readonly List<TimeTrial> timeTrials = new List<TimeTrial>();
    
    [SerializeField] private TimeTrialDisplay timeTrialDisplay;
    private TimeTrial currentDisplayHolder;

    public void AddTimeTrial(TimeTrial timeTrial)
    {
        timeTrials.Add(timeTrial);
    }

    public void RemoveTimeTrial(TimeTrial timeTrial)
    {
        timeTrials.Remove(timeTrial);
    }

    public void HideAllTimeTrials()
    {
        foreach (TimeTrial timeTrial in timeTrials)
        {
            timeTrial.HideTimeTrial();
        }
    }

    public void ShowAllTimeTrials()
    {
        foreach (TimeTrial timeTrial in timeTrials)
        {
            timeTrial.ShowTimeTrial();
        }
    }

    public string TimeToString(float time)
    {
        return System.TimeSpan.FromSeconds(time).ToString("mm':'ss'.'fff");
    }

    public GameObject RequestTimeTrialDisplay(TimeTrial timeTrial)
    {
        if (currentDisplayHolder && currentDisplayHolder != timeTrial)
        {
            currentDisplayHolder.ToggleLookAt(gameObject, false);
        }
        
        timeTrialDisplay.LoadData(timeTrial.timeTrialData);
        currentDisplayHolder = timeTrial;
        return timeTrialDisplay.gameObject;
    }

    public void DisableTimeTrialDisplay()
    {
        timeTrialDisplay.gameObject.SetActive(false);
    }
}