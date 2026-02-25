using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TryAgainUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popup;          // The panel/container that holds the button
    public Button tryAgainButton;

    [Header("Settings")]
    public float showDelay = 3f;

    void Awake()
    {
        if (popup != null)
        {
            popup.SetActive(false);
        }

        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.AddListener(OnTryAgainClicked);
        }
    }

    /// <summary>
    /// Call this to start the delayed popup after win or lose.
    /// </summary>
    public void ShowAfterDelay()
    {
        Invoke(nameof(ShowPopup), showDelay);
    }

    void ShowPopup()
    {
        if (popup != null)
        {
            popup.SetActive(true);
        }
    }

    void OnTryAgainClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
