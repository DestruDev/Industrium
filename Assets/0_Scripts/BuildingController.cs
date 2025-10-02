using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingController : MonoBehaviour
{
    [Header("Building Settings")]
    [HideInInspector] [SerializeField] private bool isBuilding = false;
    [HideInInspector] [SerializeField] private UI_Item selectedStructureItem;
    [HideInInspector] [SerializeField] private GameObject structurePreview;
    [HideInInspector] [SerializeField] private SpriteRenderer previewRenderer;
    [HideInInspector] [SerializeField] private GameObject borderParent;
    [HideInInspector] [SerializeField] private List<GameObject> borderQuads = new List<GameObject>();
    
    // Store original sorting layers for restoration
    private string originalPreviewSortingLayer;
    private int originalPreviewSortingOrder;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color placeableColor = Color.green;
    [SerializeField] private Color notPlaceableColor = Color.red;
    [SerializeField] private float previewAlpha = 0.7f;
    
    [Header("Preview Border")]
    [SerializeField] private bool showPreviewBorder = true;
    [SerializeField] private Color borderColor = Color.white;
    [SerializeField] private float borderAlpha = 0.5f;
    [SerializeField] private string previewSortingLayer = "Foreground";
    
    [Header("Border Prefabs")]
    [SerializeField] private GameObject topLeftBorderPrefab;
    [SerializeField] private GameObject topRightBorderPrefab;
    [SerializeField] private GameObject bottomLeftBorderPrefab;
    [SerializeField] private GameObject bottomRightBorderPrefab;
    
    [Header("Border Offsets")]
    [SerializeField] private Vector2 topLeftOffset = new Vector2(0.1f, 0.1f);
    [SerializeField] private Vector2 topRightOffset = new Vector2(-0.1f, 0.1f);
    [SerializeField] private Vector2 bottomLeftOffset = new Vector2(0.1f, -0.1f);
    [SerializeField] private Vector2 bottomRightOffset = new Vector2(-0.1f, -0.1f);
    
    [Header("References")]
    [SerializeField] private GridMap gridMap;
    [SerializeField] private Hotbar hotbar;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform structureContainer;
    [SerializeField] private Transform playerTransform;
    
    [Header("Player Collision")]
    [SerializeField] private int playerCollisionRadius = 1; // Extra cells around player to prevent placement
    
    [Header("Grid Highlighting")]
    [HideInInspector] [SerializeField] private GameObject gridHighlightParent;
    [HideInInspector] [SerializeField] private GameObject gridHighlightPrefab;
    [SerializeField] private Color placeableHighlightColor = new Color(0, 1, 0, 0.3f);
    [SerializeField] private Color notPlaceableHighlightColor = new Color(1, 0, 0, 0.3f);
    
    private Vector3 lastMousePosition;
    private bool isPlaceable = false;
    private List<GameObject> currentHighlights = new List<GameObject>();
    
    void Start()
    {
        // Get references if not assigned
        if (gridMap == null)
            gridMap = FindFirstObjectByType<GridMap>();
        if (hotbar == null)
            hotbar = FindFirstObjectByType<Hotbar>();
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (playerTransform == null)
            playerTransform = FindFirstObjectByType<PlayerMovement>()?.transform;
        
        // Create structure preview object
        CreateStructurePreview();
        
        // Create grid highlighting system
        CreateGridHighlighting();
    }
    
    void Update()
    {
        if (isBuilding)
        {
            HandleBuildingMode();
        }
        else
        {
            CheckForBuildingMode();
        }
    }
    
    private void CheckForBuildingMode()
    {
        // Check if a structure item is selected in hotbar
        UI_Item selectedItem = hotbar.GetSelectedItem();
        
        if (selectedItem != null && selectedItem.IsStructure())
        {
            StartBuildingMode(selectedItem);
        }
    }
    
    private void StartBuildingMode(UI_Item structureItem)
    {
        isBuilding = true;
        selectedStructureItem = structureItem;
        
        // Show structure preview
        structurePreview.SetActive(true);
        
        // Set preview sprite (don't scale - let it display at natural size)
        if (previewRenderer != null)
        {
            previewRenderer.sprite = structureItem.Image;
            
            // Don't scale the sprite - it should display at its natural size
            // The grid highlights will show how many cells it actually occupies
            previewRenderer.transform.localScale = Vector3.one;
            
            // Store original sorting layer and order
            originalPreviewSortingLayer = previewRenderer.sortingLayerName;
            originalPreviewSortingOrder = previewRenderer.sortingOrder;
            
            // Change to Foreground sorting layer during preview
            previewRenderer.sortingLayerName = previewSortingLayer;
            previewRenderer.sortingOrder = 100; // High order to ensure it's on top
        }
        
        // Create border for the structure
        CreatePreviewBorder(structureItem);
        
        Debug.Log($"Started building mode with: {structureItem.ItemName}");
    }
    
    private void StopBuildingMode()
    {
        isBuilding = false;
        selectedStructureItem = null;
        
        // Restore original sorting layer
        if (previewRenderer != null)
        {
            previewRenderer.sortingLayerName = originalPreviewSortingLayer;
            previewRenderer.sortingOrder = originalPreviewSortingOrder;
        }
        
        // Hide structure preview
        structurePreview.SetActive(false);
        
        // Clear border quads
        ClearBorderQuads();
        
        // Clear grid highlights
        ClearGridHighlights();
        
        Debug.Log("Stopped building mode");
    }
    
    private void HandleBuildingMode()
    {
        // Check if we should exit building mode
        UI_Item selectedItem = hotbar.GetSelectedItem();
        if (selectedItem == null || !selectedItem.IsStructure())
        {
            StopBuildingMode();
            return;
        }
        
        // Update preview position based on mouse
        UpdatePreviewPosition();
        
        // Check if position is placeable
        CheckPlaceability();
        
        // Update grid highlighting
        UpdateGridHighlighting();
        
        // Handle input
        HandleBuildingInput();
    }
    
    private void UpdatePreviewPosition()
    {
        if (structurePreview == null) return;
        
        // Get mouse world position
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        // Snap to grid (this gives us the grid cell position)
        Vector3 snappedPosition = SnapToGrid(mouseWorldPos);
        
        // Update preview position (no offset needed with Bottom Left pivot)
        structurePreview.transform.position = snappedPosition;
        
        // Store the grid position for validation
        lastMousePosition = SnapToGrid(mouseWorldPos);
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }
    
    private Vector3 SnapToGrid(Vector3 worldPosition)
    {
        if (gridMap == null) return worldPosition;
        
        // Convert world position to grid coordinates
        Vector2Int gridPos = gridMap.WorldToGrid(worldPosition);
        
        // Convert back to world position (this gives us the center of the grid cell)
        Vector3 gridCenter = gridMap.GridToWorld(gridPos);
        
        // Offset to get the bottom-left corner of the grid cell
        float cellSize = gridMap.GetCellSize();
        Vector3 bottomLeft = gridCenter - new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0);
        
        return bottomLeft;
    }
    
    
    private void CheckPlaceability()
    {
        if (gridMap == null || selectedStructureItem == null)
        {
            isPlaceable = false;
            return;
        }
        
        // Get grid position
        Vector2Int gridPos = gridMap.WorldToGrid(lastMousePosition);
        
        // Check if position is valid for the structure size
        isPlaceable = IsPositionValidForStructure(gridPos, selectedStructureItem);
        
        // Debug logging
        if (Time.frameCount % 60 == 0) // Log every 60 frames to avoid spam
        {
            Vector2Int structureSize = GetStructureSize(selectedStructureItem);
            Debug.Log($"GridPos: {gridPos}, StructureSize: {structureSize}, IsPlaceable: {isPlaceable}");
            Debug.Log($"CanPlaceStructure: {gridMap.CanPlaceStructure(gridPos, structureSize)}");
        }
        
        // Update preview color
        UpdatePreviewColor();
    }
    
    private bool IsPositionValidForStructure(Vector2Int gridPos, UI_Item structureItem)
    {
        // Get structure size based on subcategory
        Vector2Int structureSize = GetStructureSize(structureItem);
        
        // Check if player is in the placement area
        if (IsPlayerInPlacementArea(gridPos, structureSize))
        {
            return false;
        }
        
        // Use GridMap's built-in validation
        return gridMap.CanPlaceStructure(gridPos, structureSize);
    }
    
    private bool IsPlayerInPlacementArea(Vector2Int gridPos, Vector2Int structureSize)
    {
        if (playerTransform == null || gridMap == null) return false;
        
        // Get player's grid position
        Vector2Int playerGridPos = gridMap.WorldToGrid(playerTransform.position);
        
        // Check if any part of the structure placement area overlaps with player's collision radius
        for (int x = 0; x < structureSize.x; x++)
        {
            for (int y = 0; y < structureSize.y; y++)
            {
                Vector2Int checkPos = gridPos + new Vector2Int(x, y);
                
                // Check if this structure cell is within player's collision radius
                if (IsWithinPlayerRadius(checkPos, playerGridPos))
                {
                    return true; // Structure placement would overlap with player's collision area
                }
            }
        }
        
        return false; // Player is not in the placement area
    }
    
    private bool IsWithinPlayerRadius(Vector2Int checkPos, Vector2Int playerPos)
    {
        // Check if the position is within the player's collision radius
        int deltaX = Mathf.Abs(checkPos.x - playerPos.x);
        int deltaY = Mathf.Abs(checkPos.y - playerPos.y);
        
        // Use Manhattan distance (or you can use Euclidean distance if preferred)
        return deltaX <= playerCollisionRadius && deltaY <= playerCollisionRadius;
    }
    
    private Vector2Int GetStructureSize(UI_Item structureItem)
    {
        // Map structure subcategories to sizes
        switch (structureItem.StructureSubcategory)
        {
            case StructureSubcategory.Size1x1:
                return new Vector2Int(1, 1);
            case StructureSubcategory.Size1x2:
                return new Vector2Int(1, 2);
            case StructureSubcategory.Size2x1:
                return new Vector2Int(2, 1);
            case StructureSubcategory.Size2x2:
                return new Vector2Int(2, 2);
            case StructureSubcategory.Size2x3:
                return new Vector2Int(2, 3);
            case StructureSubcategory.Size3x1:
                return new Vector2Int(3, 1);
            case StructureSubcategory.Size3x2:
                return new Vector2Int(3, 2);
            case StructureSubcategory.Size3x3:
                return new Vector2Int(3, 3);
            default:
                return new Vector2Int(1, 1);
        }
    }
    
    private void UpdatePreviewColor()
    {
        if (previewRenderer == null) return;
        
        Color targetColor = isPlaceable ? placeableColor : notPlaceableColor;
        targetColor.a = previewAlpha;
        
        previewRenderer.color = targetColor;
        
        // Update border color
        UpdateBorderColor();
    }
    
    private void CreatePreviewBorder(UI_Item structureItem)
    {
        if (!showPreviewBorder || borderParent == null) return;
        
        // Clear existing border quads
        ClearBorderQuads();
        
        // Get structure size
        Vector2Int structureSize = GetStructureSize(structureItem);
        float cellSize = gridMap.GetCellSize();
        
        // Calculate border dimensions
        float width = structureSize.x * cellSize;
        float height = structureSize.y * cellSize;
        
        // Instantiate corner border prefabs with individual offsets
        if (topLeftBorderPrefab != null)
        {
            GameObject topLeft = Instantiate(topLeftBorderPrefab, borderParent.transform);
            topLeft.transform.localPosition = new Vector3(topLeftOffset.x, height + topLeftOffset.y, 0);
            SetBorderSortingLayer(topLeft);
            borderQuads.Add(topLeft);
        }
        
        if (topRightBorderPrefab != null)
        {
            GameObject topRight = Instantiate(topRightBorderPrefab, borderParent.transform);
            topRight.transform.localPosition = new Vector3(width + topRightOffset.x, height + topRightOffset.y, 0);
            SetBorderSortingLayer(topRight);
            borderQuads.Add(topRight);
        }
        
        if (bottomLeftBorderPrefab != null)
        {
            GameObject bottomLeft = Instantiate(bottomLeftBorderPrefab, borderParent.transform);
            bottomLeft.transform.localPosition = new Vector3(bottomLeftOffset.x, bottomLeftOffset.y, 0);
            SetBorderSortingLayer(bottomLeft);
            borderQuads.Add(bottomLeft);
        }
        
        if (bottomRightBorderPrefab != null)
        {
            GameObject bottomRight = Instantiate(bottomRightBorderPrefab, borderParent.transform);
            bottomRight.transform.localPosition = new Vector3(width + bottomRightOffset.x, bottomRightOffset.y, 0);
            SetBorderSortingLayer(bottomRight);
            borderQuads.Add(bottomRight);
        }
    }
    
    private void SetBorderSortingLayer(GameObject borderObject)
    {
        // Set all SpriteRenderers in the border object to use the preview sorting layer
        SpriteRenderer[] renderers = borderObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingLayerName = previewSortingLayer;
            renderer.sortingOrder = 99; // Just below the main preview
        }
    }
    
    private void ClearBorderQuads()
    {
        foreach (GameObject quad in borderQuads)
        {
            if (quad != null)
                DestroyImmediate(quad);
        }
        borderQuads.Clear();
    }
    
    private void UpdateBorderColor()
    {
        if (!showPreviewBorder) return;
        
        Color targetBorderColor = isPlaceable ? borderColor : notPlaceableColor;
        targetBorderColor.a = borderAlpha;
        
        foreach (GameObject borderObject in borderQuads)
        {
            if (borderObject != null)
            {
                // Update all SpriteRenderers in the border object (including children)
                SpriteRenderer[] renderers = borderObject.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer renderer in renderers)
                {
                    renderer.color = targetBorderColor;
                }
            }
        }
    }
    
    private void HandleBuildingInput()
    {
        // Left click to place structure
        if (Input.GetMouseButtonDown(0) && isPlaceable)
        {
            PlaceStructure();
        }
        
        // Right click or Escape to cancel building
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            StopBuildingMode();
        }
    }
    
    private void PlaceStructure()
    {
        if (gridMap == null || selectedStructureItem == null) return;
        
        // Check if structure item has a prefab assigned
        if (selectedStructureItem.StructurePrefab == null)
        {
            Debug.LogError($"No prefab assigned to structure item: {selectedStructureItem.ItemName}. Please assign a prefab in the UI_Item asset.");
            return;
        }
        
        Vector2Int gridPos = gridMap.WorldToGrid(lastMousePosition);
        Vector2Int structureSize = GetStructureSize(selectedStructureItem);
        
        // Place the structure using GridMap
        bool success = gridMap.PlaceStructure(gridPos, structureSize, selectedStructureItem, structureContainer);
        
        if (success)
        {
            Debug.Log($"Successfully placed {selectedStructureItem.ItemName} at grid position {gridPos} with size {structureSize}");
            
            // Continue building mode for multiple placements
            // StopBuildingMode(); // Uncomment this if you want to exit after each placement
        }
        else
        {
            Debug.LogWarning($"Failed to place {selectedStructureItem.ItemName} at grid position {gridPos}");
        }
    }
    
    private void CreateStructurePreview()
    {
        // Create preview GameObject
        structurePreview = new GameObject("StructurePreview");
        structurePreview.transform.SetParent(transform);
        
        // Add SpriteRenderer
        previewRenderer = structurePreview.AddComponent<SpriteRenderer>();
        previewRenderer.sortingOrder = 100; // Render on top
        
        // Create border parent
        borderParent = new GameObject("PreviewBorder");
        borderParent.transform.SetParent(structurePreview.transform);
        borderParent.transform.localPosition = Vector3.zero;
        
        // Initially hide the preview
        structurePreview.SetActive(false);
    }
    
    private void CreateGridHighlighting()
    {
        // Create parent object for grid highlights
        gridHighlightParent = new GameObject("GridHighlights");
        gridHighlightParent.transform.SetParent(transform);
        gridHighlightParent.transform.localPosition = Vector3.zero;
        
        // Create a simple quad prefab for highlighting
        CreateGridHighlightPrefab();
    }
    
    private void CreateGridHighlightPrefab()
    {
        // Create a simple quad for grid highlighting
        gridHighlightPrefab = GameObject.CreatePrimitive(PrimitiveType.Quad);
        gridHighlightPrefab.name = "GridHighlight";
        gridHighlightPrefab.transform.SetParent(gridHighlightParent.transform);
        
        // Remove the collider (we don't need it)
        Collider collider = gridHighlightPrefab.GetComponent<Collider>();
        if (collider != null)
            DestroyImmediate(collider);
        
        // Set up the renderer
        Renderer renderer = gridHighlightPrefab.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.sortingOrder = 50; // Render above ground but below preview
        }
        
        // Scale to match grid cell size
        if (gridMap != null)
        {
            float cellSize = gridMap.GetCellSize();
            gridHighlightPrefab.transform.localScale = new Vector3(cellSize, cellSize, 1f);
        }
        
        // Initially hide the prefab
        gridHighlightPrefab.SetActive(false);
    }
    
    private void UpdateGridHighlighting()
    {
        if (selectedStructureItem == null || gridMap == null) return;
        
        // Clear existing highlights
        ClearGridHighlights();
        
        // Get structure size and grid position
        Vector2Int structureSize = GetStructureSize(selectedStructureItem);
        Vector2Int gridPos = gridMap.WorldToGrid(lastMousePosition);
        
        // Create highlights for each cell the structure would occupy
        for (int x = 0; x < structureSize.x; x++)
        {
            for (int y = 0; y < structureSize.y; y++)
            {
                Vector2Int cellPos = gridPos + new Vector2Int(x, y);
                CreateGridHighlight(cellPos);
            }
        }
    }
    
    private void CreateGridHighlight(Vector2Int gridPosition)
    {
        if (gridHighlightPrefab == null) return;
        
        // Create a new highlight GameObject
        GameObject highlight = Instantiate(gridHighlightPrefab, gridHighlightParent.transform);
        highlight.SetActive(true);
        
        // Position it at the grid cell
        Vector3 worldPos = gridMap.GridToWorld(gridPosition);
        highlight.transform.position = worldPos;
        
        // Set color based on placeability
        Renderer renderer = highlight.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color highlightColor = isPlaceable ? placeableHighlightColor : notPlaceableHighlightColor;
            renderer.material.color = highlightColor;
        }
        
        // Add to current highlights list
        currentHighlights.Add(highlight);
    }
    
    private void ClearGridHighlights()
    {
        // Destroy all current highlights
        foreach (GameObject highlight in currentHighlights)
        {
            if (highlight != null)
                DestroyImmediate(highlight);
        }
        
        currentHighlights.Clear();
    }
    
    #region Public Methods
    public bool IsBuilding() => isBuilding;
    public UI_Item GetSelectedStructureItem() => selectedStructureItem;
    public bool IsCurrentPositionPlaceable() => isPlaceable;
    #endregion
}
