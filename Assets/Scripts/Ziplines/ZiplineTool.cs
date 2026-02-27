using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ZiplineTool : MonoBehaviour
{
    #region Prefabs
    private GameObject polePrefab;
    private GameObject wallPrefab;
    private GameObject ceilingPrefab;
    #endregion
    
    private Transform playerCamera;
    private PlayerActionStack player;

    public int maxZiplines = 3;
    public int currentZiplines = 0;
    public List<Zipline> ziplines;
    
    public bool bShowGhost = true;
    private bool bIsPlacing;
    private bool bIsMoving;
    
    private ZiplinePoint selectedPoint;
    private GameObject ziplineObject;
    private Zipline zipline;
    //private ZiplinePoint _placementPoint;

    public LayerMask layerMask;
    
    private bool gameIsQuitting;
    
    private void LoadResources()
    {
        //TODO: Replace this with Addressables system
        polePrefab = Resources.Load<GameObject>("Prefabs/ZiplinePoints/ZiplinePole");
        wallPrefab = Resources.Load<GameObject>("Prefabs/ZiplinePoints/ZiplineWall");
        ceilingPrefab = Resources.Load<GameObject>("Prefabs/ZiplinePoints/ZiplineCeiling");
    }
    
    private void OnEnable()
    {
        LoadResources();
        playerCamera = FindFirstObjectByType<CameraActionStack>().transform;
        player = FindFirstObjectByType<PlayerActionStack>();
        
        Application.quitting += QuitGame;
        InputManager.Instance.OnPrimaryActionEvent += PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent += SecondaryAction;
        
        ResetValues();
    }

    private void OnDisable()
    {
        CancelPlacement();
        
        Application.quitting -= QuitGame;
        if (gameIsQuitting) return;
        
        InputManager.Instance.OnPrimaryActionEvent -= PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent -= SecondaryAction;
    }
    
    private void QuitGame() => gameIsQuitting = true;
    
    private void FixedUpdate()
    {
        RaycastHit? rayHit = GetLookAtHit();

        if (rayHit is null) return;
        RaycastHit hit = rayHit.Value;
        
        if (bShowGhost && bIsMoving)
        {
            selectedPoint = ReplaceZiplinePoint(selectedPoint, hit.normal);
            
            selectedPoint.transform.position = hit.point;
            selectedPoint.transform.forward = GetSuitableRotation(player.transform.forward);
        }
    }
    
    private RaycastHit? GetLookAtHit()
    {
        
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 100, layerMask)) return hit;
        else return null;
    }

    private void PrimaryAction(InputValue value)
    {
        if (!value.isPressed) return;

        RaycastHit? rayHit = GetLookAtHit();

        if (rayHit is null) return;
        RaycastHit hit = rayHit.Value;

        ZiplinePoint zipPoint = hit.transform.parent?.GetComponent<ZiplinePoint>();

        // If you're moving a point, place it down
        if (selectedPoint is not null) 
        {
            bIsMoving = false;
            selectedPoint.transform.position = hit.point;
            selectedPoint.ToggleGhostRendering(false);
            selectedPoint = null;
            return;
        }
        
        // Select the point you're looking at to be moved
        if (zipPoint is not null && !bIsPlacing) 
        { 
            bIsMoving = true;
            selectedPoint = zipPoint;
            zipPoint.ToggleGhostRendering(true);
            return;
        }

        bIsPlacing = true;
        if (ziplineObject is null)
        {
            ziplineObject = new GameObject { name = "Zipline" };
            zipline = ziplineObject.AddComponent<Zipline>();
            zipline.startPoint = null;
            zipline.endPoint = null;
        }
        
        CreateZiplinePoint(hit.point, hit.normal, zipline);
        CheckPlacement();
    }

    private void SecondaryAction(InputValue value)
    {
        if (!value.isPressed) return;

        ClearAllZiplines();
        return;
        RaycastHit? rayHit = GetLookAtHit();

        if (rayHit is null) return;
        RaycastHit hit = rayHit.Value;

        ZiplinePoint zipPoint = hit.transform.parent?.GetComponent<ZiplinePoint>();

        if (zipPoint is not null && !bIsPlacing)
        {
            zipPoint.Owner.DeleteZipline();
        }
        else if (bIsMoving)
        {
            //cancel movement
        }
        else CancelPlacement();
    }

    private void CancelPlacement()
    {
        if (selectedPoint is null) return;
        
        selectedPoint.ToggleGhostRendering(false);
        selectedPoint = null;
        
        if (!bIsPlacing) return;
        
        bIsPlacing = false;
        Destroy(ziplineObject);
    }

    private void CheckPlacement()
    {
        if (zipline.endPoint is null || !ValidatePlacement()) return;
        
        if (ziplines.Count == maxZiplines)
        {
            ziplines[0].DeleteZipline();
            ziplines.RemoveAt(0);
        }
        
        ziplines.Add(zipline);
        currentZiplines = ziplines.Count;
        
        ResetValues();
    }

    private bool ValidatePlacement()
    {
        //TODO: Make zipline placement be invalid if they are colliding with an existing one
        //check if the zipline intersects any building anywhere
        //maybe check if there's "enough" room for the player on the zipline to not hit objects beneath it
        //display the error as a message on some ui element to allow the player to correct it without having to redo everything
        return true;
    }

    private void ResetValues()
    {
        bIsPlacing = false;
        ziplineObject = null;
        zipline = null;
        //_placementPoint = null;
    }

    private void CreateZiplinePoint(Vector3 inPosition, Vector3 inNormal, Zipline ownerZipline)
    {
        if (ownerZipline is null) return;
        
        GameObject zipPointObject = null;

        ZiplinePoint.EPointTypes pointType;
        
        if (inNormal == Vector3.down)
        {
            zipPointObject = Instantiate(ceilingPrefab);
            zipPointObject.transform.up = -inNormal;
            pointType = ZiplinePoint.EPointTypes.Ceiling;
        }
        else if (inNormal == Vector3.forward || inNormal == Vector3.back ||
                 inNormal == Vector3.right   || inNormal == Vector3.left)
        {
            zipPointObject = Instantiate(wallPrefab);
            pointType = ZiplinePoint.EPointTypes.Wall;
        }
        else
        {
            zipPointObject = Instantiate(polePrefab);
            zipPointObject.transform.up = inNormal;
            pointType = ZiplinePoint.EPointTypes.Pole;
        }
        
        zipPointObject.transform.parent = ownerZipline.transform;
        
        ZiplinePoint zipPoint = zipPointObject.GetComponent<ZiplinePoint>();
        zipPoint.pointType = pointType;
        zipPoint.enabled = false;
        
        zipPointObject.transform.position = inPosition; // TODO: Might need to add an offset function to the zipline points so they can handle being put in the correct spot by themselves
        zipPointObject.transform.forward = GetSuitableRotation(player.transform.forward);

        if (ownerZipline.startPoint is null) ownerZipline.startPoint = zipPoint;
        else if (ownerZipline.endPoint is null)
        {
            ownerZipline.endPoint = zipPoint;
            
            Debug.LogError("Trying to update zipline mesh");
            ownerZipline.UpdateMesh();
        }
        
        zipPoint.ToggleGhostRendering(false);
    }

    private ZiplinePoint ReplaceZiplinePoint(ZiplinePoint inZiplinePoint, Vector3 inNormal)
    {
        switch (inZiplinePoint.pointType)
        {
            case ZiplinePoint.EPointTypes.Pole:
                if (inNormal == Vector3.up) return inZiplinePoint;
                break;
            case ZiplinePoint.EPointTypes.Wall:
                if (inNormal == Vector3.forward || inNormal == Vector3.back ||
                    inNormal == Vector3.right   || inNormal == Vector3.left) return inZiplinePoint;
                break;
            case ZiplinePoint.EPointTypes.Ceiling:
                if (inNormal == Vector3.down) return inZiplinePoint;
                break;
        }
        
        ZiplinePoint newZiplinePoint;
        if (inNormal == Vector3.up)
        {
            newZiplinePoint = Instantiate(polePrefab).GetComponent<ZiplinePoint>();
            newZiplinePoint.pointType = ZiplinePoint.EPointTypes.Pole;
        }
        else if (inNormal == Vector3.forward || inNormal == Vector3.back ||
                 inNormal == Vector3.right   || inNormal == Vector3.left)
        {
            newZiplinePoint = Instantiate(wallPrefab).GetComponent<ZiplinePoint>();
            newZiplinePoint.pointType = ZiplinePoint.EPointTypes.Wall;
        }
        else if (inNormal == Vector3.down)
        {
            newZiplinePoint =
                Instantiate(ceilingPrefab).GetComponent<ZiplinePoint>();
            newZiplinePoint.pointType = ZiplinePoint.EPointTypes.Ceiling;
        }
        else return inZiplinePoint;
        
        newZiplinePoint.transform.parent = inZiplinePoint.OwnerObject.transform;
        newZiplinePoint.enabled = false;
        
        newZiplinePoint.ToggleGhostRendering(true);
        
        Zipline ownerZipline = inZiplinePoint.Owner;
        if (ownerZipline.startPoint == inZiplinePoint) ownerZipline.startPoint = newZiplinePoint;
        else if (ownerZipline.endPoint == inZiplinePoint) ownerZipline.endPoint = newZiplinePoint;
        
        Destroy(inZiplinePoint.gameObject);
        Destroy(inZiplinePoint);
        
        return newZiplinePoint;
    }

    private Vector3 GetSuitableRotation(Vector3 inRotation)
    {
        Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };

        for (int i = 0; i < directions.Length; i++)
        {
            float dot = Vector3.Dot(inRotation, directions[i]);
            if (dot < 0) continue;
            
            if (dot < 0.75)
            {
                int leftIndex = (i + directions.Length - 1) % directions.Length;
                int rightIndex = (i + 1) % directions.Length;
                
                float leftDot = Vector3.Dot(inRotation, directions[leftIndex]);
                if (leftDot > 0 && leftDot < 0.5)
                {
                    Vector3 newDirection = directions[i] + directions[leftIndex];
                    return newDirection.normalized;
                }
                
                float rightDot = Vector3.Dot(inRotation, directions[rightIndex]);
                if (rightDot > 0 && rightDot < 0.5)
                {
                    Vector3 newDirection = directions[i] + directions[rightIndex];
                    return newDirection.normalized;
                }
            }
            else return directions[i];
        }
        
        return Vector3.zero;
    }

    private void ClearAllZiplines()
    {
        foreach (var aZipline in ziplines)
        {
            aZipline.DeleteZipline();
        }
        
        ziplines.Clear();
        currentZiplines = 0;
    }
}