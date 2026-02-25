using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public HandController handController;
    public MonsterController monsterController;
    
    [Header("Scene Flow")]
    public string nextSceneName = ""; 
    public float delayBeforeNextLevel = 1.5f;

    [Header("UI")]
    public TryAgainUI tryAgainUI;

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

        // If the finger enters the MouthZone while mouth is dangerous → instant bite
        if (other.CompareTag("MouthZone"))
        {
            TryMouthZoneBite();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (gameEnded) return;

        // If the finger is resting inside MouthZone and mouth becomes dangerous → bite
        if (other.CompareTag("MouthZone"))
        {
            TryMouthZoneBite();
        }
    }
    // -------------------------------------

    void TryMouthZoneBite()
    {
        if (gameEnded) return;
        if (monsterController == null) return;

        if (monsterController.IsMouthDangerous())
        {
            Debug.Log("Finger in MouthZone while mouth is dangerous — BITE!");
            monsterController.TriggerBite();
        }
    }

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

        if (tryAgainUI != null)
        {
            tryAgainUI.ShowAfterDelay();
        }
    }

    void WinLevel()
    {
        gameEnded = true;
        Debug.Log("LEVEL WON!");

        if (tryAgainUI != null)
        {
            tryAgainUI.ShowAfterDelay();
        }
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