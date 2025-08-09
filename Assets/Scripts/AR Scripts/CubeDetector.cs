using UnityEngine;

public class CubeDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRadius = 0.5f;
    public LayerMask playerLayer = -1; // Default to all layers
    public bool showDebugInfo = true;
    
    private int cubeNumber;
    private ARNumberLineGenerator numberLineGenerator;
    private bool playerInside = false;
    private Collider cubeCollider;
    
    public void Initialize(int number, ARNumberLineGenerator generator)
    {
        cubeNumber = number;
        numberLineGenerator = generator;
        
        // Get or add collider
        cubeCollider = GetComponent<Collider>();
        if (cubeCollider == null)
        {
            cubeCollider = gameObject.AddComponent<BoxCollider>();
        }
        
        // Set as trigger
        cubeCollider.isTrigger = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"CubeDetector initialized for cube {cubeNumber} - {gameObject.name}");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player (camera in AR)
        if (IsPlayerObject(other))
        {
            if (!playerInside)
            {
                playerInside = true;
                
                if (showDebugInfo)
                {
                    Debug.Log($"🎯 Player entered cube: {gameObject.name} (Number: {cubeNumber})");
                }
                
                // Notify the number line generator
                if (numberLineGenerator != null)
                {
                    numberLineGenerator.HandlePlayerEnteredCube(cubeNumber);
                }
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // Check if the exiting object is the player
        if (IsPlayerObject(other))
        {
            if (playerInside)
            {
                playerInside = false;
                
                if (showDebugInfo)
                {
                    Debug.Log($"Player exited cube: {gameObject.name} (Number: {cubeNumber})");
                }
                
                // Notify the number line generator
                if (numberLineGenerator != null)
                {
                    numberLineGenerator.HandlePlayerExitedCube();
                }
            }
        }
    }
    
    bool IsPlayerObject(Collider other)
    {
        // Check for various player/camera objects
        return other.CompareTag("Player") || 
               other.CompareTag("MainCamera") || 
               other.name.Contains("Player") || 
               other.name.Contains("Camera") ||
               other.name.Contains("AR") || 
               other.name.Contains("XR") ||
               other.name.Contains("ARCamera") ||
               other.name.Contains("XROrigin");
    }
    
    // Alternative detection method using raycast from camera
    void Update()
    {
        // Only check if we have a camera and the cube is visible
        if (Camera.main != null && gameObject.activeInHierarchy)
        {
            // Cast a ray from camera down to check if player is above this cube
            Ray ray = new Ray(Camera.main.transform.position, Vector3.down);
            RaycastHit hit;
            
            // Use a larger detection range and check all layers
            if (Physics.Raycast(ray, out hit, 5f))
            {
                // Check if the hit object is this cube
                if (hit.collider.gameObject == gameObject)
                {
                    if (!playerInside)
                    {
                        playerInside = true;
                        
                        if (showDebugInfo)
                        {
                            Debug.Log($"🎯 Player detected above cube: {gameObject.name} (Number: {cubeNumber}) via raycast");
                        }
                        
                        // Notify the number line generator
                        if (numberLineGenerator != null)
                        {
                            numberLineGenerator.HandlePlayerEnteredCube(cubeNumber);
                        }
                    }
                }
            }
            else
            {
                // Player is not above this cube
                if (playerInside)
                {
                    playerInside = false;
                    
                    if (showDebugInfo)
                    {
                        Debug.Log($"Player no longer above cube: {gameObject.name} (Number: {cubeNumber})");
                    }
                    
                    // Notify the number line generator
                    if (numberLineGenerator != null)
                    {
                        numberLineGenerator.HandlePlayerExitedCube();
                    }
                }
            }
        }
    }
    
    // Visual debugging
    void OnDrawGizmosSelected()
    {
        if (showDebugInfo)
        {
            Gizmos.color = playerInside ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
} 