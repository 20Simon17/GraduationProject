using UnityEngine;

public class TempTeleportCollider : MonoBehaviour
{
    private PlayerActionStack player;
    [SerializeField] private Vector3 spawnLocation;
    
    private void Start()
    {
        player = FindAnyObjectByType<PlayerActionStack>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            player.transform.position = spawnLocation;
        }
    }
}
