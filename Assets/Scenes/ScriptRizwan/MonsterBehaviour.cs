using UnityEngine;

public class MonsterBehaviour : MonoBehaviour
{
    public Transform mouthZone;
    public float minDistanceToMouth = 1f;
    public float biteDistance = 0.4f;
    public Transform fingerTip;
    private Animator animator;

    private bool isOpened = false;
    private bool isBitten = false;

    [SerializeField] private TouchController touch;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isBitten) return;

        if(Vector3.Distance(fingerTip.position, mouthZone.position) < minDistanceToMouth)
        {
            isOpened = true;
            animator.Play("Open");
        }
        else if(isOpened)
        {
            isOpened = false;
            animator.Play("Close");
        }
    }


    public void playBite()
    {
        animator.Play("Bite");
        isBitten = true;
    }

    public void Bitten()
    {
        Debug.Log(Vector3.Distance(fingerTip.position, mouthZone.position));
        if (Vector3.Distance(fingerTip.position, mouthZone.position) < biteDistance)
        {
            touch.CutFinger();
        }
    }
}
