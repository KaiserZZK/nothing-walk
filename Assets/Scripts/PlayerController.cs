using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement-related
    public float moveSpeed = 5f;
    public Transform movePoint;

    public LayerMask whatStopsMovement;

    public Animator anim; 
    
    
    // Tilemap-related 
    private MapManager mapManager;
    [SerializeField] Sprite[] numberedSprites;
    [SerializeField] Sprite currentSprite;
    private SpriteRenderer spriteRenderer;
    private int currentValue = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        movePoint.parent = null;
        spriteRenderer = transform.Find("Player Sprite").GetComponent<SpriteRenderer>();
        mapManager = FindObjectOfType<MapManager>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, 
            movePoint.position, 
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, movePoint.position) <= 0.05f)
        {
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f)
            {
                if (
                    !Physics2D.OverlapCircle(
                        movePoint.position + new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f), 
                        .2f, 
                        whatStopsMovement
                    )
                    &&
                    !mapManager.DoesTileBlockMovement(
                        movePoint.position + new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f), 
                        currentValue
                    )
                )
                {
                    movePoint.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
                }
                UpdateValueAndSprite(movePoint.position);
            } else if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f)
            {
                if (
                    !Physics2D.OverlapCircle(
                        movePoint.position + new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f), 
                        .2f, 
                        whatStopsMovement
                    )
                    && 
                    !mapManager.DoesTileBlockMovement(
                        movePoint.position + new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f), 
                        currentValue
                    )
                )
                {
                    movePoint.position += new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);
                }
                UpdateValueAndSprite(movePoint.position);
            }
            
            anim.SetBool("moving", false);
        }
        else
        {
            anim.SetBool("moving", true);
        }
        
    }

    private void UpdateValueAndSprite(Vector2 position)
    {
        int tileValue = mapManager.GetTileValue(position);

        if (tileValue % 7 == 0 || tileValue == -1) return;

        currentValue += tileValue;
        currentValue %= 7;

        currentSprite = numberedSprites[currentValue];
        spriteRenderer.sprite = currentSprite;

        if (currentValue == 0)
        {
            mapManager.ReplaceTileWithSeven(position);
            // TODO update in UI panel on number of 7s created 
        }
        else
        {
            mapManager.RemoveTile(position);
        }
    }
}
