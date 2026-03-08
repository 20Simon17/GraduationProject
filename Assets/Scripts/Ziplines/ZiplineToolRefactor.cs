using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.VirtualTexturing;

public class ZiplineToolRefactor : MonoBehaviour
{
    #region Prefabs
    private GameObject polePrefab;
    private GameObject wallPrefab;
    private GameObject ceilingPrefab;
    #endregion
    
    private Transform playerCamera;
    private PlayerActionStack player;

    private GameObject userZiplinesObject;

    public int maxZiplines = 3;
    public int currentZiplines = 0;
    public List<Zipline> ziplines;
    
    public bool bShowGhost = true;
    private bool bIsPlacing;
    private bool bIsMoving;
    
    private ZiplinePoint selectedPoint;
    private Zipline zipline;
    
    private Vector3 savedSelectedPosition;
    private Quaternion savedSelectedRotation;

    public LayerMask layerMaskIncludeZiplines;
    public LayerMask layerMaskExcludeZiplines;

    public float rotationStepDegrees = 5f;

    public float minZiplineDistance = 2f;
    
    private bool gameIsQuitting;

    private bool placingFirstPoint;
    
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
        userZiplinesObject = GameObject.Find("UserZiplines");
        
        Application.quitting += QuitGame;
        InputManager.Instance.OnPrimaryActionEvent += PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent += SecondaryAction;
        InputManager.Instance.OnScrollEvent += HandleRotation;
    }

    private void OnDisable()
    {
        Application.quitting -= QuitGame;
        if (gameIsQuitting) return;
        
        InputManager.Instance.OnPrimaryActionEvent -= PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent -= SecondaryAction;
        InputManager.Instance.OnScrollEvent -= HandleRotation;
    }
    
    private void QuitGame() => gameIsQuitting = true;
    
    private void FixedUpdate()
    {
        RaycastHit? rayHit = GetLookAtHit(false);

        if (rayHit is null) return;
        RaycastHit hit = rayHit.Value;
        
        if (bShowGhost && selectedPoint)
        {
            selectedPoint = ReplaceZiplinePoint(selectedPoint, hit.normal);
            
            selectedPoint.transform.position = hit.point;
            //selectedPoint.transform.forward = GetSuitableRotation(player.transform.forward);
        }
    }
    
    private RaycastHit? GetLookAtHit(bool includeZipline = true)
    {
        LayerMask layerMask = includeZipline ? layerMaskIncludeZiplines : layerMaskExcludeZiplines;
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 100, layerMask)) return hit;
        else return null;
    }

    private void HandlePrimaryAction()
    {
        RaycastHit? checkHit = GetLookAtHit(!bIsMoving || bIsPlacing);
        if (!checkHit.HasValue) return;
            
        RaycastHit hit = checkHit.Value;

        // if we are moving a zipline, finalize it
        if (bIsMoving)
        {
            HandleMovement(true);
            return;
        }
        //otherwise, if we looked at a zipline point, select it and move it
        if (!bIsPlacing && hit.transform.parent.TryGetComponent(out ZiplinePoint ziplinePoint))
        {
            if (ziplinePoint.Owner.isUserMade && !ziplinePoint.Owner.isInUse)
            {
                HandleMovement(false, ziplinePoint);
            }
            return;
        }
            
        // if we're not already placing a zipline, start placing one
        if (!bIsPlacing)
        {
            bIsPlacing = true;
            zipline = CreateZipline();
        }

        HandlePlacement(hit.point, hit.normal, zipline);
    }

    private void HandleSecondaryAction()
    {
        if (bIsMoving || bIsPlacing)
        {
            Cancel();
            return;
        }
        
        RaycastHit? checkHit = GetLookAtHit();
        if (!checkHit.HasValue) return;
            
        RaycastHit hit = checkHit.Value;
        if (hit.transform.parent.TryGetComponent(out ZiplinePoint ziplinePoint))
        {
            if (ziplinePoint.Owner.isUserMade && !ziplinePoint.Owner.isInUse)
            {
                ziplinePoint.Owner.DeleteZipline();
            }
        }
    }

    private void HandlePlacement(Vector3 position, Vector3 normal, Zipline owner)
    {
        ZiplinePoint ziplinePoint = selectedPoint ? selectedPoint :
            CreateZiplinePoint(position, normal, owner);

        if (owner.startPoint is null)
        {
            owner.startPoint = ziplinePoint;
                
            selectedPoint = CreateZiplinePoint(position, normal, owner);
            owner.endPoint = selectedPoint;
                
            owner.ToggleGhostRendering(true);
        }
        else
        {
            owner.endPoint = ziplinePoint;
            owner.ToggleGhostRendering(false);
            
            AddZipline(zipline);
                
            selectedPoint = null;
            bIsPlacing = false;
        }
    }

    private void HandleMovement(bool finishedMovement, ZiplinePoint movedPoint = null)
    {
        bIsMoving = !finishedMovement;

        if (movedPoint)
        {
            selectedPoint = movedPoint;
        }

        selectedPoint.ToggleGhostRendering(!finishedMovement);
        if (finishedMovement)
        {
            selectedPoint = null;
        }
        else
        {
            savedSelectedPosition = selectedPoint.transform.position;
            savedSelectedRotation = selectedPoint.transform.rotation;
        }
    }
    
    private void HandleRotation(InputValue value)
    {
        if (!selectedPoint || selectedPoint.pointType == ZiplinePoint.EPointTypes.Wall) return;


        //int inputFactor = value.Get<float>() > 0 ? 1 : -1;
        int inputFactor = (int) value.Get<float>();
        selectedPoint.transform.eulerAngles += new Vector3(0, rotationStepDegrees * inputFactor, 0);
    }

    private Zipline CreateZipline()
    {
        GameObject go = new GameObject { name = "Zipline" };
        Zipline ziplineComponent = go.AddComponent<Zipline>();
        go.transform.parent = userZiplinesObject.transform;
        
        ziplineComponent.startPoint = null;
        ziplineComponent.endPoint = null;
        ziplineComponent.isUserMade = true;
        
        ziplineComponent.ToggleGhostRendering(true);
        
        return ziplineComponent;
    }

    private void AddZipline(Zipline inZipline)
    {
        if (ziplines.Count == maxZiplines)
        {
            ziplines[0].DeleteZipline();
            ziplines.RemoveAt(0);
        }
        
        ziplines.Add(inZipline);
        currentZiplines = ziplines.Count;
    }

    private void Cancel()
    {
        if (bIsMoving)
        {
            bIsMoving = false;
            
            selectedPoint.transform.position = savedSelectedPosition;
            selectedPoint.transform.rotation = savedSelectedRotation;
            selectedPoint.Owner.ToggleGhostRendering(false);
        }
        else if (bIsPlacing)
        {
            bIsPlacing = false;
            
            zipline.DeleteZipline();
            zipline = null;
        }
        
        selectedPoint = null;
    }

    private ZiplinePoint CreateZiplinePoint(Vector3 inPosition, Vector3 inNormal, Zipline owner)
    {
        if (!owner) return null;
        
        GameObject go;
        ZiplinePoint.EPointTypes pointType;
        if (inNormal == Vector3.down)
        {
            go = Instantiate(ceilingPrefab, owner.transform);
            go.transform.up = -inNormal;
            pointType = ZiplinePoint.EPointTypes.Ceiling;
        }
        else if (inNormal == Vector3.forward || inNormal == Vector3.back ||
                 inNormal == Vector3.right   || inNormal == Vector3.left)
        {
            go = Instantiate(wallPrefab, owner.transform);
            pointType = ZiplinePoint.EPointTypes.Wall;
        }
        else
        {
            go = Instantiate(polePrefab, owner.transform);
            go.transform.up = inNormal;
            pointType = ZiplinePoint.EPointTypes.Pole;
        }
        
        go.transform.position = inPosition;
        
        ZiplinePoint ziplinePoint = go.GetComponent<ZiplinePoint>();
        ziplinePoint.pointType = pointType;
        ziplinePoint.enabled = false;
        
        return ziplinePoint;
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

    private void PrimaryAction(InputValue value)
    {
        if (!value.isPressed) return;
        HandlePrimaryAction();
    }

    private void SecondaryAction(InputValue value)
    {
        if (!value.isPressed) return;
        HandleSecondaryAction();
    }
}