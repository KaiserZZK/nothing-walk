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
    public bool isWrong = false;

}
