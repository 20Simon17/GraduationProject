using System;
using UnityEngine;

[Serializable]
public record TimeTrialData
{
    [Header("Important")]
    public string timeTrialName;
    public TimeTrialDifficulty timeTrialDifficulty;
    public float firstCompletionReward;
    public float repeatCompletionReward;
    
    [Header("No Edit")]
    public bool hasBeenCompleted;
    public int numberOfAttempts;
    public int numberOfCompletions;
    public float playerPersonalBest;
}

[Serializable]
public enum TimeTrialDifficulty
{
    Easy,
    Medium,
    Hard,
    Expert,
    Insane,
    Impossible
}