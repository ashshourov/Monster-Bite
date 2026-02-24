using UnityEngine;

public class TouchController : MonoBehaviour
{
    private Camera mainCamera;
    private float objectZDistance;
    private bool isDragging = false;
    public Sprite ringFinger;
    public Sprite cutFinger;
    public MonsterBehaviour monster;

    void Start()
    {
        mainCamera = Camera.main;

        // Store distance from camera to object
        objectZDistance = mainCamera.WorldToScreenPoint(transform.position).z;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ring"))
        {
            collision.gameObject.SetActive(false);
            wearRing();
        }
    }

    void wearRing()
    {
        monster.playBite();
        gameObject.GetComponent<SpriteRenderer>().sprite = ringFinger;
    }

    public void CutFinger()
    {
        gameObject.GetComponent <SpriteRenderer>().sprite = cutFinger;
    }

    void Update()
    {
        // ===== MOBILE TOUCH =====
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            Vector3 touchPosition = new Vector3(touch.position.x, touch.position.y, objectZDistance);
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(touchPosition);

            if (touch.phase == TouchPhase.Began)
            {
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                transform.position = worldPosition;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isDragging = false;
            }
        }

        // ===== MOUSE (FOR EDITOR TESTING) =====
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, objectZDistance);
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            transform.position = worldPosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
#endif
    }
}