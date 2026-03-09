using UnityEngine;

public class ZiplinePoint : MonoBehaviour
{
    [SerializeField] private GameObject attachObject;

    private MeshRenderer[] meshRenderers;
    private Collider[] colliders;
    [SerializeField] private GameObject[] attachmentPoints;

    public Vector3 attachDirection;
    
    public EPointTypes pointType;
    
    public enum EPointTypes
    {
        Pole,
        Wall,
        Ceiling
    }

    [SerializeField] private Material ghostMaterial;
    [SerializeField] private Material defaultMaterial;

    #region Properties
    public Vector3 AttachLocation => attachObject.transform.position;
    public GameObject OwnerObject => transform.parent.gameObject;
    public Zipline Owner => OwnerObject.GetComponent<Zipline>();
    #endregion
    
    private void OnEnable()
    {
        meshRenderers = transform.GetComponentsInChildren<MeshRenderer>();
        colliders = transform.GetComponentsInChildren<Collider>();
    }
    
    public void ToggleGhostRendering(bool newGhost)
    {
        foreach (var aMeshRenderer in meshRenderers)
        {
            aMeshRenderer.material = newGhost ? ghostMaterial : defaultMaterial;
        }
        
        foreach (var aCollider in colliders)
        {
            aCollider.enabled = !newGhost;
        }
    }

    public bool ValidateAttachment()
    {
        foreach (GameObject attachmentPoint in attachmentPoints)
        {
            Ray ray = new Ray(attachmentPoint.transform.position, attachmentPoint.transform.forward);
            if (!Physics.Raycast(ray, 1f))
            {
                return false;
            }
        }
        return true;
    }
}