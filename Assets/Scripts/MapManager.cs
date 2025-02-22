using System;
using System.Collections;
using System.Collections.Generic;
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

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int gridPosition = new Vector3Int(x, y, 0);
                TileBase tile = map.GetTile(gridPosition);

                if (tile != null && dataFromTiles.TryGetValue(tile, out TileData tileData))
                {
                    if (!rowSums.ContainsKey(y)) rowSums[y] = 0;
                    rowSums[y] += tileData.value;

                    if (!colSums.ContainsKey(x)) colSums[x] = 0;
                    colSums[x] += tileData.value;
                }
            }
        }

        // Save original sums for later comparisons
        originalRowSums = new Dictionary<int, int>(rowSums);
        originalColSums = new Dictionary<int, int>(colSums);
    }
    
    public void UpdateSums(Vector2 position, int oldValue, int newValue)
    {
        // Vector3Int gridPosition = map.WorldToCell(position);
        // int row = gridPosition.y;
        // int col = gridPosition.x;
        //
        // if (rowSums.ContainsKey(row)) rowSums[row] += newValue - oldValue;
        // if (colSums.ContainsKey(col)) colSums[col] += newValue - oldValue;
        //
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
            if (tileData.isMonitor)
            {
                return -1;
            }
            
            if (!tileData.isNonInteractive)
            {
                return tileData.value;
            }
            
        }
        
        return 0;
    }
    
    public void RemoveTile(Vector2 worldPosition)
    {
        Vector3Int gridPosition = map.WorldToCell(worldPosition);
        map.SetTile(gridPosition, null); 
    }
    
    public void ReplaceTileWithSeven(Vector2 worldPosition)
    {
        Vector3Int gridPosition = map.WorldToCell(worldPosition);
        map.SetTile(gridPosition, tileDataList[6].tiles[0]); 
    }
    
    public bool DoesTileBlockMovement(Vector2 worldPosition, int playerValue)
    {
        Vector3Int gridPosition = map.WorldToCell(worldPosition);
        TileBase tile = map.GetTile(gridPosition);

        if (tile != null && dataFromTiles.TryGetValue(tile, out TileData tileData))
        {
            if (tileData.isMonitor)
            {
                return (!tileData.isWrong) | (playerValue != 0);
            }
        }

        return false;
    }

    
}
