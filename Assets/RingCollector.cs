using UnityEngine;

public class RingCollector : MonoBehaviour
{
    public GameManager gameManager;
    
    void Start()
    {
        // Auto-find GameManager if not assigned
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }
        
        // Ensure this GameObject has the "Ring" tag
        gameObject.tag = "Ring";
        Debug.Log("RingCollector initialized. GameManager: " + (gameManager != null ? "Found" : "NOT FOUND"));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (gameManager == null)
        {
            Debug.LogError("RingCollector: GameManager not found!");
            return;
        }
        
        Debug.Log($"Ring collider touched by: {other.name} (tag: {other.tag})");
        
        // Check if FingerTip touched the ring (by name or tag)
        if (other.name.Contains("FingerTip") || other.name.Contains("Finger") || other.CompareTag("FingerTip"))
        {
            Debug.Log("Ring touched by FingerTip!");
            gameManager.TryCollectRing(gameObject);
        }
        else
        {
            Debug.LogWarning($"Ring touched but collider is not FingerTip. Collider name: {other.name}");
        }
    }
}
