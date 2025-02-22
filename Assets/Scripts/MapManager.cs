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
        BoundsInt bounds = map.cellBounds;

        for (int x = -7; x < 1; x++)
        {
            for (int y = -4; y < 4; y++)
            {
                Vector3Int gridPosition = new Vector3Int(x, y, 0);
                TileBase tile = map.GetTile(gridPosition);
                if (!rowSums.ContainsKey(y)) rowSums[y] = 0;
                if (!colSums.ContainsKey(x)) colSums[x] = 0;

                if (tile != null && dataFromTiles.TryGetValue(tile, out TileData tileData))
                {
                    rowSums[y] += tileData.value;

                    colSums[x] += tileData.value;
                }
            }
        }

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
    
    public void UpdateSums(Vector2 position, int oldValue, int newValue)
    {
        Vector3Int gridPosition = map.WorldToCell(position);
        int row = gridPosition.y;
        int col = gridPosition.x;
        
        Debug.Log($"pre-update Row {row}: {rowSums[row]}, Col {col}: {colSums[col]}");
        
        rowSums[row] += newValue - oldValue;
        colSums[col] += newValue - oldValue;
        
        Debug.Log($"post-update Row {row}: {rowSums[row]}, Col {col}: {colSums[col]}");
        
        // // Check if monitor tiles need to be updated
        // UpdateMonitorTiles();
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
            
            if (!tileData.isNonInteractive)
            {
                /**
                 * the reason we don't just code 0 as the value of 7-tile is
                 *  we still want to use 7 for calculating row/col sum  
                 */
                return tileData.value;
            }
            
        }
        
        return 0;
    }
    
    public void RemoveTile(Vector2 worldPosition)
    {
        Vector3Int gridPosition = map.WorldToCell(worldPosition);
        TileBase oldTile = map.GetTile(gridPosition);
        int oldValue = dataFromTiles.ContainsKey(oldTile) ? dataFromTiles[oldTile].value : 0;

        map.SetTile(gridPosition, null);

        UpdateSums(worldPosition, oldValue, 0);
    }
    
    public void ReplaceTileWithSeven(Vector2 worldPosition)
    {
        Vector3Int gridPosition = map.WorldToCell(worldPosition);
        TileBase newTile = tileDataList[6].tiles[0]; // Last tile in list as replacement

        TileBase oldTile = map.GetTile(gridPosition);
        int oldValue = dataFromTiles.ContainsKey(oldTile) ? dataFromTiles[oldTile].value : 0;
        int newValue = 7;

        map.SetTile(gridPosition, newTile);

        UpdateSums(worldPosition, oldValue, newValue);
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
