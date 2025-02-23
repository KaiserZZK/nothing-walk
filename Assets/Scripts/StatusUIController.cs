using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusUIController : MonoBehaviour
{
    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer statusRenderer;
    
    private Sprite currentSprite;
    private Sprite currentStatusSprite;
    [SerializeField] Sprite[] numberedSprites;
    [SerializeField] Sprite[] numberedStatusSprites;

    [SerializeField] private bool updateStoppedDueToStartScreen = false;
    
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = transform.Find("Sprite UI").GetComponent<SpriteRenderer>();
        statusRenderer = transform.Find("Status UI").GetComponent<SpriteRenderer>();
        playerController = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!updateStoppedDueToStartScreen)
        {
            currentSprite = numberedSprites[playerController.currentValue];
            spriteRenderer.sprite = currentSprite;
            currentStatusSprite = numberedStatusSprites[playerController.currentValue];
            statusRenderer.sprite = currentStatusSprite;
            
        }
    }
}
