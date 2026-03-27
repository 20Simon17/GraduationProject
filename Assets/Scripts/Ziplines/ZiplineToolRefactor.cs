using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ZiplineToolRefactor : ItemBase
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
    [SerializeField] private bool bIsPlacing;
    [SerializeField] private bool bIsMoving;
    
    [SerializeField] private ZiplinePoint selectedPoint;
    [SerializeField] private Zipline zipline;
    
    private Vector3 savedSelectedPosition;
    private Quaternion savedSelectedRotation;

    public LayerMask layerMaskIncludeZiplines;
    public LayerMask layerMaskExcludeZiplines;

    public float rotationStepDegrees = 5f;

    public float minZiplineDistance = 2f;
    
    private bool gameIsQuitting;

    private bool placingFirstPoint;

    [SerializeField] private bool lockedSelection;
    public float nudgeDistance = 0.25f;
    
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
        
        BindEvents();
    }

    private void OnDisable() => UnbindEvents();

    private void BindEvents()
    {
        Application.quitting += QuitGame;
        InputManager.Instance.OnPrimaryActionEvent += PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent += SecondaryAction;
        InputManager.Instance.OnScrollEvent += HandleRotation;
        InputManager.Instance.OnLockEvent += LockPlacement;
        InputManager.Instance.OnAltMoveEvent += MovePlacement;
    }

    private void UnbindEvents()
    {
        Application.quitting -= QuitGame;
        if (gameIsQuitting) return;
        
        InputManager.Instance.OnPrimaryActionEvent -= PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent -= SecondaryAction;
        InputManager.Instance.OnScrollEvent -= HandleRotation;
        InputManager.Instance.OnLockEvent -= LockPlacement;
        InputManager.Instance.OnAltMoveEvent -= MovePlacement;
    }
    
    private void QuitGame() => gameIsQuitting = true;
    
    public override void EquipItem()
    {
        base.EquipItem();
        BindEvents();
        gameObject.SetActive(true);
    }

    public override void UnequipItem()
    {
        base.UnequipItem();
        UnbindEvents();
        gameObject.SetActive(false);
    }
    
    private void FixedUpdate()
    {
        if (lockedSelection) return;
        RaycastHit? rayHit = GetLookAtHit(false);

        if (rayHit is null) return;
        RaycastHit hit = rayHit.Value;
        
        if (bShowGhost && selectedPoint)
        {
            selectedPoint = ReplaceZiplinePoint(selectedPoint, hit.normal);
            selectedPoint.transform.position = GetClosestGridPoint(hit.point);
            
            if (selectedPoint.ValidateAttachment())
            {
                savedSelectedPosition = selectedPoint.transform.position;
            }
            else selectedPoint.transform.position = savedSelectedPosition;
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
        if (lockedSelection)
        {
            HandlePlacement(selectedPoint.AttachLocation, Vector3.zero, zipline);
            return;
        }
        
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
            if (ziplinePoint.Owner && ziplinePoint.Owner.isUserMade && 
                !ziplinePoint.Owner.isInUse)
            {
                zipline = ziplinePoint.Owner;
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
            selectedPoint = CreateZiplinePoint(position, normal, owner);
            
            owner.startPoint = ziplinePoint;
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
            lockedSelection = false;
            zipline = null;
            savedSelectedPosition = Vector3.zero;
        }
    }

    private void HandleMovement(bool finishedMovement, ZiplinePoint movedPoint = null)
    {
        bIsMoving = !finishedMovement;

        if (movedPoint)
        {
            selectedPoint = movedPoint;
        }

        zipline.ToggleGhostRendering(!finishedMovement);
        
        if (finishedMovement)
        {
            selectedPoint = null;
            lockedSelection = false;
            zipline = null;
            savedSelectedPosition = Vector3.zero;
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

        int inputFactor = (int)value.Get<float>();
        if (inputFactor == 0) return;
        
        Vector3 savedRotation = selectedPoint.transform.eulerAngles;
        selectedPoint.transform.eulerAngles += new Vector3(0, rotationStepDegrees * inputFactor, 0);
        
        if (!selectedPoint.ValidateAttachment())
        {
            selectedPoint.transform.eulerAngles = savedRotation;
        }
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
        lockedSelection = false;
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

            go.transform.forward = player.transform.forward;
            go.transform.eulerAngles = new Vector3(
                go.transform.eulerAngles.x,
                GetClosestSnapRotation(go.transform.eulerAngles.y),
                go.transform.eulerAngles.z);
        }
        else if (inNormal == Vector3.forward || inNormal == Vector3.back ||
                 inNormal == Vector3.right   || inNormal == Vector3.left)
        {
            go = Instantiate(wallPrefab, owner.transform);
            pointType = ZiplinePoint.EPointTypes.Wall;
            go.transform.forward = -inNormal;
        }
        else
        {
            go = Instantiate(polePrefab, owner.transform);
            go.transform.up = inNormal;
            pointType = ZiplinePoint.EPointTypes.Pole;

            go.transform.forward = player.transform.forward;
            go.transform.eulerAngles = new Vector3(
                go.transform.eulerAngles.x,
                GetClosestSnapRotation(go.transform.eulerAngles.y),
                go.transform.eulerAngles.z);
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
            newZiplinePoint.transform.forward = -inNormal;
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

    private void LockPlacement(InputValue value)
    {
        if (!bIsPlacing && !bIsMoving) return;
        lockedSelection = !lockedSelection;
    }

    private void MovePlacement(InputValue value)
    {
        if ((!bIsPlacing && !bIsMoving) || !lockedSelection || !selectedPoint) return;
        
        Vector2 inputVector = value.Get<Vector2>();
        if (inputVector == Vector2.zero) return;
        
        Vector3 savedPosition = selectedPoint.transform.position;
        
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right, Vector3.up, Vector3.down };
        
        float bestForwardDot = 0f;
        float bestRightDot = 0f;
        Vector3 closestForward = Vector3.zero;
        Vector3 closestRight = Vector3.zero;

        // this might be expensive?
        foreach (Vector3 direction in directions)
        {
            float forwardDot = Vector3.Dot(player.transform.forward, direction);
            if (forwardDot > bestForwardDot)
            {
                bestForwardDot = forwardDot;
                closestForward = direction;
            }
            
            float rightDot = Vector3.Dot(player.transform.right, direction);
            if (rightDot > bestRightDot)
            {
                bestRightDot = rightDot;
                closestRight = direction;
            }
        }
        
        if (inputVector.x != 0)
        {
            // denormalize the input value to keep the point aligned no matter how you move it
            // otherwise moving on 2 axis at once would not lead to the same position as both axis separately
            int inputModifier = inputVector.x > 0 ? 1 : -1;
            selectedPoint.transform.position += closestRight * inputModifier * nudgeDistance;
        }
        if (inputVector.y != 0)
        {
            // - || -
            int inputModifier = inputVector.y > 0 ? 1 : -1;
            selectedPoint.transform.position += closestForward * inputModifier * nudgeDistance;
        }
        
        selectedPoint.transform.position = GetClosestGridPoint(selectedPoint.transform.position);

        if (!selectedPoint.ValidateAttachment())
        {
            selectedPoint.transform.position = savedPosition;
        }
    }

    private Vector3 GetClosestGridPoint(Vector3 inPosition)
    {
        SnapPositionToGrid(ref inPosition.x, inPosition.x % nudgeDistance);
        SnapPositionToGrid(ref inPosition.y, inPosition.y % nudgeDistance);
        SnapPositionToGrid(ref inPosition.z, inPosition.z % nudgeDistance);
        return inPosition;
        
        void SnapPositionToGrid(ref float inPos, float inRest)
        {
            int polarity = inPos > 0 ? 1 : -1;
            switch (inRest)
            {
                case 0: break;
                case > 0: inPos -= inRest * polarity; break;
                default: inPos += inRest * polarity; break;
            }
        }
    }

    private float GetClosestSnapRotation(float inAngle)
    {
        float rest = inAngle % rotationStepDegrees;
        inAngle += rest > 0 ? -rest : rest;
        return inAngle;
    }

    private void PrimaryAction(InputValue value)
    {
        if (!value.isPressed) return;
        Debug.Log("Primary action");
        HandlePrimaryAction();
    }

    private void SecondaryAction(InputValue value)
    {
        if (!value.isPressed) return;
        HandleSecondaryAction();
    }
}