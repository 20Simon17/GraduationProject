using System.Collections.Generic;
using UnityEngine;

public class TimeTrialManager : Singleton<TimeTrialManager>
{
    private readonly List<TimeTrial> timeTrials = new List<TimeTrial>();
    
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
}