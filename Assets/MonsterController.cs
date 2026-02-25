using UnityEngine;
using System;

public class MonsterController : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer monsterRenderer;
    public Transform eyeSocket;
    public Transform fingerTip;
    public Transform ring;
    public SpriteRenderer ringRenderer;

    [Header("State Settings")]
    public float alertDistance = 2.5f;
    public float alertCloseDuration = 1.25f;
    public float alertOpenDuration = 0.3f;
    public float mouthCountdownDuration = 2f;
    public float biteCloseDuration = 0.12f;
    public int biteThresholdFrameOffset = 1;

    [Header("Chomp Bite Settings")]
    public float chompOpenDuration = 0.08f;
    public float chompCloseDuration = 0.06f;
    public int chompOpenFrameOffset = 2;
    public int biteChompCount = 3;
    public float biteChompOpenDuration = 0.06f;
    public float biteChompCloseDuration = 0.04f;
    public int biteChompOpenFrameOffset = 3;

    public int ringVisibleMinFrameIndex = 5;

    private float countdownTimer = 0f;
    private bool fingerInsideMouth = false;
    private bool biteTriggered = false;
    private int currentFrameIndex = -1;
    private bool ringCollected = false;
    private bool biteAnimationPlaying = false;
    private bool biteEventRaised = false;
    private float biteTimer = 0f;
    private int biteStartFrame = -1;
    private bool alertClosingStarted = false;
    private float alertCloseTimer = 0f;
    private int alertStartFrame = -1;
    private bool alertOpening = false;
    private float alertOpenTimer = 0f;
    private int alertOpenStartFrame = -1;
    private bool isChompBite = false;
    private float chompTimer = 0f;
    private int biteChompIndex = 0;

    public event Action BiteTriggered;

    [System.Serializable]
    public struct MonsterFrame
    {
        public Sprite mouthSprite;
        public Vector2 eyeSocketLocalPosition;
    }

    [Header("Animation Frames")]
    public MonsterFrame[] frames;

    public bool IsFingerInsideMouth => fingerInsideMouth;
    public float RemainingTime => Mathf.Max(0f, countdownTimer);

    void Awake()
    {
        if (monsterRenderer == null)
        {
            monsterRenderer = GetComponent<SpriteRenderer>();
        }

        if (ringRenderer == null && ring != null)
        {
            ringRenderer = ring.GetComponent<SpriteRenderer>();
            if (ringRenderer == null)
            {
                ringRenderer = ring.GetComponentInChildren<SpriteRenderer>();
            }
        }
    }

    void Start()
    {
        SetFrame(GetOpenFrameIndex());
        UpdateRingVisibility();
    }

    void Update()
    {
        if (frames == null || frames.Length == 0 || monsterRenderer == null || fingerTip == null || ring == null)
        {
            return;
        }

        if (ringCollected)
        {
            UpdateRingVisibility();
            return;
        }

        if (biteTriggered)
        {
            if (biteAnimationPlaying)
            {
                chompTimer += Time.deltaTime;
                int peakFrame = Mathf.Min(GetClosedFrameIndex() + biteChompOpenFrameOffset, GetOpenFrameIndex());
                float singleChompDuration = biteChompOpenDuration + biteChompCloseDuration;
                float totalDuration = singleChompDuration * biteChompCount;

                if (chompTimer < totalDuration)
                {
                    // Determine which chomp cycle we're in
                    float timeInCycle = chompTimer % singleChompDuration;

                    if (timeInCycle < biteChompOpenDuration)
                    {
                        // Opening phase of chomp
                        float t = Mathf.Clamp01(timeInCycle / Mathf.Max(0.01f, biteChompOpenDuration));
                        SetFrame(Mathf.RoundToInt(Mathf.Lerp(GetClosedFrameIndex(), peakFrame, t)));
                    }
                    else
                    {
                        // Closing phase of chomp
                        float t = Mathf.Clamp01((timeInCycle - biteChompOpenDuration) / Mathf.Max(0.01f, biteChompCloseDuration));
                        SetFrame(Mathf.RoundToInt(Mathf.Lerp(peakFrame, GetClosedFrameIndex(), t)));
                    }
                }
                else
                {
                    // All chomps done — hold closed and fire event
                    SetFrame(GetClosedFrameIndex());
                    biteAnimationPlaying = false;
                    if (!biteEventRaised)
                    {
                        biteEventRaised = true;
                        BiteTriggered?.Invoke();
                    }
                }
            }

            UpdateRingVisibility();
            return;
        }

        if (ringCollected)
        {
            UpdateRingVisibility();
            return;
        }

        // --- Distance-based alert mouth closing/opening ---
        float distanceToRing = Vector2.Distance(fingerTip.position, ring.position);

        // --- Smooth opening animation when finger leaves alert zone ---
        if (alertOpening)
        {
            if (distanceToRing <= alertDistance)
            {
                // Finger came back — switch to closing from wherever we are
                alertOpening = false;
                alertClosingStarted = true;
                alertCloseTimer = 0f;
                alertStartFrame = currentFrameIndex >= 0 ? currentFrameIndex : GetOpenFrameIndex();
            }
            else
            {
                alertOpenTimer += Time.deltaTime;
                float t = Mathf.Clamp01(alertOpenTimer / Mathf.Max(0.01f, alertOpenDuration));
                int frameIndex = Mathf.RoundToInt(Mathf.Lerp(alertOpenStartFrame, GetOpenFrameIndex(), t));
                SetFrame(frameIndex);
                UpdateRingVisibility();

                if (t >= 1f)
                {
                    alertOpening = false;
                }
                return;
            }
        }

        if (!alertClosingStarted)
        {
            if (distanceToRing <= alertDistance)
            {
                alertClosingStarted = true;
                alertCloseTimer = 0f;
                alertStartFrame = currentFrameIndex >= 0 ? currentFrameIndex : GetOpenFrameIndex();
            }
            else
            {
                SetFrame(GetOpenFrameIndex());
                UpdateRingVisibility();
                return;
            }
        }
        else if (distanceToRing > alertDistance && !fingerInsideMouth)
        {
            // Finger left alert zone — start smooth opening
            alertClosingStarted = false;
            alertCloseTimer = 0f;
            alertOpening = true;
            alertOpenTimer = 0f;
            alertOpenStartFrame = currentFrameIndex >= 0 ? currentFrameIndex : GetClosedFrameIndex();
            UpdateRingVisibility();
            return;
        }

        alertCloseTimer += Time.deltaTime;
        float alertT = Mathf.Clamp01(alertCloseTimer / Mathf.Max(0.01f, alertCloseDuration));
        int alertFrame = Mathf.RoundToInt(Mathf.Lerp(alertStartFrame, GetClosedFrameIndex(), alertT));
        SetFrame(alertFrame);

        // Mouth fully closed AND finger is inside MouthZone → bite!
        if (alertT >= 1f && fingerInsideMouth)
        {
            TriggerBite();
            return;
        }

        UpdateRingVisibility();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (fingerTip == null || biteTriggered || ringCollected)
        {
            return;
        }

        if (!IsFingerTipCollider(other))
        {
            return;
        }

        NotifyFingerEnteredMouth();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (fingerTip == null)
        {
            return;
        }

        if (!IsFingerTipCollider(other))
        {
            return;
        }

        NotifyFingerExitedMouth();
    }

    public void NotifyFingerEnteredMouth()
    {
        if (biteTriggered || ringCollected) return;

        fingerInsideMouth = true;

        // If mouth is already fully closed when finger enters → immediate bite
        if (IsMouthFullyClosed())
        {
            TriggerBite();
        }
    }

    public void NotifyFingerExitedMouth()
    {
        if (biteTriggered || ringCollected) return;

        fingerInsideMouth = false;
        countdownTimer = 0f;
        alertClosingStarted = false;
        alertCloseTimer = 0f;

        // Start smooth opening from wherever the mouth currently is
        alertOpening = true;
        alertOpenTimer = 0f;
        alertOpenStartFrame = currentFrameIndex >= 0 ? currentFrameIndex : GetClosedFrameIndex();
    }

    public bool CanCollectRingNow()
    {
        if (biteTriggered || ringCollected) return false;

        // Ring is collectible when the mouth is open enough (not near-closed)
        int biteThreshold = GetClosedFrameIndex() + biteThresholdFrameOffset;
        return currentFrameIndex > biteThreshold;
    }

    public bool IsMouthFullyClosed()
    {
        return currentFrameIndex >= 0 && currentFrameIndex <= GetClosedFrameIndex();
    }

    public bool IsMouthDangerous()
    {
        // Already biting
        if (biteTriggered) return true;

        // Mouth is fully closed
        if (IsMouthFullyClosed()) return true;

        // Alert closing is ≥70% complete
        if (alertClosingStarted && alertCloseTimer >= alertCloseDuration * 0.7f) return true;

        return false;
    }

    public void OnRingCollected()
    {
        ringCollected = true;
        fingerInsideMouth = false;
        countdownTimer = 0f;
        alertClosingStarted = false;
        alertCloseTimer = 0f;
        SetFrame(GetOpenFrameIndex());
        UpdateRingVisibility();
    }

    public void TriggerBite()
    {
        if (biteTriggered || ringCollected) return;

        biteTriggered = true;
        fingerInsideMouth = false;
        countdownTimer = 0f;
        alertClosingStarted = false;
        alertCloseTimer = 0f;
        alertOpening = false;
        biteEventRaised = false;

        // Always play rapid chomp animation
        chompTimer = 0f;
        biteChompIndex = 0;
        biteAnimationPlaying = true;

        // Snap to closed first so the chomping starts from closed
        SetFrame(GetClosedFrameIndex());
        UpdateRingVisibility();
    }

    int GetClosedFrameIndex()
    {
        if (frames == null || frames.Length == 0) return 0;
        return 0;
    }

    int GetOpenFrameIndex()
    {
        if (frames == null || frames.Length == 0) return 0;
        return frames.Length - 1;
    }

    int GetAlmostClosedFrameIndex()
    {
        if (frames == null || frames.Length < 2) return GetClosedFrameIndex();
        return 1;
    }

    int GetFrameIndexFromOpenPercent(float openPercent)
    {
        int closed = GetClosedFrameIndex();
        int open = GetOpenFrameIndex();
        return Mathf.RoundToInt(Mathf.Lerp(closed, open, Mathf.Clamp01(openPercent)));
    }

    void SetFrame(int frameIndex)
    {
        if (frames == null || frames.Length == 0 || monsterRenderer == null) return;

        frameIndex = Mathf.Clamp(frameIndex, 0, frames.Length - 1);
        if (currentFrameIndex == frameIndex) return;

        currentFrameIndex = frameIndex;
        monsterRenderer.sprite = frames[frameIndex].mouthSprite;

        if (eyeSocket != null)
        {
            eyeSocket.localPosition = frames[frameIndex].eyeSocketLocalPosition;
        }
    }

    void UpdateRingVisibility()
    {
        if (ringRenderer == null) return;

        if (ringCollected || biteTriggered)
        {
            ringRenderer.enabled = false;
            return;
        }

        int minVisibleFrame = Mathf.Clamp(ringVisibleMinFrameIndex, 0, GetOpenFrameIndex());
        ringRenderer.enabled = currentFrameIndex >= minVisibleFrame;
    }

    bool IsFingerTipCollider(Collider2D other)
    {
        return other != null && (other.transform == fingerTip || other.transform.IsChildOf(fingerTip));
    }
}