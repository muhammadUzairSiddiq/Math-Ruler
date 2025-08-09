using UnityEngine;
using UnityEngine.EventSystems;

public class CubeTouchDetector : MonoBehaviour, IPointerDownHandler
{
    [Header("Touch Settings")]
    public bool showDebugInfo = true;
    
    private ARNumberLineGenerator numberLineGenerator;
    private int cubeNumber;
    
    void Start()
    {
        // Find the number line generator
        numberLineGenerator = FindObjectOfType<ARNumberLineGenerator>();
        
        // Get cube number from name
        if (int.TryParse(gameObject.name, out int number))
        {
            cubeNumber = number;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"CubeTouchDetector initialized for cube {cubeNumber} - {gameObject.name}");
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (numberLineGenerator == null) return;
        Debug.Log($"[TOUCH] PointerDown at screen position: {eventData.position} on cube: {gameObject.name} (Number: {cubeNumber})");
        // Check if this cube is touchable (first or last visible cube)
        if (numberLineGenerator.IsCubeTouchable(cubeNumber))
        {
            Debug.Log($"[TOUCH] Cube {gameObject.name} (Number: {cubeNumber}) is expandable. Expanding...");
            numberLineGenerator.OnCubeTouched(gameObject);
        }
        else
        {
            Debug.Log($"[TOUCH] Cube {gameObject.name} (Number: {cubeNumber}) is NOT expandable.");
        }
    }
    
    // Alternative touch detection for mobile
    void Update()
    {
        if (numberLineGenerator == null) return;
        // Handle touch input for mobile devices
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                // Check if touch is on this cube
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        Debug.Log($"[TOUCH] Raycast touch at screen position: {touch.position} hit cube: {gameObject.name} (Number: {cubeNumber})");
                        if (numberLineGenerator.IsCubeTouchable(cubeNumber))
                        {
                            Debug.Log($"[TOUCH] Cube {gameObject.name} (Number: {cubeNumber}) is expandable. Expanding...");
                            numberLineGenerator.OnCubeTouched(gameObject);
                        }
                        else
                        {
                            Debug.Log($"[TOUCH] Cube {gameObject.name} (Number: {cubeNumber}) is NOT expandable.");
                        }
                    }
                }
            }
        }
    }
    
    // Visual feedback for touchable cubes
    void OnDrawGizmos()
    {
        if (numberLineGenerator != null && numberLineGenerator.IsCubeTouchable(cubeNumber))
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, transform.localScale * 1.1f);
        }
    }
} 