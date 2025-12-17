using UnityEngine;

public class ZiplinePoint : MonoBehaviour
{
    [SerializeField] private GameObject attachObject;

    private MeshRenderer _meshRenderer;
    private Collider _collider;

    [SerializeField] private Material ghostMaterial;
    [SerializeField] private Material defaultMaterial;

    #region Properties
    public Vector3 AttachLocation => attachObject.transform.position;
    public GameObject OwnerObject => transform.parent.gameObject;
    public Zipline Owner => OwnerObject.GetComponent<Zipline>();
    #endregion
    
    private void OnEnable()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<Collider>();
    }
    
    public void ToggleGhostRendering(bool newGhost)
    {
        _collider.enabled = !newGhost;
        _meshRenderer.material = newGhost ? ghostMaterial : defaultMaterial;
    }
}