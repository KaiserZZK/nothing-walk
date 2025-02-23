using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class StatusUIManager : MonoBehaviour
{
    [SerializeField] 
    private Tilemap map;

    [SerializeField] private List<Button> tileDataList;
    private Dictionary<TileBase, Button> dataFromTiles;

    // Start is called before the first frame update
    void Awake()
    {
        dataFromTiles = new Dictionary<TileBase, Button>();
        foreach (var tileData in tileDataList)
        {
            dataFromTiles.Add(tileData.tile, tileData);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = map.WorldToCell(mousePos);
            
            TileBase clickedTile = map.GetTile(gridPosition);
            if (clickedTile != null)
            {
                Button tileData = dataFromTiles[clickedTile];
                if (tileData.isHome)
                {
                    SceneController.sceneInstance.LoadScene("0 Start");
                } else if (tileData.isRestart)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
                
            }
            
        }
    }
}
