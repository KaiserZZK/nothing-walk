using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileData : ScriptableObject
{
    public TileBase[] tiles;

    public int value;

    // Monitor attributes (blank by default)
    public bool isMonitor; 
    public bool isRow;
    public int rowIndex;
    public int columnIndex;
    public int currentSum;
    
    public bool isWrong = false;

    private void Update()
    {
        // TODO check sum against original data 
        
        // TODO change sprite if condition met
        if (isWrong)
        {
            Sprite sprite = Resources.Load<Sprite>("Sprites/WrongTile");
        }
        
    }
}
