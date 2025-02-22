using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    [SerializeField] 
    private Tilemap map;

    [SerializeField] private List<TileData> tileDataList;

    private Dictionary<TileBase, TileData> dataFromTiles;
    private Dictionary<int, int> rowSums, colSums;
    private Dictionary<int, int> originalRowSums, originalColSums; 
    private List<TileData> monitorTiles = new List<TileData>(); 

    private void Start()
    {
        dataFromTiles = new Dictionary<TileBase, TileData>();
        rowSums = new Dictionary<int, int>();
        colSums = new Dictionary<int, int>();
        originalRowSums = new Dictionary<int, int>();
        originalColSums = new Dictionary<int, int>();


        foreach (var tileData in tileDataList)
        {
            foreach (var tile in tileData.tiles)
            {
                dataFromTiles.Add(tile, tileData);
            }
            if (tileData.isMonitor) monitorTiles.Add(tileData);
        }
        
        BuildRowColSums();
        
    }
    
    private void BuildRowColSums()
    {
        int brickCOUnt = 0;
        for (int x = -7; x < 1; x++)
        {
            for (int y = -4; y < 5; y++)
            {
                Vector3Int gridPosition = new Vector3Int(x, y, 0);
                TileBase tile = map.GetTile(gridPosition);
                if (!rowSums.ContainsKey(y)) rowSums[y] = 0;
                if (!colSums.ContainsKey(x)) colSums[x] = 0;

                if (tile != null && dataFromTiles.TryGetValue(tile, out TileData tileData))
                {
                    if (tileData.value == -1)
                    {
                        Debug.Log($"{y}, {x}, {tileData.value}");
                        
                    }
                    rowSums[y] += tileData.value;
                    colSums[x] += tileData.value;
                    rowSums[y] %= 7;
                    colSums[x] %= 7;
                    brickCOUnt++; 
                }
            }
        }
        
        Debug.Log($"a total of {brickCOUnt}");

        // Save original sums for later comparisons
        originalRowSums = new Dictionary<int, int>(rowSums);
        originalColSums = new Dictionary<int, int>(colSums);
        // Properly print row and column sums
        Debug.Log($"Row sum initialized: {DictionaryToString(rowSums)}");
        Debug.Log($"Col sum initialized: {DictionaryToString(colSums)}");
    }
    
    private string DictionaryToString(Dictionary<int, int> dict)
    {
        return string.Join(", ", dict.Select(kv => $"[{kv.Key}: {kv.Value}]"));
    }
    
    // public void UpdateSums(Vector2 position, int oldValue, int newValue, int playerValue)
    // {
    //     Vector3Int gridPosition = map.WorldToCell(position);
    //     int row = gridPosition.y;
    //     int col = gridPosition.x;
    //     
    //     // Debug.Log($"pre-update Row {row}: {rowSums[row]}, Col {col}: {colSums[col]}");
    //     
    //     rowSums[row] += newValue - oldValue;
    //     colSums[col] += newValue - oldValue;
    //
    //     if (rowSums[row] != originalRowSums[row])
    //     {
    //         Debug.Log($"Row {row}: {rowSums[row]} originally {originalRowSums[row]}");
    //     }
    //
    //     if (colSums[col] != originalColSums[col])
    //     {
    //         Debug.Log($"Col {row}: {colSums[col]} originally {originalColSums[col]}");
    //     }
    //     
    //     // Debug.Log($"post-update Row {row}: {rowSums[row]}, Col {col}: {colSums[col]}");
    //     
    //     // // Check if monitor tiles need to be updated
    //     // UpdateMonitorTiles();
    // }
    
    public void UpdateSums(
        Vector3Int currentGridPosition, 
        Vector3Int previousGridPosition, 
        int currentPlayerValue, 
        int previousPlayerValue,
        int currentTileValue
    )
    {
        Vector3Int newGridPos = map.WorldToCell(currentGridPosition);
        Vector3Int prevGridPos = map.WorldToCell(previousGridPosition);

        int currentRow = newGridPos.y+1;
        int currentCol = newGridPos.x+1;
        int prevRow = prevGridPos.y+1;
        int prevCol = prevGridPos.x+1;
        
        Debug.Log($"movement, {previousGridPosition}, {currentGridPosition}");
        
        bool movedHorizontally = currentRow == prevRow; // Same row -> only col sum changes
        bool movedVertically = currentCol == prevCol;   // Same column -> both rows change

        if (movedHorizontally)
        {
            // Debug.Log($"Moved horizontally, {prevGridPos} to {newGridPos}");
            if (colSums.ContainsKey(prevCol))
            {
                colSums[prevCol] -= previousPlayerValue;
                colSums[prevCol] = ((colSums[prevCol] % 7) + 7) % 7; 
            };
            
            colSums[currentCol] += (currentPlayerValue - currentTileValue);
            colSums[currentCol] %= 7;
            
            if (colSums.ContainsKey(currentCol) && colSums[currentCol] != originalColSums[currentCol])
            {
                Debug.Log($"Col {currentCol} updated: {colSums[currentCol]} (original: {originalColSums[currentCol]})");
            }
            if (colSums.ContainsKey(prevCol) && colSums[prevCol] != originalColSums[prevCol])
            {
                Debug.Log($"Col {prevCol} updated: {colSums[prevCol]} (original: {originalColSums[prevCol]})");
            }
        }

        if (movedVertically)
        {
            Debug.Log($"Moved vertically, {prevGridPos} to {newGridPos}");
            if (rowSums.ContainsKey(prevRow))
            {
                rowSums[prevRow] -= previousPlayerValue;
                rowSums[prevRow] = ((rowSums[prevRow] % 7) + 7) % 7; 

            };

            rowSums[currentRow] += (currentPlayerValue - currentTileValue);
            rowSums[currentRow] %= 7;
            
            if (rowSums.ContainsKey(currentRow) && rowSums[currentRow] != originalRowSums[currentRow])
            {
                Debug.Log($"Row {currentRow} updated: {rowSums[currentRow]} (original: {originalRowSums[currentRow]})");
            }
            if (rowSums.ContainsKey(prevRow) && rowSums[prevRow] != originalRowSums[prevRow])
            {
                Debug.Log($"Row {prevRow} updated: {rowSums[prevRow]} (original: {originalRowSums[prevRow]})");
            }
        }

        // Check if monitor tiles need to be updated
        UpdateMonitorTiles();
    }
    
    private void UpdateMonitorTiles()
    {
        // foreach (var monitor in monitorTiles)
        // {
        //     int currentSum = monitor.isRow ? rowSums.GetValueOrDefault(monitor.index, 0)
        //         : colSums.GetValueOrDefault(monitor.index, 0);
        //     int originalSum = monitor.isRow ? originalRowSums.GetValueOrDefault(monitor.index, 0)
        //         : originalColSums.GetValueOrDefault(monitor.index, 0);
        //
        //     monitor.isWrong = (currentSum != originalSum);
        // }
        //
        // // Update sprite appearances
        // UpdateTileSprites();
    }
    
    private void UpdateTileSprites()
    {
        foreach (var tileData in monitorTiles)
        {
            // TODO figure out this part
            // could use an approach similar to  ReplaceTileWithSeven
            // if (tileData.isWrong)
            // {
            //     tileData.SetSprite("Sprites/WrongTile");
            // }
            // else
            // {
            //     tileData.SetSprite("Sprites/CorrectTile"); // Assuming a correct sprite exists
            // }
        }
    }
    
    public int GetTileValue(Vector2 worldPosition)
    {
        Vector3Int gridPosition = map.WorldToCell(worldPosition);
        
        TileBase clickedTile = map.GetTile(gridPosition);
        if (clickedTile != null)
        {
            TileData tileData = dataFromTiles[clickedTile];
            
            return tileData.value;
            
        }
        
        return 0;
    }
    
    public void UpdateWithoutTileChanges(
        Vector2 currentPosition, 
        Vector2 prevPosition, 
        int previousPlayerValue, 
        int currentPlayerValue
    )
    {
        Vector3Int currentGridPosition = map.WorldToCell(currentPosition);
        Vector3Int prevGridPosition = map.WorldToCell(prevPosition);
        int currentTileValue = 0;
        
        UpdateSums(currentGridPosition, prevGridPosition, currentPlayerValue, previousPlayerValue, currentTileValue);
    }

    
    public void RemoveTile(
        Vector2 currentPosition, 
        Vector2 prevPosition, 
        int previousPlayerValue, 
        int currentPlayerValue
    )
    {
        // Do tile removal 
        Vector3Int currentGridPosition = map.WorldToCell(currentPosition);
        TileBase currentTile = map.GetTile(currentGridPosition);
        map.SetTile(currentGridPosition, null);
        
        Vector3Int prevGridPosition = map.WorldToCell(prevPosition);
        
        int currentTileValue = dataFromTiles.ContainsKey(currentTile) ? dataFromTiles[currentTile].value : 0;

        UpdateSums(currentGridPosition, prevGridPosition, currentPlayerValue, previousPlayerValue, currentTileValue);
    }
    
    
    public void ReplaceTileWithSeven(
        Vector2 currentPosition, 
        Vector2 prevPosition, 
        int previousPlayerValue, 
        int currentPlayerValue
    )
    {
        // Do tile respawn 
        Vector3Int currentGridPosition = map.WorldToCell(currentPosition);
        TileBase newTile = tileDataList[6].tiles[0]; // Last tile in list as replacement
        map.SetTile(currentGridPosition, newTile);
        
        Vector3Int prevGridPosition = map.WorldToCell(prevPosition);
    
        int currentTileValue = 7;    
    
        UpdateSums(currentGridPosition, prevGridPosition, currentPlayerValue, previousPlayerValue, currentTileValue);
    }
    
    public bool DoesTileBlockMovement(Vector2 worldPosition, int playerValue)
    {
        Vector3Int gridPosition = map.WorldToCell(worldPosition);
        TileBase tile = map.GetTile(gridPosition);

        if (tile != null && dataFromTiles.TryGetValue(tile, out TileData tileData))
        {
            if (tileData.isMonitor)
            {
                // Debug.Log($"Monitor hit at {gridPosition}, isWrong: {tileData.isWrong}, playerValue: {playerValue}");
                if (gridPosition.x == 1)
                {
                    // Debug.Log($"This is a row monitor with row index {gridPosition.y}");
                } 
                else if (gridPosition.y == -5)
                {
                    // Debug.Log($"This is a col monitor with col index {gridPosition.x}");
                }
                
                return (!tileData.isWrong) | (playerValue != 0);
            }
        }

        return false;
    }

    
}
