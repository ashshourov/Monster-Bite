using UnityEngine;
using UnityEngine.SceneManagement; // Needed to restart game

public class GameManager : MonoBehaviour
{
    public HandController handController;
    public MonsterController monsterController;
    [Header("Scene Flow")]
    public string nextSceneName = ""; // Optional. If empty, loads next build-index scene.

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

    public void TryCollectRing(GameObject ringObject)
    {
        if (gameEnded) return;

        Debug.Log("TryCollectRing called. Checking conditions...");
        Debug.Log($"  - monsterController is null: {monsterController == null}");
        Debug.Log($"  - handController is null: {handController == null}");
        
        if (monsterController == null)
        {
            Debug.LogError("TryCollectRing: MonsterController is null!");
            return;
        }
        
        bool canTakeRing = monsterController.CanCollectRingNow();
        Debug.Log($"  - CanCollectRingNow: {canTakeRing}");
        Debug.Log($"  - IsTimerRunning: {monsterController.IsTimerRunning}");
        Debug.Log($"  - CountdownTimer: {monsterController.RemainingTime}");
        Debug.Log($"  - FingerInsideMouth: {monsterController.IsFingerInsideMouth}");

        if (canTakeRing)
        {
            Debug.Log("✓ Ring collection SUCCESSFUL! Hand sprite changing now...");
            if (handController != null)
            {
                handController.CollectRing();
                Debug.Log("HandController.CollectRing() called");
            }
            else
            {
                Debug.LogError("HandController is null! Cannot change sprite!");
            }
            
            monsterController.OnRingCollected();
            WinLevel();
        }
        else
        {
            Debug.LogWarning("✗ Ring touched but timer not active or expired. Conditions not met for collection.");
        }
    }

    void HandleBite()
    {
        if (gameEnded) return;

        gameEnded = true;
        handController.GetBitten();
        Debug.Log("GAME OVER: Monster bite.");
    }

    void WinLevel()
    {
        gameEnded = true;
        Debug.Log("LEVEL WON! Loading next level...");

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
            gameEnded = false;
        }
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}