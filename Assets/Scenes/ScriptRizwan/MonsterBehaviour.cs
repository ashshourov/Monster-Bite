using UnityEngine;

public class MonsterBehaviour : MonoBehaviour
{
    [Header("Distance Settings")]
    public Transform mouthZone;
    public float minDistanceToMouth = 1f;
    public float biteDistance = 0.4f;
    public Transform fingerTip;
    
    [Header("References")]
    public TouchController touch; // ADDED: This fixes the error!
    public GameObject ringObject; 

    private Animator animator;
    private bool isOpened = false;
    private bool isBitten = false;

    void Start()
    {
        // ADDED: This ensures the animator variable isn't empty!
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isBitten || animator == null) return;

        if(Vector3.Distance(fingerTip.position, mouthZone.position) < minDistanceToMouth)
        {
            if (!isOpened) // Only play it once when entering the zone
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
    }

    public void Bitten()
    {
        Debug.Log(Vector3.Distance(fingerTip.position, mouthZone.position));
        if (Vector3.Distance(fingerTip.position, mouthZone.position) < biteDistance)
        {
            if (touch != null) touch.CutFinger();
        }
    }

    // --- ANIMATION EVENTS ---
    public void ShowRingInMouth()
    {
        if (ringObject != null) ringObject.SetActive(true);
    }

    public void HideRingInMouth()
    {
        if (ringObject != null) ringObject.SetActive(false);
    }
}