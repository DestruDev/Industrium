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
    
    [Header("Visual Feedback")]
    [SerializeField] private Color placeableColor = Color.green;
    [SerializeField] private Color notPlaceableColor = Color.red;
    [SerializeField] private float previewAlpha = 0.7f;
    
    [Header("References")]
    [SerializeField] private GridMap gridMap;
    [SerializeField] private Hotbar hotbar;
    [SerializeField] private Camera mainCamera;
    
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
        }
        
        Debug.Log($"Started building mode with: {structureItem.ItemName}");
    }
    
    private void StopBuildingMode()
    {
        isBuilding = false;
        selectedStructureItem = null;
        
        // Hide structure preview
        structurePreview.SetActive(false);
        
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
        
        // Update preview color
        UpdatePreviewColor();
    }
    
    private bool IsPositionValidForStructure(Vector2Int gridPos, UI_Item structureItem)
    {
        // Get structure size based on subcategory
        Vector2Int structureSize = GetStructureSize(structureItem);
        
        // Use GridMap's built-in validation
        return gridMap.CanPlaceStructure(gridPos, structureSize);
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
        
        Vector2Int gridPos = gridMap.WorldToGrid(lastMousePosition);
        Vector2Int structureSize = GetStructureSize(selectedStructureItem);
        
        // Place the structure using GridMap
        bool success = gridMap.PlaceStructure(gridPos, structureSize, selectedStructureItem);
        
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
