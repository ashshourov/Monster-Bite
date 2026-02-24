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
    public float mouthCountdownDuration = 2f;
    public float biteCloseDuration = 0.12f;
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
    public bool IsTimerRunning => fingerInsideMouth && !biteTriggered && !ringCollected;
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
                biteTimer += Time.deltaTime;
                float t = Mathf.Clamp01(biteTimer / Mathf.Max(0.01f, biteCloseDuration));
                int frameIndex = Mathf.RoundToInt(Mathf.Lerp(biteStartFrame, GetClosedFrameIndex(), t));
                SetFrame(frameIndex);

                if (t >= 1f)
                {
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

        if (fingerInsideMouth)
        {
            countdownTimer -= Time.deltaTime;
            if (countdownTimer <= 0f)
            {
                countdownTimer = 0f;
                TriggerBite();
                return;
            }

            float remainingPercent = mouthCountdownDuration <= 0f ? 0f : countdownTimer / mouthCountdownDuration;
            int openFrame = GetOpenFrameIndex();
            int almostClosedFrame = GetAlmostClosedFrameIndex();
            int frameIndex = Mathf.RoundToInt(Mathf.Lerp(almostClosedFrame, openFrame, remainingPercent));
            SetFrame(frameIndex);
            UpdateRingVisibility();
            return;
        }

        float distanceToRing = Vector2.Distance(fingerTip.position, ring.position);

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
            alertClosingStarted = false;
            alertCloseTimer = 0f;
            SetFrame(GetOpenFrameIndex());
            UpdateRingVisibility();
            return;
        }

        alertCloseTimer += Time.deltaTime;
        float alertT = Mathf.Clamp01(alertCloseTimer / Mathf.Max(0.01f, alertCloseDuration));
        int alertFrame = Mathf.RoundToInt(Mathf.Lerp(alertStartFrame, GetClosedFrameIndex(), alertT));
        SetFrame(alertFrame);

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
        countdownTimer = Mathf.Max(0f, mouthCountdownDuration);
        SetFrame(GetOpenFrameIndex());
    }

    public void NotifyFingerExitedMouth()
    {
        if (biteTriggered || ringCollected) return;

        fingerInsideMouth = false;
        countdownTimer = 0f;
        alertClosingStarted = false;
        alertCloseTimer = 0f;
    }

    public bool CanCollectRingNow()
    {
        return IsTimerRunning && countdownTimer > 0f;
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
        biteAnimationPlaying = true;
        biteTimer = 0f;
        biteEventRaised = false;
        biteStartFrame = currentFrameIndex >= 0 ? currentFrameIndex : GetOpenFrameIndex();

        if (biteStartFrame <= GetClosedFrameIndex())
        {
            SetFrame(GetClosedFrameIndex());
            biteAnimationPlaying = false;
            biteEventRaised = true;
            BiteTriggered?.Invoke();
        }

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