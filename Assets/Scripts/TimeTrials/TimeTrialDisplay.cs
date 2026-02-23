using TMPro;
using UnityEngine;

public class TimeTrialDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private TMP_Text completionStatusText;
    [SerializeField] private TMP_Text personalBestText;
    [SerializeField] private TMP_Text completionsText;
    [SerializeField] private TMP_Text rewardsText;

    public void LoadData(TimeTrialData timeTrialData)
    {
        nameText.text = timeTrialData.timeTrialName;
        difficultyText.text = timeTrialData.timeTrialDifficulty.ToString();
        completionsText.text = "Completions:\n" + timeTrialData.numberOfCompletions;
        rewardsText.text = $"{timeTrialData.firstCompletionReward} | {timeTrialData.repeatCompletionReward}";
        
        if (timeTrialData.hasBeenCompleted)
        {
            completionStatusText.text = "Completed";
            completionStatusText.color = Color.green;

            personalBestText.text = "Personal Best:\n" + timeTrialData.playerPersonalBest;
        }
        else
        {
            completionStatusText.text = "Not Completed";
            completionStatusText.color = Color.red;
            
            personalBestText.text = "Personal Best:\n" + "None";
        }
    }
}
