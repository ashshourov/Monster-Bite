using UnityEngine;

public class MonsterBehaviour : MonoBehaviour
{
    [Header("Distance Settings")]
    public Transform mouthZone;
    public float minDistanceToMouth = 1f;
    public float biteDistance = 0.4f;
    public Transform fingerTip;
    
    [Header("References")]
    public TouchController touch;
    public GameObject ringObject; 

    private Animator animator;
    private bool isOpened = false;
    private bool isBitten = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (ringObject == null)
            Debug.LogWarning("MonsterBehaviour: ringObject is NOT assigned in the Inspector!");
        else
            Debug.Log("MonsterBehaviour: ringObject is assigned -> " + ringObject.name);
    }

    void Update()
    {
        if (isBitten || animator == null) return;

        if(Vector3.Distance(fingerTip.position, mouthZone.position) < minDistanceToMouth)
        {
            if (!isOpened)
            {
                isOpened = true;
                animator.Play("Open");
            }
        }
        else if(isOpened)
        {
            isOpened = false;
            animator.Play("Close");
        }
    }

    public void playBite()
    {
        if (animator != null) animator.Play("Bite");
        isBitten = true;

        // Fallback: show ring immediately in case the animation event doesn't fire
        ShowRingInMouth();
    }

    public void Bitten()
    {
        Debug.Log(Vector3.Distance(fingerTip.position, mouthZone.position));
        if (Vector3.Distance(fingerTip.position, mouthZone.position) < biteDistance)
        {
            if (touch != null) touch.CutFinger();

            // Hide ring when the bite actually lands
            HideRingInMouth();
        }
    }

    // --- ANIMATION EVENTS ---
    public void ShowRingInMouth()
    {
        Debug.Log("ShowRingInMouth called | ringObject " + (ringObject != null ? "FOUND" : "NULL"));
        if (ringObject != null)
        {
            ringObject.SetActive(true);
        }
    }

    public void HideRingInMouth()
    {
        Debug.Log("HideRingInMouth called | ringObject " + (ringObject != null ? "FOUND" : "NULL"));
        if (ringObject != null)
        {
            ringObject.SetActive(false);
        }
    }
}