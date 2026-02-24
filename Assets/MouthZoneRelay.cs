using UnityEngine;

public class MouthZoneRelay : MonoBehaviour
{
    public MonsterController monsterController;
    public Transform fingerTip;

    void Awake()
    {
        if (monsterController == null)
        {
            monsterController = GetComponentInParent<MonsterController>();
        }

        if (fingerTip == null && monsterController != null)
        {
            fingerTip = monsterController.fingerTip;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (monsterController == null || fingerTip == null) return;
        if (other.transform == fingerTip || other.transform.IsChildOf(fingerTip))
        {
            monsterController.NotifyFingerEnteredMouth();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (monsterController == null || fingerTip == null) return;
        if (other.transform == fingerTip || other.transform.IsChildOf(fingerTip))
        {
            monsterController.NotifyFingerExitedMouth();
        }
    }
}
