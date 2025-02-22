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


    private void Start()
    {
        dataFromTiles = new Dictionary<TileBase, TileData>();
        
        // TODO also build initial row/col values mapping 

        foreach (var tileData in tileDataList)
        {
            foreach (var tile in tileData.tiles)
            {
                dataFromTiles.Add(tile, tileData);
            }
        }
        
    }

    private void Update()
    {
        // if (Input.GetMouseButtonDown(0))
        // {
        //     Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //     Vector3Int gridPosition = map.WorldToCell(mousePosition);
        //     
        //     TileBase clickedTile = map.GetTile(gridPosition);
        //     TileData tileData = dataFromTiles[clickedTile];
        //     if (!tileData.isMonitor)
        //     {
        //         int value = tileData.value;
        //         print("At tile: " + clickedTile + ", value: " + value);
        //     }
        //     else
        //     {
        //         if (!tileData.isRow)
        //         {
        //             
        //         }
        //         else
        //         {
        //             
        //         }
        //     }
        //     
        // }
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
}
