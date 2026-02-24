using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Sprites")]
    public SpriteRenderer handRenderer;
    public Sprite handOpen;        
    public Sprite handWithRing;    
    public Sprite handCut;         
    public Sprite detachedFingerSprite;
    public Transform detachedFingerSpawnPoint;

    [Header("Bite Fall Settings")]
    public float detachedFingerGravity = 2.5f;
    public float detachedFingerSpin = 220f;
    public float destroyDetachedFingerAfter = 3f;
    public float detachedFingerScaleMultiplier = 0.22f;

    private bool isDead = false;
    private bool hasRing = false;

    // --- Dragging Variables ---
    private bool isDragging = false;
    private Vector3 dragOffset;

    void Start()
    {
        if (handRenderer != null && handOpen != null)
        {
            handRenderer.sprite = handOpen;
        }
    }

    void Update()
    {
        // Stop everything if the player is bitten
        if (isDead) return;

        // 1. Calculate where the mouse/touch is in the game world
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f; // Keep it flat on the 2D plane

        // 2. TOUCH START (Mouse Clicked)
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            // Calculate the distance between the center of the hand and where you clicked.
            // This prevents the hand from abruptly "teleporting" its center to your mouse.
            dragOffset = transform.position - mouseWorldPosition;
        }

        // 3. TOUCH HOLD (Dragging)
        if (Input.GetMouseButton(0) && isDragging)
        {
            // Move the hand exactly where the mouse goes, keeping that offset
            transform.position = mouseWorldPosition + dragOffset;
        }

        // 4. TOUCH END (Mouse Released)
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    // --- GAME LOGIC METHODS ---
    public void CollectRing()
    {
        Debug.Log("CollectRing() called!");
        Debug.Log($"  - handRenderer is null: {handRenderer == null}");
        Debug.Log($"  - handWithRing is null: {handWithRing == null}");
        
        hasRing = true;
        if (handRenderer != null && handWithRing != null)
        {
            Debug.Log("✓ Both handRenderer and handWithRing are valid. Changing sprite...");
            handRenderer.sprite = handWithRing;
            Debug.Log($"✓ Sprite changed! Current sprite: {handRenderer.sprite.name}");
        }
        else
        {
            Debug.LogError($"✗ Cannot change sprite! handRenderer: {handRenderer}, handWithRing: {handWithRing}");
        }
    }

    public void GetBitten()
    {
        Debug.Log("GetBitten() called!");
        Debug.Log($"  - isDead: {isDead}");
        
        if (isDead) return;

        isDead = true;
        Debug.Log($"  - handRenderer is null: {handRenderer == null}");
        Debug.Log($"  - handCut is null: {handCut == null}");
        
        if (handRenderer != null && handCut != null)
        {
            Debug.Log("✓ Both handRenderer and handCut are valid. Changing sprite...");
            handRenderer.sprite = handCut;
            Debug.Log($"✓ Bite sprite applied! Current sprite: {handRenderer.sprite.name}");
        }
        else
        {
            Debug.LogError($"✗ Cannot apply bite sprite! handRenderer: {handRenderer}, handCut: {handCut}");
        }
        
        SpawnDetachedFinger();
        Debug.Log("Detached finger spawned. Game Over!");
    }

    public bool HasRing()
    {
        return hasRing;
    }

    void SpawnDetachedFinger()
    {
        if (detachedFingerSprite == null) return;

        GameObject detachedFinger = new GameObject("DetachedFinger");
        detachedFinger.transform.position = detachedFingerSpawnPoint != null ? detachedFingerSpawnPoint.position : transform.position;
        detachedFinger.transform.rotation = transform.rotation;
        detachedFinger.transform.localScale = Vector3.Scale(transform.lossyScale, Vector3.one * Mathf.Max(0.01f, detachedFingerScaleMultiplier));

        SpriteRenderer detachedRenderer = detachedFinger.AddComponent<SpriteRenderer>();
        detachedRenderer.sprite = detachedFingerSprite;

        if (handRenderer != null)
        {
            detachedRenderer.sortingLayerID = handRenderer.sortingLayerID;
            detachedRenderer.sortingOrder = handRenderer.sortingOrder + 2;
        }

        Rigidbody2D rb = detachedFinger.AddComponent<Rigidbody2D>();
        rb.gravityScale = detachedFingerGravity;
        rb.angularVelocity = detachedFingerSpin;
        rb.linearVelocity = new Vector2(Random.Range(-0.6f, 0.6f), Random.Range(1.2f, 2.2f));

        Destroy(detachedFinger, destroyDetachedFingerAfter);
    }

}