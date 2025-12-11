using UnityEngine;

public abstract class ZiplinePoint : MonoBehaviour
{
    [SerializeField]
    private GameObject attachObject;
    
    public Vector3 AttachLocation => attachObject.transform.position;
    
    public abstract Vector3[] GetAttachmentVerts();
}