using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Zipline : ProceduralMesh, IInteractable
{
    public ZiplinePoint startPoint;
    public ZiplinePoint endPoint;
    
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    [SerializeField] private Material ghostMaterial;
    [SerializeField] private Material defaultMaterial;

    [SerializeField] private float ziplineSize = 0.05f;
    [SerializeField] private float colliderSize = 0.75f;

    public bool isInUse;
    public bool isUserMade;

    public void Interact(GameObject interactor)
    {
        PlayerActionStack player = interactor.GetComponent<PlayerActionStack>();
        
        if (!player) return;
        if (player.dataRecord.isOnZipline) return;
        if (player.dataRecord.isInTimeTrial && isUserMade) return;
        
        player.AddZiplineAction(this);
        isInUse = true;
    }

    public void ToggleLookAt(GameObject interactor, bool newToggle)
    {
        // implement e to interact with zipline ui message
    }

    private void LoadResources()
    {
        defaultMaterial = Resources.Load<Material>("Materials/ZiplineMaterial");
        ghostMaterial = Resources.Load<Material>("Materials/ZiplineGhostMaterial");
    }

    private void OnEnable()
    {
        LoadResources();
        
        meshRenderer = GetComponent<MeshRenderer>();
    }
    
    private new void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Zipline");
        
        // Set default material
        if (meshRenderer && defaultMaterial)
        {
            meshRenderer.material = defaultMaterial;
        }
    }

    protected override Mesh CreateMesh()
    {
        if (startPoint is null || endPoint is null) return null;
        
        Mesh mesh = new Mesh();
        mesh.hideFlags = HideFlags.DontSave;
        mesh.name = "Zipline";

        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> zipTriangles = new List<int>();
        List<int> colTriangles = new List<int>();
        
        Vector3 sP = startPoint.AttachLocation; //startPoint
        Vector3 eP = endPoint.AttachLocation;   //endPoint
        
        Vector3 direction = (eP - sP).normalized;
        Vector3 rightVector = -Vector3.Cross(direction, Vector3.up);
        Vector3 upVector = Vector3.Cross(direction, rightVector);

        float meshSize = ziplineSize;
        float colSize = colliderSize;
        
        vertices.AddRange(new Vector3[]
        {
            // wire mesh
            sP - rightVector * meshSize - upVector * meshSize,  //0
            sP - rightVector * meshSize + upVector * meshSize,  //1
            sP + rightVector * meshSize + upVector * meshSize,  //2
            sP + rightVector * meshSize - upVector * meshSize,  //3
            
            eP - rightVector * meshSize - upVector * meshSize,  //4
            eP - rightVector * meshSize + upVector * meshSize,  //5
            eP + rightVector * meshSize + upVector * meshSize,  //6
            eP + rightVector * meshSize - upVector * meshSize,  //7
            
            // collision mesh
            sP - rightVector * colSize - upVector * colSize,    //8
            sP - rightVector * colSize + upVector * colSize,    //9
            sP + rightVector * colSize + upVector * colSize,    //10
            sP + rightVector * colSize - upVector * colSize,    //11
            
            eP - rightVector * colSize - upVector * colSize,    //12
            eP - rightVector * colSize + upVector * colSize,    //13
            eP + rightVector * colSize + upVector * colSize,    //14
            eP + rightVector * colSize - upVector * colSize     //15
        });
        
        colors.AddRange(new Color[vertices.Count]);
        
        for (int i = 0; i < vertices.Count; i++) 
            colors[i] = Color.red;
        
        zipTriangles.AddRange(new int[]
        {
            //0, 1, 3, 3, 1, 2, //front
            4, 5, 1, 1, 0, 4,   //left
            1, 5, 2, 2, 5, 6,   //top
            3, 2, 7, 7, 2, 6,   //right
            0, 3, 4, 3, 7, 4    //bottom
            //6, 5, 7, 7, 5, 4  //back
        });
        
        colTriangles.AddRange(new int[]
        {
            //8, 9, 11, 11, 9, 10,      //front
            12, 13, 9, 9, 8, 12,        //left
            9, 13, 10, 10, 13, 14,      //top
            11, 10, 15, 15, 10, 14,     //right
            8, 11, 12, 11, 15, 12       //bottom
            //14, 13, 15, 15, 13, 12    //back
        });
        
        // assign the mesh data
        mesh.vertices = vertices.ToArray();
        mesh.colors = colors.ToArray();
        
        mesh.subMeshCount = 2;
        mesh.SetTriangles(zipTriangles, 0);
        mesh.SetTriangles(colTriangles, 1);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (!meshCollider)
        {
            if (!TryGetComponent(out meshCollider))
            {
                meshCollider = gameObject.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.isTrigger = true;
            }
        }
        
        meshCollider.sharedMesh = mesh;
        return mesh;
    }

    private void Update()
    {
        #if UNITY_EDITOR
        if (endPoint is null || startPoint is null) return;
        
        if (endPoint.transform.hasChanged || startPoint.transform.hasChanged)
        {
            UpdateMesh();
        }
        #endif
    }

    public void ToggleGhostRendering(bool newGhost)
    {
        if (meshCollider) meshCollider.enabled = !newGhost;
        meshRenderer.material = newGhost ? ghostMaterial : defaultMaterial;
        
        startPoint?.ToggleGhostRendering(newGhost);
        endPoint?.ToggleGhostRendering(newGhost);
    }

    public void ToggleGhostRenderingExclusive(bool newGhost)
    {
        if (meshCollider) meshCollider.enabled = !newGhost;
        meshRenderer.material = newGhost ? ghostMaterial : defaultMaterial;
    }

    public void DeleteZipline()
    {
        Destroy(startPoint);
        Destroy(endPoint);
        Destroy(gameObject);
    }

    public Vector3 GetClosestPointOnZipline(Vector3 position)
    {
        // found online, closest point on line segment
        Vector3 directionToPosition = position - startPoint.AttachLocation;
        Vector3 ziplineDirection = endPoint.AttachLocation - startPoint.AttachLocation;
        
        float squaredMagnitude = ziplineDirection.sqrMagnitude;
        
        float dotProduct = Vector3.Dot(directionToPosition, ziplineDirection);
        
        float normalizedDistance = dotProduct / squaredMagnitude;
        
        Vector3 closestPoint = startPoint.AttachLocation + ziplineDirection * normalizedDistance;
        return closestPoint;
    }

    public Vector3 GetZiplineDirection()
    {
        if (Mathf.Approximately(startPoint.AttachLocation.y, endPoint.AttachLocation.y)) return Vector3.zero;
        return GetZiplineDirectionNonZero();
    }

    public Vector3 GetZiplineDirectionNonZero()
    {
        Vector3 highestPoint, lowestPoint;

        if (startPoint.AttachLocation.y > endPoint.AttachLocation.y)
        {
            highestPoint = startPoint.AttachLocation;
            lowestPoint = endPoint.AttachLocation;
        }
        else
        {
            highestPoint = endPoint.AttachLocation;
            lowestPoint = startPoint.AttachLocation;
        }
        
        return (highestPoint - lowestPoint).normalized;
    }

    public bool IsPointOnZipline(Vector3 point)
    {
        // found this calculation online, and it worked but I don't really understand how it works to be honest...
        Vector3 ziplineDirection = endPoint.AttachLocation - startPoint.AttachLocation;
        Vector3 pointDirection = point - startPoint.AttachLocation;

        float dot = Vector3.Dot(ziplineDirection, pointDirection);
        if (dot < 0) return false;

        if (dot > ziplineDirection.sqrMagnitude) return false;
        return true;
    }
}