using UnityEngine;
using UnityEngine.Tilemaps;

public class GridMap : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(50, 50);
    [SerializeField] private Vector2 gridOffset = Vector2.zero;
    
    [Header("Ground Tilemap References")]
    [SerializeField] private Tilemap undergroundTilemap;
    [SerializeField] private Tilemap lowgroundTilemap;
    [SerializeField] private Tilemap midgroundTilemap;
    [SerializeField] private Tilemap highgroundTilemap;
    
    [Header("Structure Tilemap References")]
    [SerializeField] private Tilemap structuresCollisionTilemap;
    [SerializeField] private Tilemap structuresWalkThroughTilemap;
    
    [Header("Grid Visualization")]
    [SerializeField] private bool showGridGizmos = true;
    [SerializeField] private Color gridColor = Color.white;
    
    [Header("Runtime Grid Lines")]
    [SerializeField] private bool showRuntimeGridLines = false;
    [SerializeField] private Color runtimeGridColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private float runtimeGridLineWidth = 0.02f;
    [SerializeField] private Material gridLineMaterial;
    [SerializeField] private bool pixelPerfectGridLines = true;
    
    private LineRenderer[] gridLineRenderers;
    private GameObject gridLinesParent;
    
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo = true;
    
    // Grid data storage
    private GridTile[,] gridData;
    private Grid grid;
    
    void Start()
    {
        InitializeGrid();
        InitializeTilemaps();
        ScanExistingTiles();
        InitializeRuntimeGridLines();
    }
    
    private void InitializeGrid()
    {
        // Create the grid data array
        gridData = new GridTile[gridSize.x, gridSize.y];
        
        // Initialize all grid tiles
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                gridData[x, y] = new GridTile();
            }
        }
        
        // Get or create the Grid component
        grid = GetComponent<Grid>();
        if (grid == null)
        {
            grid = gameObject.AddComponent<Grid>();
        }
        
        // Set grid cell size
        grid.cellSize = new Vector3(cellSize, cellSize, 1f);
        
        if (showDebugInfo)
        {
            Debug.Log($"GridMap initialized: {gridSize.x}x{gridSize.y} grid with {cellSize} cell size");
        }
    }
    
    private void InitializeTilemaps()
    {
        // Find ground tilemaps if not assigned
        if (undergroundTilemap == null)
            undergroundTilemap = transform.Find("GridGround/Underground")?.GetComponent<Tilemap>();
        if (lowgroundTilemap == null)
            lowgroundTilemap = transform.Find("GridGround/Lowground")?.GetComponent<Tilemap>();
        if (midgroundTilemap == null)
            midgroundTilemap = transform.Find("GridGround/Midground")?.GetComponent<Tilemap>();
        if (highgroundTilemap == null)
            highgroundTilemap = transform.Find("GridGround/Highground")?.GetComponent<Tilemap>();
        
        // Find structure tilemaps if not assigned
        if (structuresCollisionTilemap == null)
            structuresCollisionTilemap = transform.Find("GridStructures/StructuresCollision")?.GetComponent<Tilemap>();
        if (structuresWalkThroughTilemap == null)
            structuresWalkThroughTilemap = transform.Find("GridStructures/StructuresWalkThrough")?.GetComponent<Tilemap>();
        
        if (showDebugInfo)
        {
            Debug.Log($"Ground Tilemaps found - Underground: {undergroundTilemap != null}, Lowground: {lowgroundTilemap != null}, Midground: {midgroundTilemap != null}, Highground: {highgroundTilemap != null}");
            Debug.Log($"Structure Tilemaps found - Collision: {structuresCollisionTilemap != null}, WalkThrough: {structuresWalkThroughTilemap != null}");
        }
    }
    
    private void ScanExistingTiles()
    {
        if (showDebugInfo)
        {
            Debug.Log("Scanning existing tiles from tilemaps...");
        }
        
        int tilesFound = 0;
        
        // Scan each tilemap layer
        tilesFound += ScanTilemapLayer(undergroundTilemap, TilemapLayer.Underground);
        tilesFound += ScanTilemapLayer(lowgroundTilemap, TilemapLayer.Lowground);
        tilesFound += ScanTilemapLayer(midgroundTilemap, TilemapLayer.Midground);
        tilesFound += ScanTilemapLayer(highgroundTilemap, TilemapLayer.Highground);
        tilesFound += ScanTilemapLayer(structuresCollisionTilemap, TilemapLayer.StructuresCollision);
        tilesFound += ScanTilemapLayer(structuresWalkThroughTilemap, TilemapLayer.StructuresWalkThrough);
        
        if (showDebugInfo)
        {
            Debug.Log($"Scan complete! Found {tilesFound} existing tiles across all layers.");
        }
    }
    
    private int ScanTilemapLayer(Tilemap tilemap, TilemapLayer layer)
    {
        if (tilemap == null) return 0;
        
        int tilesFound = 0;
        BoundsInt bounds = tilemap.cellBounds;
        
        // Iterate through all tiles in the tilemap
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilemapPos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(tilemapPos);
                
                if (tile != null)
                {
                    Vector2Int gridPos = TilemapToGrid(tilemapPos);
                    
                    // Only process tiles within our grid bounds
                    if (IsValidGridPosition(gridPos))
                    {
                        GridTile gridTile = GetGridTile(gridPos);
                        if (gridTile != null)
                        {
                            gridTile.SetTile(tile, layer);
                            tilesFound++;
                        }
                    }
                }
            }
        }
        
        if (showDebugInfo && tilesFound > 0)
        {
            Debug.Log($"Found {tilesFound} tiles in {layer} layer");
        }
        
        return tilesFound;
    }
    
    private void InitializeRuntimeGridLines()
    {
        // Get actual tilemap cell size and bounds
        Vector3 tilemapCellSize = GetTilemapCellSize();
        Bounds tilemapBounds = GetTilemapBounds();
        
        if (showDebugInfo)
        {
            Debug.Log($"Tilemap cell size: {tilemapCellSize}");
            Debug.Log($"Tilemap bounds: {tilemapBounds}");
        }
        
        // Create parent object for grid lines
        gridLinesParent = new GameObject("GridLines");
        gridLinesParent.transform.SetParent(transform);
        gridLinesParent.transform.localPosition = Vector3.zero;
        
        // Calculate grid dimensions based on tilemap bounds
        float actualCellSize = tilemapCellSize.x; // Use tilemap's actual cell size
        Vector3 gridStart = tilemapBounds.min;
        Vector3 gridEnd = tilemapBounds.max;
        
        // Calculate number of lines needed based on tilemap bounds
        int verticalLines = Mathf.CeilToInt((gridEnd.x - gridStart.x) / actualCellSize) + 1;
        int horizontalLines = Mathf.CeilToInt((gridEnd.y - gridStart.y) / actualCellSize) + 1;
        int totalLines = verticalLines + horizontalLines;
        
        gridLineRenderers = new LineRenderer[totalLines];
        
        // Create vertical lines
        for (int i = 0; i < verticalLines; i++)
        {
            GameObject lineObj = new GameObject($"VerticalLine_{i}");
            lineObj.transform.SetParent(gridLinesParent.transform);
            
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = gridLineMaterial != null ? gridLineMaterial : CreateDefaultLineMaterial();
            lr.material.color = runtimeGridColor;
            lr.startWidth = runtimeGridLineWidth;
            lr.endWidth = runtimeGridLineWidth;
            lr.positionCount = 2;
            lr.useWorldSpace = true; // Use world space to match tilemap
            lr.sortingOrder = 1000; // Render on top
            
            // Set line positions based on tilemap bounds
            float x = gridStart.x + (i * actualCellSize);
            Vector3 start = new Vector3(x, gridStart.y, 0);
            Vector3 end = new Vector3(x, gridEnd.y, 0);
            
            // Snap to pixel boundaries if enabled
            if (pixelPerfectGridLines)
            {
                start = SnapToPixelBoundary(start);
                end = SnapToPixelBoundary(end);
            }
            
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            
            gridLineRenderers[i] = lr;
        }
        
        // Create horizontal lines
        for (int i = 0; i < horizontalLines; i++)
        {
            GameObject lineObj = new GameObject($"HorizontalLine_{i}");
            lineObj.transform.SetParent(gridLinesParent.transform);
            
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = gridLineMaterial != null ? gridLineMaterial : CreateDefaultLineMaterial();
            lr.material.color = runtimeGridColor;
            lr.startWidth = runtimeGridLineWidth;
            lr.endWidth = runtimeGridLineWidth;
            lr.positionCount = 2;
            lr.useWorldSpace = true; // Use world space to match tilemap
            lr.sortingOrder = 1000; // Render on top
            
            // Set line positions based on tilemap bounds
            float y = gridStart.y + (i * actualCellSize);
            Vector3 start = new Vector3(gridStart.x, y, 0);
            Vector3 end = new Vector3(gridEnd.x, y, 0);
            
            // Snap to pixel boundaries if enabled
            if (pixelPerfectGridLines)
            {
                start = SnapToPixelBoundary(start);
                end = SnapToPixelBoundary(end);
            }
            
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            
            gridLineRenderers[verticalLines + i] = lr;
        }
        
        // Initially hide the grid lines
        SetRuntimeGridLinesVisible(showRuntimeGridLines);
        
        if (showDebugInfo)
        {
            Debug.Log($"Created {totalLines} runtime grid lines ({verticalLines} vertical, {horizontalLines} horizontal)");
            Debug.Log($"Grid lines aligned to tilemap bounds: {gridStart} to {gridEnd}");
        }
    }
    
    private Vector3 GetTilemapCellSize()
    {
        // Get cell size from the first available tilemap
        if (undergroundTilemap != null)
            return undergroundTilemap.cellSize;
        if (lowgroundTilemap != null)
            return lowgroundTilemap.cellSize;
        if (midgroundTilemap != null)
            return midgroundTilemap.cellSize;
        if (highgroundTilemap != null)
            return highgroundTilemap.cellSize;
        if (structuresCollisionTilemap != null)
            return structuresCollisionTilemap.cellSize;
        if (structuresWalkThroughTilemap != null)
            return structuresWalkThroughTilemap.cellSize;
        
        // Fallback to inspector cell size
        return new Vector3(cellSize, cellSize, 1f);
    }
    
    private Bounds GetTilemapBounds()
    {
        Bounds combinedBounds = new Bounds();
        bool boundsInitialized = false;
        
        // Check all tilemaps and combine their bounds
        Tilemap[] tilemaps = { undergroundTilemap, lowgroundTilemap, midgroundTilemap, 
                              highgroundTilemap, structuresCollisionTilemap, structuresWalkThroughTilemap };
        
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap != null)
            {
                Bounds tilemapBounds = tilemap.localBounds;
                if (!boundsInitialized)
                {
                    combinedBounds = tilemapBounds;
                    boundsInitialized = true;
                }
                else
                {
                    combinedBounds.Encapsulate(tilemapBounds);
                }
            }
        }
        
        // If no tilemaps found, use inspector settings
        if (!boundsInitialized)
        {
            Vector3 center = new Vector3(gridOffset.x + (gridSize.x * cellSize * 0.5f), 
                                       gridOffset.y + (gridSize.y * cellSize * 0.5f), 0);
            Vector3 size = new Vector3(gridSize.x * cellSize, gridSize.y * cellSize, 1);
            combinedBounds = new Bounds(center, size);
        }
        
        return combinedBounds;
    }
    
    private Vector3 SnapToPixelBoundary(Vector3 position)
    {
        // Get the camera to determine pixel size
        Camera cam = Camera.main;
        if (cam == null) return position;
        
        // Calculate pixel size based on camera orthographic size and screen height
        float pixelSize = (cam.orthographicSize * 2f) / Screen.height;
        
        // Snap to nearest pixel boundary
        float snappedX = Mathf.Round(position.x / pixelSize) * pixelSize;
        float snappedY = Mathf.Round(position.y / pixelSize) * pixelSize;
        
        return new Vector3(snappedX, snappedY, position.z);
    }
    
    private Material CreateDefaultLineMaterial()
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = runtimeGridColor;
        return mat;
    }
    
    public void SetRuntimeGridLinesVisible(bool visible)
    {
        showRuntimeGridLines = visible;
        
        if (gridLinesParent != null)
        {
            gridLinesParent.SetActive(visible);
        }
    }
    
    public void ToggleRuntimeGridLines()
    {
        SetRuntimeGridLinesVisible(!showRuntimeGridLines);
    }
    
    public void UpdateRuntimeGridColor(Color newColor)
    {
        runtimeGridColor = newColor;
        
        if (gridLineRenderers != null)
        {
            foreach (LineRenderer lr in gridLineRenderers)
            {
                if (lr != null && lr.material != null)
                {
                    lr.material.color = newColor;
                }
            }
        }
    }
    
    #region Grid Coordinate Conversion
    /// <summary>
    /// Convert world position to grid coordinates
    /// </summary>
    /// <param name="worldPos">World position</param>
    /// <returns>Grid coordinates</returns>
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        int x = Mathf.FloorToInt((localPos.x - gridOffset.x) / cellSize);
        int y = Mathf.FloorToInt((localPos.y - gridOffset.y) / cellSize);
        return new Vector2Int(x, y);
    }
    
    /// <summary>
    /// Convert grid coordinates to world position
    /// </summary>
    /// <param name="gridPos">Grid coordinates</param>
    /// <returns>World position (center of cell)</returns>
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float worldX = gridPos.x * cellSize + gridOffset.x + (cellSize * 0.5f);
        float worldY = gridPos.y * cellSize + gridOffset.y + (cellSize * 0.5f);
        return transform.TransformPoint(new Vector3(worldX, worldY, 0f));
    }
    
    /// <summary>
    /// Convert grid coordinates to tilemap position
    /// </summary>
    /// <param name="gridPos">Grid coordinates</param>
    /// <returns>Tilemap position</returns>
    public Vector3Int GridToTilemap(Vector2Int gridPos)
    {
        return new Vector3Int(gridPos.x, gridPos.y, 0);
    }
    
    /// <summary>
    /// Convert tilemap position to grid coordinates
    /// </summary>
    /// <param name="tilemapPos">Tilemap position</param>
    /// <returns>Grid coordinates</returns>
    public Vector2Int TilemapToGrid(Vector3Int tilemapPos)
    {
        return new Vector2Int(tilemapPos.x, tilemapPos.y);
    }
    #endregion
    
    #region Grid Validation
    /// <summary>
    /// Check if grid coordinates are within bounds
    /// </summary>
    /// <param name="gridPos">Grid coordinates to check</param>
    /// <returns>True if coordinates are valid</returns>
    public bool IsValidGridPosition(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridSize.x && 
               gridPos.y >= 0 && gridPos.y < gridSize.y;
    }
    
    /// <summary>
    /// Get grid tile at specified coordinates
    /// </summary>
    /// <param name="gridPos">Grid coordinates</param>
    /// <returns>GridTile at position, or null if invalid</returns>
    public GridTile GetGridTile(Vector2Int gridPos)
    {
        if (!IsValidGridPosition(gridPos)) return null;
        return gridData[gridPos.x, gridPos.y];
    }
    
    /// <summary>
    /// Check if a specific layer has a tile at the given position
    /// </summary>
    /// <param name="gridPos">Grid coordinates</param>
    /// <param name="layer">Layer to check</param>
    /// <returns>True if tile exists on that layer</returns>
    public bool HasTileAt(Vector2Int gridPos, TilemapLayer layer)
    {
        GridTile gridTile = GetGridTile(gridPos);
        return gridTile != null && gridTile.HasTile(layer);
    }
    
    /// <summary>
    /// Get the tile at a specific position and layer
    /// </summary>
    /// <param name="gridPos">Grid coordinates</param>
    /// <param name="layer">Layer to check</param>
    /// <returns>TileBase at position, or null if none</returns>
    public TileBase GetTileAt(Vector2Int gridPos, TilemapLayer layer)
    {
        GridTile gridTile = GetGridTile(gridPos);
        if (gridTile == null) return null;
        
        switch (layer)
        {
            case TilemapLayer.Underground:
                return gridTile.undergroundTile;
            case TilemapLayer.Lowground:
                return gridTile.lowgroundTile;
            case TilemapLayer.Midground:
                return gridTile.midgroundTile;
            case TilemapLayer.Highground:
                return gridTile.highgroundTile;
            case TilemapLayer.StructuresCollision:
                return gridTile.structuresCollisionTile;
            case TilemapLayer.StructuresWalkThrough:
                return gridTile.structuresWalkThroughTile;
            default:
                return null;
        }
    }
    
    /// <summary>
    /// Check if a position is clear for structure placement (no collision structures)
    /// </summary>
    /// <param name="gridPos">Grid coordinates</param>
    /// <returns>True if position is clear for structures</returns>
    public bool IsPositionClearForStructure(Vector2Int gridPos)
    {
        return !HasTileAt(gridPos, TilemapLayer.StructuresCollision);
    }
    
    /// <summary>
    /// Check if a position has any collision structures
    /// </summary>
    /// <param name="gridPos">Grid coordinates</param>
    /// <returns>True if position has collision structures</returns>
    public bool HasCollisionStructure(Vector2Int gridPos)
    {
        return HasTileAt(gridPos, TilemapLayer.StructuresCollision);
    }
    
    /// <summary>
    /// Check if a position has any walk-through structures
    /// </summary>
    /// <param name="gridPos">Grid coordinates</param>
    /// <returns>True if position has walk-through structures</returns>
    public bool HasWalkThroughStructure(Vector2Int gridPos)
    {
        return HasTileAt(gridPos, TilemapLayer.StructuresWalkThrough);
    }
    
    /// <summary>
    /// Check if a position has ground (lowground or midground)
    /// </summary>
    /// <param name="gridPos">Grid coordinates</param>
    /// <returns>True if position has ground tile</returns>
    public bool HasGroundAt(Vector2Int gridPos)
    {
        return HasTileAt(gridPos, TilemapLayer.Lowground) || HasTileAt(gridPos, TilemapLayer.Midground);
    }
    
    /// <summary>
    /// Check if a position has any tile on any layer
    /// </summary>
    /// <param name="gridPos">Grid coordinates</param>
    /// <returns>True if position has any tile</returns>
    public bool HasAnyTileAt(Vector2Int gridPos)
    {
        return HasTileAt(gridPos, TilemapLayer.Underground) ||
               HasTileAt(gridPos, TilemapLayer.Lowground) ||
               HasTileAt(gridPos, TilemapLayer.Midground) ||
               HasTileAt(gridPos, TilemapLayer.Highground);
    }
    #endregion
    
    #region Tile Placement
    /// <summary>
    /// Place a tile at grid coordinates
    /// </summary>
    /// <param name="gridPos">Grid coordinates</param>
    /// <param name="tile">Tile to place</param>
    /// <param name="layer">Which tilemap layer to use</param>
    /// <returns>True if placement was successful</returns>
    public bool PlaceTile(Vector2Int gridPos, TileBase tile, TilemapLayer layer = TilemapLayer.Lowground)
    {
        if (!IsValidGridPosition(gridPos)) return false;
        
        Vector3Int tilemapPos = GridToTilemap(gridPos);
        Tilemap targetTilemap = GetTilemapByLayer(layer);
        
        if (targetTilemap == null) return false;
        
        // Place tile in tilemap
        targetTilemap.SetTile(tilemapPos, tile);
        
        // Update grid data
        GridTile gridTile = GetGridTile(gridPos);
        if (gridTile != null)
        {
            gridTile.SetTile(tile, layer);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Placed tile at grid position {gridPos} on {layer} layer");
        }
        
        return true;
    }
    
    /// <summary>
    /// Remove tile at grid coordinates
    /// </summary>
    /// <param name="gridPos">Grid coordinates</param>
    /// <param name="layer">Which tilemap layer to clear</param>
    /// <returns>True if removal was successful</returns>
    public bool RemoveTile(Vector2Int gridPos, TilemapLayer layer = TilemapLayer.Lowground)
    {
        if (!IsValidGridPosition(gridPos)) return false;
        
        Vector3Int tilemapPos = GridToTilemap(gridPos);
        Tilemap targetTilemap = GetTilemapByLayer(layer);
        
        if (targetTilemap == null) return false;
        
        // Remove tile from tilemap
        targetTilemap.SetTile(tilemapPos, null);
        
        // Update grid data
        GridTile gridTile = GetGridTile(gridPos);
        if (gridTile != null)
        {
            gridTile.ClearTile(layer);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Removed tile at grid position {gridPos} from {layer} layer");
        }
        
        return true;
    }
    #endregion
    
    #region Helper Methods
    private Tilemap GetTilemapByLayer(TilemapLayer layer)
    {
        switch (layer)
        {
            case TilemapLayer.Underground:
                return undergroundTilemap;
            case TilemapLayer.Lowground:
                return lowgroundTilemap;
            case TilemapLayer.Midground:
                return midgroundTilemap;
            case TilemapLayer.Highground:
                return highgroundTilemap;
            case TilemapLayer.StructuresCollision:
                return structuresCollisionTilemap;
            case TilemapLayer.StructuresWalkThrough:
                return structuresWalkThroughTilemap;
            default:
                return lowgroundTilemap;
        }
    }
    #endregion
    
    #region Structure Placement
    /// <summary>
    /// Place a structure at the specified grid position
    /// </summary>
    /// <param name="gridPosition">Grid position to place the structure</param>
    /// <param name="structureSize">Size of the structure (width x height)</param>
    /// <param name="structureItem">The structure item data</param>
    /// <param name="layer">Which tilemap layer to place on</param>
    /// <returns>True if placement was successful</returns>
    public bool PlaceStructure(Vector2Int gridPosition, Vector2Int structureSize, UI_Item structureItem, TilemapLayer layer = TilemapLayer.StructuresCollision)
    {
        // Validate placement
        if (!CanPlaceStructure(gridPosition, structureSize))
        {
            Debug.LogWarning($"Cannot place structure at {gridPosition} with size {structureSize}");
            return false;
        }
        
        // Get the appropriate tilemap
        Tilemap targetTilemap = GetTilemapByLayer(layer);
        if (targetTilemap == null)
        {
            Debug.LogError($"No tilemap found for layer: {layer}");
            return false;
        }
        
        // Place tiles for the structure
        for (int x = 0; x < structureSize.x; x++)
        {
            for (int y = 0; y < structureSize.y; y++)
            {
                Vector2Int tilePos = gridPosition + new Vector2Int(x, y);
                Vector3Int tilemapPos = GridToTilemap(tilePos);
                
                // Place the tile (you'll need to create a tile asset from the structure item)
                TileBase structureTile = CreateTileFromStructureItem(structureItem);
                targetTilemap.SetTile(tilemapPos, structureTile);
                
                // Update grid data
                if (IsValidGridPosition(tilePos))
                {
                    gridData[tilePos.x, tilePos.y].SetTile(structureTile, layer);
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Placed structure {structureItem.ItemName} at {gridPosition} with size {structureSize} on layer {layer}");
        }
        
        return true;
    }
    
    /// <summary>
    /// Check if a structure can be placed at the specified position
    /// </summary>
    /// <param name="gridPosition">Grid position to check</param>
    /// <param name="structureSize">Size of the structure</param>
    /// <returns>True if the structure can be placed</returns>
    public bool CanPlaceStructure(Vector2Int gridPosition, Vector2Int structureSize)
    {
        // Check if all required cells are within bounds and clear
        for (int x = 0; x < structureSize.x; x++)
        {
            for (int y = 0; y < structureSize.y; y++)
            {
                Vector2Int checkPos = gridPosition + new Vector2Int(x, y);
                
                // Check bounds
                if (!IsValidGridPosition(checkPos))
                {
                    return false;
                }
                
                // Check for collision structures
                if (HasCollisionStructure(checkPos))
                {
                    return false;
                }
                
                // Check for ground (structures need ground to be placed on)
                if (!HasGroundAt(checkPos))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Remove a structure from the specified position
    /// </summary>
    /// <param name="gridPosition">Grid position to remove from</param>
    /// <param name="structureSize">Size of the structure to remove</param>
    /// <param name="layer">Which tilemap layer to remove from</param>
    /// <returns>True if removal was successful</returns>
    public bool RemoveStructure(Vector2Int gridPosition, Vector2Int structureSize, TilemapLayer layer = TilemapLayer.StructuresCollision)
    {
        // Get the appropriate tilemap
        Tilemap targetTilemap = GetTilemapByLayer(layer);
        if (targetTilemap == null)
        {
            Debug.LogError($"No tilemap found for layer: {layer}");
            return false;
        }
        
        // Remove tiles for the structure
        for (int x = 0; x < structureSize.x; x++)
        {
            for (int y = 0; y < structureSize.y; y++)
            {
                Vector2Int tilePos = gridPosition + new Vector2Int(x, y);
                Vector3Int tilemapPos = GridToTilemap(tilePos);
                
                // Remove the tile
                targetTilemap.SetTile(tilemapPos, null);
                
                // Update grid data
                if (IsValidGridPosition(tilePos))
                {
                    gridData[tilePos.x, tilePos.y].ClearTile(layer);
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Removed structure at {gridPosition} with size {structureSize} from layer {layer}");
        }
        
        return true;
    }
    
    /// <summary>
    /// Create a TileBase from a structure item (placeholder implementation)
    /// </summary>
    /// <param name="structureItem">The structure item to convert</param>
    /// <returns>A TileBase for the structure</returns>
    private TileBase CreateTileFromStructureItem(UI_Item structureItem)
    {
        // This is a placeholder - you'll need to implement this based on your tile system
        // For now, return null which will create empty tiles
        // You might want to:
        // 1. Create a mapping from structure items to tile assets
        // 2. Generate tiles procedurally from the structure item's sprite
        // 3. Use a tile database system
        
        Debug.LogWarning($"CreateTileFromStructureItem not implemented for {structureItem.ItemName}");
        return null;
    }
    #endregion
    
    #region Getters
    public Vector2Int GetGridSize() => gridSize;
    public float GetCellSize() => cellSize;
    public Vector2 GetGridOffset() => gridOffset;
    public Grid GetGrid() => grid;
    public bool IsRuntimeGridLinesVisible() => showRuntimeGridLines;
    #endregion
    
    #region Gizmos
    void OnDrawGizmos()
    {
        if (!showGridGizmos) return;
        
        Gizmos.color = gridColor;
        
        // Draw grid lines
        for (int x = 0; x <= gridSize.x; x++)
        {
            Vector3 start = GridToWorld(new Vector2Int(x, 0)) - Vector3.right * (cellSize * 0.5f);
            Vector3 end = GridToWorld(new Vector2Int(x, gridSize.y)) - Vector3.right * (cellSize * 0.5f);
            Gizmos.DrawLine(start, end);
        }
        
        for (int y = 0; y <= gridSize.y; y++)
        {
            Vector3 start = GridToWorld(new Vector2Int(0, y)) - Vector3.up * (cellSize * 0.5f);
            Vector3 end = GridToWorld(new Vector2Int(gridSize.x, y)) - Vector3.up * (cellSize * 0.5f);
            Gizmos.DrawLine(start, end);
        }
    }
    #endregion
}

// Enum for different tilemap layers
public enum TilemapLayer
{
    Underground,
    Lowground,
    Midground,
    Highground,
    StructuresCollision,
    StructuresWalkThrough
}

// Class to store grid tile data
[System.Serializable]
public class GridTile
{
    public TileBase undergroundTile;
    public TileBase lowgroundTile;
    public TileBase midgroundTile;
    public TileBase highgroundTile;
    public TileBase structuresCollisionTile;
    public TileBase structuresWalkThroughTile;
    
    public void SetTile(TileBase tile, TilemapLayer layer)
    {
        switch (layer)
        {
            case TilemapLayer.Underground:
                undergroundTile = tile;
                break;
            case TilemapLayer.Lowground:
                lowgroundTile = tile;
                break;
            case TilemapLayer.Midground:
                midgroundTile = tile;
                break;
            case TilemapLayer.Highground:
                highgroundTile = tile;
                break;
            case TilemapLayer.StructuresCollision:
                structuresCollisionTile = tile;
                break;
            case TilemapLayer.StructuresWalkThrough:
                structuresWalkThroughTile = tile;
                break;
        }
    }
    
    public void ClearTile(TilemapLayer layer)
    {
        SetTile(null, layer);
    }
    
    public bool HasTile(TilemapLayer layer)
    {
        switch (layer)
        {
            case TilemapLayer.Underground:
                return undergroundTile != null;
            case TilemapLayer.Lowground:
                return lowgroundTile != null;
            case TilemapLayer.Midground:
                return midgroundTile != null;
            case TilemapLayer.Highground:
                return highgroundTile != null;
            case TilemapLayer.StructuresCollision:
                return structuresCollisionTile != null;
            case TilemapLayer.StructuresWalkThrough:
                return structuresWalkThroughTile != null;
            default:
                return false;
        }
    }
}
