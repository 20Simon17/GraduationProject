using UnityEngine;

public class ZiplinePoint : MonoBehaviour
{
    [SerializeField]
    private GameObject attachObject;
    
    public Vector3 AttachLocation => attachObject.transform.position;
}