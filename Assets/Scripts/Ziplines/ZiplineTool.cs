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
    
    private PlayerCamera playerCamera;
    private Player player;

    //TODO: make use of these
    public int maxZiplines = 3;
    public int currentZiplines = 0;
    public List<Zipline> ziplines;
    
    public bool bShowGhost = true;
    private bool bIsPlacing;
    private bool bIsMoving;
    
    private ZiplinePoint selectedPoint;
    private GameObject _ziplineObject;
    private Zipline _zipline;
    private ZiplinePoint _placementPoint;

    private void LoadResources()
    {
        polePrefab = Resources.Load<GameObject>("Prefabs/ZiplinePoints/ZiplinePole");
        wallPrefab = Resources.Load<GameObject>("Prefabs/ZiplinePoints/ZiplineWall");
        ceilingPrefab = Resources.Load<GameObject>("Prefabs/ZiplinePoints/ZiplineCeiling");
    }
    
    private void OnEnable()
    {
        LoadResources();
        playerCamera = FindFirstObjectByType<PlayerCamera>();
        player = FindFirstObjectByType<Player>();
        
        InputManager.Instance.OnPrimaryActionEvent += PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent += SecondaryAction;
        
        ResetValues();
    }

    private void OnDisable()
    {
        InputManager.Instance.OnPrimaryActionEvent -= PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent -= SecondaryAction;
        
        CancelPlacement();
    }

    private void FixedUpdate()
    {
        RaycastHit? rayHit = playerCamera.GetLookAtHit();

        if (rayHit is null) return;
        RaycastHit hit = rayHit.Value;
        
        if (bShowGhost && bIsMoving)
        {
            selectedPoint = ReplaceZiplinePoint(selectedPoint, hit.normal);
            
            selectedPoint.transform.position = hit.point;
            selectedPoint.transform.forward = GetSuitableRotation(player.transform.forward);
        }
    }

    private void PrimaryAction(InputValue value)
    {
        if (!value.isPressed) return;

        RaycastHit? rayHit = playerCamera.GetLookAtHit();

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
        if (_ziplineObject is null)
        {
            _ziplineObject = new GameObject { name = "Zipline" };
            _zipline = _ziplineObject.AddComponent<Zipline>();
        }
        
        CreateZiplinePoint(hit.point, hit.normal, _zipline);
        CheckPlacement();
    }

    private void SecondaryAction(InputValue value)
    {
        if (!value.isPressed) return;

        RaycastHit? rayHit = playerCamera.GetLookAtHit();

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
        if (selectedPoint == null) return;
        //selectedPoint.Owner.ToggleGhostRendering(false); // this is also called from the zipline point's ToggleGhostRendering()
        selectedPoint.ToggleGhostRendering(false);
        selectedPoint = null;

        if (!bIsPlacing) return;
        
        bIsPlacing = false;
        Destroy(_ziplineObject);
    }

    private void CheckPlacement()
    {
        if (_zipline.endPoint is null || !ValidatePlacement()) return;
        
        if (ziplines.Count == maxZiplines)
        {
            ziplines[0].DeleteZipline();
            ziplines.RemoveAt(0);
        }
        
        ziplines.Add(_zipline);
        currentZiplines = ziplines.Count;
        
        ResetValues();
    }

    private bool ValidatePlacement()
    {
        //check if the zipline intersects any building anywhere
        //maybe check if there's "enough" room for the player on the zipline to not hit objects beneath it
        return true;
    }

    private void ResetValues()
    {
        bIsPlacing = false;
        _ziplineObject = null;
        _zipline = null;
        _placementPoint = null;
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

        if (ownerZipline.startPoint is null) ownerZipline.startPoint = zipPoint;
        else if (ownerZipline.endPoint is null) ownerZipline.endPoint = zipPoint;
        
        zipPointObject.transform.position = inPosition; // TODO: Might need to add an offset function to the zipline points so they can handle being put in the correct spot by themselves
        zipPointObject.transform.forward = GetSuitableRotation(player.transform.forward);
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
        foreach (var zipline in ziplines)
        {
            zipline.DeleteZipline();
        }
        ziplines.Clear();

        currentZiplines = 0;
    }
}