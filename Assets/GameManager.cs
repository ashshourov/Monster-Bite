using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public HandController handController;
    public MonsterController monsterController;
    
    [Header("Scene Flow")]
    public string nextSceneName = ""; 
    public float delayBeforeNextLevel = 1.5f; // Added a delay so you can see the sprites!

    private bool gameEnded = false;

    void OnEnable()
    {
        if (monsterController != null)
        {
            monsterController.BiteTriggered += HandleBite;
        }
    }

    void OnDisable()
    {
        if (monsterController != null)
        {
            monsterController.BiteTriggered -= HandleBite;
        }
    }

    // --- THIS IS WHAT THE AGENT FORGOT ---
    // This detects the physics collision between the finger and the ring
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameEnded) return;

        // If the finger touches the object tagged "Ring"
        if (other.CompareTag("Ring"))
        {
            TryCollectRing(other.gameObject);
        }
    }
    // -------------------------------------

    public void TryCollectRing(GameObject ringObject)
    {
        if (gameEnded) return;

        if (monsterController == null) return;
        
        bool canTakeRing = monsterController.CanCollectRingNow();

        if (canTakeRing)
        {
            if (handController != null)
            {
                handController.CollectRing();
            }
            
            monsterController.OnRingCollected();
            WinLevel();
        }
        else
        {
            // Finger touched ring while mouth is closed — monster bites!
            Debug.Log("Ring touched with mouth closed — BITE!");
            monsterController.TriggerBite();
        }
    }

    void HandleBite()
    {
        if (gameEnded) return;

        gameEnded = true;
        
        if (handController != null)
        {
            handController.GetBitten();
        }
        
        Debug.Log("GAME OVER: Monster bite.");
        
        // Optional: Restart the level after getting bitten
        // Invoke("RestartLevel", delayBeforeNextLevel); 
    }

    void WinLevel()
    {
        gameEnded = true;
        Debug.Log("LEVEL WON! Waiting to load next level...");

        // Invoke calls the function after a delay, so you can see the ring in your hand!
        Invoke("LoadNextScene", delayBeforeNextLevel);
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log("No next scene in Build Settings. Staying on current level.");
            gameEnded = false; // Reset if there's nowhere to go
        }
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}