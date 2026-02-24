using UnityEngine;

public class GameManagerRiz : MonoBehaviour
{

    public static GameManagerRiz Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }
    }

    public void onGameLost()
    {
        Debug.Log("GameOver");
    }

    public void onGameWon()
    {
        Debug.Log("Won");
    }


}
