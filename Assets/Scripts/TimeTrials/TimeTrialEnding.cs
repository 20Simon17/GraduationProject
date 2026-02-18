using UnityEngine;

public class TimeTrialEnding : MonoBehaviour
{
    public TimeTrial owner;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerActionStack>() != null)
        {
            owner?.EndTimeTrial(true);
        }
    }
}