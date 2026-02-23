using System;

[Serializable]
public record TimeTrialData
{
    public string timeTrialName;
    public TimeTrialDifficulty timeTrialDifficulty;
    public bool hasBeenCompleted;
    
    public float playerPersonalBest;
    public int numberOfCompletions;
    public int numberOfAttempts;
    
    public float firstCompletionReward;
    public float repeatCompletionReward;
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