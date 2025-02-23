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
        }
        
        BuildRowColSums();
        
    }
    
    private void BuildRowColSums()
    {
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
                    // if (tileData.value == -1)
                    // {
                    //     Debug.Log($"{y}, {x}, {tileData.value}");
                    //     
                    // }
                    rowSums[y] += tileData.value;
                    colSums[x] += tileData.value;
                    rowSums[y] %= 7;
                    colSums[x] %= 7;
                }
            }
        }
        
        // Save original sums for later comparisons
        originalRowSums = new Dictionary<int, int>(rowSums);
        originalColSums = new Dictionary<int, int>(colSums);
        // Properly print row and column sums
        // Debug.Log($"Row sum initialized: {DictionaryToString(rowSums)}");
        // Debug.Log($"Col sum initialized: {DictionaryToString(colSums)}");
    }
    
    private string DictionaryToString(Dictionary<int, int> dict)
    {
        return string.Join(", ", dict.Select(kv => $"[{kv.Key}: {kv.Value}]"));
    }
    
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
        
        // Debug.Log($"movement, {previousGridPosition}, {currentGridPosition}");
        
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

            if (colSums.ContainsKey(currentCol))
            {
                colSums[currentCol] += (currentPlayerValue - currentTileValue);
                colSums[currentCol] %= 7;
            }
            
            if (colSums.ContainsKey(currentCol))
            {
                if (colSums[currentCol] != originalColSums[currentCol])
                {
                    // Debug.Log($"Col {currentCol} updated: {colSums[currentCol]} (original: {originalColSums[currentCol]})");
                    AlterMonitorState(currentCol, true, true);
                }
                else
                {
                    AlterMonitorState(currentCol, true, false);
                }
            }
            
            if (colSums.ContainsKey(prevCol))
            {
                if (colSums[prevCol] != originalColSums[prevCol])
                {
                    // Debug.Log($"Col {prevCol} updated: {colSums[prevCol]} (original: {originalColSums[prevCol]})");
                    AlterMonitorState(prevCol, true, true);
                }
                else
                {
                    AlterMonitorState(prevCol, true, false);
                }
                
            }
        }

        if (movedVertically)
        {
            // Debug.Log($"Moved vertically, {prevGridPos} to {newGridPos}");
            if (rowSums.ContainsKey(prevRow))
            {
                rowSums[prevRow] -= previousPlayerValue;
                rowSums[prevRow] = ((rowSums[prevRow] % 7) + 7) % 7; 
            };

            if (rowSums.ContainsKey(currentRow))
            {
                rowSums[currentRow] += (currentPlayerValue - currentTileValue);
                rowSums[currentRow] %= 7;
            }

            
            if (rowSums.ContainsKey(currentRow))
            {
                if (rowSums[currentRow] != originalRowSums[currentRow])
                {
                    // Debug.Log($"Row {currentRow} updated: {rowSums[currentRow]} (original: {originalRowSums[currentRow]})");
                    AlterMonitorState(currentRow, false, true);
                }
                else
                {
                    AlterMonitorState(currentRow, false, false);
                }
            }

            if (rowSums.ContainsKey(prevRow))
            {
                if (rowSums[prevRow] != originalRowSums[prevRow])
                {
                    // Debug.Log($"Row {prevRow} updated: {rowSums[prevRow]} (original: {originalRowSums[prevRow]})");
                    AlterMonitorState(prevRow, false, true);
                } else
                {
                    AlterMonitorState(prevRow, false, false);
                }
            }
        }
    }

    private void AlterMonitorState(
        int currentIndex,
        bool isCol,
        bool isWrong
    )
    {
        Vector3Int currentMonitorGridPosition;
        TileBase newMonitorTile;

        if (isCol)
        {
            currentMonitorGridPosition = new Vector3Int(currentIndex, -5, 0);
        }
        else
        {
            currentMonitorGridPosition = new Vector3Int(1, currentIndex, 0);
        }

        if (isWrong)
        {
            newMonitorTile = tileDataList[8].tiles[0];
        }
        else
        {
            newMonitorTile = tileDataList[7].tiles[0];
        }
        
        TileBase monitorTile = map.GetTile(currentMonitorGridPosition);
        if (monitorTile != null)
        {
            // TileData monitorTileData = dataFromTiles[monitorTile];
            // Debug.Log($"Modifying {currentMonitorGridPosition}, {isCol}, previouly {monitorTileData.isWrong} to {isWrong}");
            // monitorTileData.isWrong = isWrong;
            map.SetTile(currentMonitorGridPosition, newMonitorTile);
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
        if (playerValue == 6969724)
        {
            // special switch for start screen
            return false;
        }
        Vector3Int gridPosition = map.WorldToCell(worldPosition);
        TileBase tile = map.GetTile(gridPosition);

        if (tile != null && dataFromTiles.TryGetValue(tile, out TileData tileData))
        {
            if (tileData.isMonitor)
            {
                // Debug.Log($"I should work!, {tileData.isWrong}, player value: {playerValue}");
                return (!tileData.isWrong) | (playerValue != 0);
            }
        }

        return false;
    }

    
}
