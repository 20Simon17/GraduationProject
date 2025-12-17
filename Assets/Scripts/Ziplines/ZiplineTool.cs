using UnityEngine;
using UnityEngine.InputSystem;

public class ZiplineTool : MonoBehaviour
{
    #region Prefabs
    private GameObject _polePrefab;
    private GameObject _wallPrefab;
    private GameObject _ceilingPrefab;
    #endregion
    
    private PlayerCamera _playerCamera;
    private Player _player;
    
    private ZiplinePoint _selectedPoint;
    private bool _isPlacingZipline = false;
    private GameObject _ziplineObject;
    private Zipline _zipline;
    private ZiplinePoint _placementPoint;

    private void LoadResources()
    {
        _polePrefab = Resources.Load<GameObject>("Prefabs/ZiplinePoints/ZiplinePole");
        _wallPrefab = Resources.Load<GameObject>("Prefabs/ZiplinePoints/ZiplineWall");
        _ceilingPrefab = Resources.Load<GameObject>("Prefabs/ZiplinePoints/ZiplineCeiling");
    }
    
    private void OnEnable()
    {
        LoadResources();
        _playerCamera = FindFirstObjectByType<PlayerCamera>();
        _player = FindFirstObjectByType<Player>();
        
        InputManager.Instance.OnPrimaryActionEvent += PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent += SecondaryAction;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnPrimaryActionEvent -= PrimaryAction;
        InputManager.Instance.OnSecondaryActionEvent -= SecondaryAction;
        
        CancelPlacement();
    }

    private void FixedUpdate()
    {
        RaycastHit? rayHit = _playerCamera.GetLookAtHit();

        if (rayHit is null) return;
        RaycastHit hit = rayHit.Value;
        
        if (_isPlacingZipline)
        {
            //_placementPoint.transform.position = hit.point;
        }
        else if (_selectedPoint is not null)
        {
            _selectedPoint.transform.position = hit.point;
        }
    }

    private void PrimaryAction(InputValue value)
    {
        RaycastHit? rayHit = _playerCamera.GetLookAtHit();

        if (rayHit is null) return;
        RaycastHit hit = rayHit.Value;
        
        ZiplinePoint zipPoint = hit.transform.GetComponent<ZiplinePoint>();

        if (_selectedPoint is not null)
        {
            _selectedPoint.transform.position = hit.transform.position;
            _selectedPoint = null;
            return;
        }

        if (zipPoint is not null && !_isPlacingZipline)
        {
            _selectedPoint = zipPoint;
            zipPoint.ToggleGhostRendering(true);
            zipPoint.Owner.ToggleZiplineGhostRendering(true);
            return;
        }

        _isPlacingZipline = true;
        if (_ziplineObject is null)
        {
            _ziplineObject = new GameObject { name = "Zipline" };
            _zipline = _ziplineObject.AddComponent<Zipline>();
        }
        
        CreateZiplinePoint(hit.point, hit.normal, _zipline);
        
        // TODO: when a zipline has been selected for relocation or is being placed, display a "ghost" zipline object everywhere the player is looking
        // for example: create the actual zipline, turn off collision and set material to semi transparent material, fix upon placing
        
        CheckPlacement();
    }

    private void SecondaryAction(InputValue value)
    {
        
    }

    private void CancelPlacement()
    {
        _selectedPoint.Owner.ToggleZiplineGhostRendering(false);
        _selectedPoint.ToggleGhostRendering(false);
        _selectedPoint = null;

        if (!_isPlacingZipline) return;
        
        _isPlacingZipline = false;
        Destroy(_ziplineObject);
    }

    private void CheckPlacement()
    {
        if (_zipline.endPoint is null || !ValidatePlacement()) return;
        
        _isPlacingZipline = false;
        _ziplineObject = null;
        _zipline = null;
        _placementPoint = null;
    }

    private bool ValidatePlacement()
    {
        //check if the zipline intersects any building anywhere
        //maybe check if there's "enough" room for the player on the zipline to not hit objects beneath it
        return true;
    }

    private void CreateZiplinePoint(Vector3 inPosition, Vector3 inNormal, Zipline ownerZipline)
    {
        if (ownerZipline is null) return;
        
        GameObject zipPointObject = null;
        
        if (inNormal == Vector3.down)
        {
            zipPointObject = Instantiate(_ceilingPrefab);
            zipPointObject.transform.up = -inNormal;
        }
        else if (inNormal == Vector3.forward || inNormal == Vector3.back ||
                 inNormal == Vector3.right   || inNormal == Vector3.left)
        {
            zipPointObject = Instantiate(_wallPrefab);
        }
        else
        {
            zipPointObject = Instantiate(_polePrefab);
            zipPointObject.transform.up = inNormal;
        }
        
        ZiplinePoint zipPoint = zipPointObject.GetComponent<ZiplinePoint>();
        zipPoint.enabled = false;

        if (ownerZipline.startPoint is null) ownerZipline.startPoint = zipPoint;
        else if (ownerZipline.endPoint is null) ownerZipline.endPoint = zipPoint;
        
        zipPointObject.transform.position = inPosition; // TODO: Might need to add an offset function to the zipline points so they can handle being put in the correct spot by themselves
        //zipPointObject.transform.forward = GetSuitableRotation(_player.transform.eulerAngles);
        
    }

    private Vector3 GetSuitableRotation(Vector3 inRotation)
    {
        Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.up, Vector3.left };

        for (int i = 0; i < directions.Length; i++)
        {
            float dot = Vector3.Dot(inRotation, directions[i]);
            if (dot < 0) continue;
            
            if (dot < 0.5)
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
}