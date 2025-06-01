using UnityEngine;
using UnityEngine.UI;

public class MapZoomController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 3f;
    [SerializeField] private float zoomSpeed = 0.5f;
    [SerializeField] private float mouseWheelZoomSpeed = 0.1f;

    [Header("Drag Settings")]
    [SerializeField] private float dragSensitivity = 1f;
    [SerializeField] private float maxDragDistance = 500f; // Maximum distance map can be dragged from center

    [Header("References")]
    [SerializeField] private RectTransform mapRect;

    private float currentZoom = 1f;
    private Vector2 lastTouchDistance;
    private bool isPinching = false;
    
    // Drag variables
    private bool isDragging = false;
    private Vector2 lastDragPosition;
    private Vector2 mapStartPosition;

    private void Start()
    {
        if (mapRect != null)
        {
            mapStartPosition = mapRect.anchoredPosition;
        }
    }

    private void Update()
    {
        // Handle touch input (mobile)
        if (Input.touchCount == 2)
        {
            HandlePinchZoom();
            isDragging = false; // Stop dragging when pinching
        }
        else if (Input.touchCount == 1)
        {
            HandleTouchDrag();
        }
        // Handle mouse input (editor)
        else if (Input.touchCount == 0)
        {
            HandleMouseWheelZoom();
            HandleMouseDrag();
        }
    }

    private void HandleTouchDrag()
    {
        Touch touch = Input.GetTouch(0);
        
        if (touch.phase == TouchPhase.Began)
        {
            StartDrag(touch.position);
        }
        else if (touch.phase == TouchPhase.Moved && isDragging)
        {
            UpdateDrag(touch.position);
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            EndDrag();
        }
    }

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }

    private void StartDrag(Vector2 inputPosition)
    {
        isDragging = true;
        lastDragPosition = inputPosition;
    }

    private void UpdateDrag(Vector2 inputPosition)
    {
        if (!isDragging || mapRect == null) return;

        // Calculate drag delta in screen space
        Vector2 dragDelta = (inputPosition - lastDragPosition) * dragSensitivity;
        
        // Apply drag to map position
        Vector2 newPosition = mapRect.anchoredPosition + dragDelta;
        
        // Apply clamping based on zoom level
        newPosition = ClampMapPosition(newPosition);
        
        mapRect.anchoredPosition = newPosition;
        lastDragPosition = inputPosition;
    }

    private void EndDrag()
    {
        isDragging = false;
    }

    private Vector2 ClampMapPosition(Vector2 position)
    {
        if (mapRect == null) return position;

        // Calculate maximum drag distance based on zoom level and actual map size
        // At 1x zoom: no dragging allowed (map fits perfectly in view)
        // At higher zooms: allow dragging based on how much content is off-screen
        Vector2 maxDrag = Vector2.zero;
        
        if (currentZoom > 1f)
        {
            // Get the actual size of the map RectTransform
            Rect mapSize = mapRect.rect;
            
            // Calculate how much content is off-screen on each side
            // When zoomed in by factor Z, the visible area is mapSize/Z
            // So the hidden content on each side is: (mapSize - mapSize/Z) / 2 = mapSize * (Z-1) / (2*Z)
            // But we want to allow full access to hidden content, so we use: mapSize * (Z-1) / 2
            float maxDragX = Mathf.Abs(mapSize.width) * (currentZoom - 1f) / 2f;
            float maxDragY = Mathf.Abs(mapSize.height) * (currentZoom - 1f) / 2f;
            
            maxDrag = new Vector2(maxDragX, maxDragY);
        }
        
        // Clamp the position within the allowed drag bounds
        float clampedX = Mathf.Clamp(position.x, mapStartPosition.x - maxDrag.x, mapStartPosition.x + maxDrag.x);
        float clampedY = Mathf.Clamp(position.y, mapStartPosition.y - maxDrag.y, mapStartPosition.y + maxDrag.y);
        
        return new Vector2(clampedX, clampedY);
    }

    private void HandlePinchZoom()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        // Get the current touch positions
        Vector2 touch0Pos = touch0.position;
        Vector2 touch1Pos = touch1.position;

        // Calculate the distance between touches
        Vector2 currentTouchDistance = touch0Pos - touch1Pos;

        if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
        {
            lastTouchDistance = currentTouchDistance;
            isPinching = true;
        }
        else if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
        {
            isPinching = false;
        }
        else if (isPinching)
        {
            // Calculate the difference in distance
            float distanceDelta = currentTouchDistance.magnitude - lastTouchDistance.magnitude;
            
            // Apply zoom
            float zoomDelta = distanceDelta * zoomSpeed * Time.deltaTime;
            UpdateZoom(zoomDelta);

            // Update last touch distance
            lastTouchDistance = currentTouchDistance;
        }
    }

    private void HandleMouseWheelZoom()
    {
        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0)
        {
            float zoomDelta = scrollDelta * mouseWheelZoomSpeed;
            UpdateZoom(zoomDelta);
        }
    }

    private void UpdateZoom(float zoomDelta)
    {
        // Calculate new zoom level
        float newZoom = currentZoom + zoomDelta;
        
        // Clamp zoom level
        newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);

        // Only update if zoom level changed
        if (newZoom != currentZoom)
        {
            currentZoom = newZoom;
            
            // Apply zoom to map
            if (mapRect != null)
            {
                mapRect.localScale = new Vector3(currentZoom, currentZoom, 1f);
                
                // Re-clamp position after zoom change
                mapRect.anchoredPosition = ClampMapPosition(mapRect.anchoredPosition);
            }
        }
    }

    // Public method to reset zoom and position
    public void ResetZoom()
    {
        currentZoom = 1f;
        if (mapRect != null)
        {
            mapRect.localScale = Vector3.one;
            mapRect.anchoredPosition = mapStartPosition;
        }
    }

    // Public method to get current zoom level
    public float GetCurrentZoom()
    {
        return currentZoom;
    }

    // Public method to set drag sensitivity
    public void SetDragSensitivity(float sensitivity)
    {
        dragSensitivity = sensitivity;
    }
} 