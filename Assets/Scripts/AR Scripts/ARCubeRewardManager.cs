using UnityEngine;

/// <summary>
/// Simple script to manage AR rewards on each cube prefab
/// Manually assign AR reward references by dragging and dropping in the Inspector
/// </summary>
public class ARCubeRewardManager : MonoBehaviour
{
    [Header("AR Reward References - Drag & Drop Here")]
    [Tooltip("Drag your AR House object here")]
    public GameObject arHouse;
    
    [Tooltip("Drag your AR Pet object here")]
    public GameObject arPet;
    
    [Tooltip("Drag your AR Car object here")]
    public GameObject arCar;
    
    [Tooltip("Drag your AR Tree object here")]
    public GameObject arTree;
    
    [Header("Debug")]
    [Tooltip("Enable to see debug logs in console")]
    public bool showDebugLogs = false;
    
    private void Start()
    {
        // Deactivate all AR rewards at startup
        DeactivateAllRewards();
        
        if (showDebugLogs)
            Debug.Log($"ARCubeRewardManager initialized on {gameObject.name}");
    }
    
    /// <summary>
    /// Activates the appropriate AR reward based on correct answers in a row
    /// </summary>
    /// <param name="correctAnswersInRow">Number of correct answers in a row (1-4)</param>
    public void ActivateReward(int correctAnswersInRow)
    {
        // Activate the appropriate reward based on counter (don't deactivate previous rewards)
        switch (correctAnswersInRow)
        {
            case 1:
                if (arHouse != null)
                {
                    arHouse.SetActive(true);
                    if (showDebugLogs)
                        Debug.Log($"Activated AR House on {gameObject.name}");
                }
                else if (showDebugLogs)
                {
                    Debug.LogWarning($"AR House reference is null on {gameObject.name}");
                }
                break;
                
            case 2:
                if (arPet != null)
                {
                    arPet.SetActive(true);
                    if (showDebugLogs)
                        Debug.Log($"Activated AR Pet on {gameObject.name}");
                }
                else if (showDebugLogs)
                {
                    Debug.LogWarning($"AR Pet reference is null on {gameObject.name}");
                }
                break;
                
            case 3:
                if (arCar != null)
                {
                    arCar.SetActive(true);
                    if (showDebugLogs)
                        Debug.Log($"Activated AR Car on {gameObject.name}");
                }
                else if (showDebugLogs)
                {
                    Debug.LogWarning($"AR Car reference is null on {gameObject.name}");
                }
                break;
                
            case 4:
                if (arTree != null)
                {
                    arTree.SetActive(true);
                    if (showDebugLogs)
                        Debug.Log($"Activated AR Tree on {gameObject.name}");
                }
                else if (showDebugLogs)
                {
                    Debug.LogWarning($"AR Tree reference is null on {gameObject.name}");
                }
                break;
                
            default:
                if (showDebugLogs)
                    Debug.Log($"No reward for {correctAnswersInRow} correct answers on {gameObject.name}");
                break;
        }
    }
    
    /// <summary>
    /// Deactivates all AR rewards on this cube
    /// </summary>
    public void DeactivateAllRewards()
    {
        if (arHouse != null) arHouse.SetActive(false);
        if (arPet != null) arPet.SetActive(false);
        if (arCar != null) arCar.SetActive(false);
        if (arTree != null) arTree.SetActive(false);
        
        if (showDebugLogs)
            Debug.Log($"Deactivated all AR rewards on {gameObject.name}");
    }
    
    /// <summary>
    /// Activates a specific reward by name
    /// </summary>
    /// <param name="rewardName">Name of the reward to activate ("House", "Pet", "Car", "Tree")</param>
    public void ActivateSpecificReward(string rewardName)
    {
        DeactivateAllRewards(); // Clear existing rewards first
        
        switch (rewardName.ToLower())
        {
            case "house":
                if (arHouse != null) arHouse.SetActive(true);
                break;
            case "pet":
                if (arPet != null) arPet.SetActive(true);
                break;
            case "car":
                if (arCar != null) arCar.SetActive(true);
                break;
            case "tree":
                if (arTree != null) arTree.SetActive(true);
                break;
            default:
                if (showDebugLogs)
                    Debug.LogWarning($"Unknown reward name: {rewardName}");
                break;
        }
        
        if (showDebugLogs)
            Debug.Log($"Activated {rewardName} on {gameObject.name}");
    }
    
    /// <summary>
    /// Checks if any reward is currently active
    /// </summary>
    /// <returns>True if any reward is active, false otherwise</returns>
    public bool HasActiveReward()
    {
        return (arHouse != null && arHouse.activeInHierarchy) ||
               (arPet != null && arPet.activeInHierarchy) ||
               (arCar != null && arCar.activeInHierarchy) ||
               (arTree != null && arTree.activeInHierarchy);
    }
    
    /// <summary>
    /// Gets the name of the currently active reward
    /// </summary>
    /// <returns>Name of active reward or "None" if no reward is active</returns>
    public string GetActiveRewardName()
    {
        if (arHouse != null && arHouse.activeInHierarchy) return "House";
        if (arPet != null && arPet.activeInHierarchy) return "Pet";
        if (arCar != null && arCar.activeInHierarchy) return "Car";
        if (arTree != null && arTree.activeInHierarchy) return "Tree";
        return "None";
    }
    
    // Simple context menu tests for debugging (Editor Only)
    #if UNITY_EDITOR
    
    [ContextMenu("Test Activate House")]
    void TestActivateHouse()
    {
        ActivateSpecificReward("House");
    }
    
    [ContextMenu("Test Activate Pet")]
    void TestActivatePet()
    {
        ActivateSpecificReward("Pet");
    }
    
    [ContextMenu("Test Activate Car")]
    void TestActivateCar()
    {
        ActivateSpecificReward("Car");
    }
    
    [ContextMenu("Test Activate Tree")]
    void TestActivateTree()
    {
        ActivateSpecificReward("Tree");
    }
    
    [ContextMenu("Test Deactivate All")]
    void TestDeactivateAll()
    {
        DeactivateAllRewards();
    }
    
    [ContextMenu("Test Counter-Based Activation")]
    void TestCounterBasedActivation()
    {
        Debug.Log("=== TESTING COUNTER-BASED ACTIVATION ===");
        
        // First deactivate all rewards to start clean
        DeactivateAllRewards();
        
        // Test progressive activation (rewards should accumulate)
        for (int i = 1; i <= 4; i++)
        {
            Debug.Log($"Testing {i} correct answers in a row...");
            ActivateReward(i);
            
            // Check what's actually active
            string activeReward = GetActiveRewardName();
            Debug.Log($"Active reward: {activeReward}");
            
            // Wait a moment to see the visual change
            System.Threading.Thread.Sleep(500);
        }
        
        Debug.Log("=== TEST COMPLETE ===");
        Debug.Log("Check if you can see House, Pet, Car, and Tree all visible on the cube!");
    }
    
    [ContextMenu("Test Progressive Reward Accumulation")]
    void TestProgressiveRewardAccumulation()
    {
        Debug.Log("=== TESTING PROGRESSIVE REWARD ACCUMULATION ===");
        
        // Start fresh
        DeactivateAllRewards();
        
        // Simulate player getting correct answers one by one
        Debug.Log("Player gets 1st correct answer → House should appear");
        ActivateReward(1);
        System.Threading.Thread.Sleep(1000);
        
        Debug.Log("Player gets 2nd correct answer → Pet should join House");
        ActivateReward(2);
        System.Threading.Thread.Sleep(1000);
        
        Debug.Log("Player gets 3rd correct answer → Car should join House + Pet");
        ActivateReward(3);
        System.Threading.Thread.Sleep(1000);
        
        Debug.Log("Player gets 4th correct answer → Tree should join House + Pet + Car");
        ActivateReward(4);
        System.Threading.Thread.Sleep(1000);
        
        Debug.Log("=== PROGRESSIVE TEST COMPLETE ===");
        Debug.Log("You should now see ALL 4 rewards visible on the cube!");
    }
    
    [ContextMenu("Test Multi-Cube Reward Distribution")]
    void TestMultiCubeRewardDistribution()
    {
        Debug.Log("=== TESTING MULTI-CUBE REWARD DISTRIBUTION ===");
        Debug.Log("This simulates real gameplay with different correct answer cubes");
        
        // Start fresh
        DeactivateAllRewards();
        
        // Simulate real gameplay scenario
        Debug.Log("Player gets 1st correct answer = 3 → House should appear on cube 3");
        ActivateReward(1);
        System.Threading.Thread.Sleep(1000);
        
        Debug.Log("Player gets 2nd correct answer = 7 → Pet should appear on cube 7");
        ActivateReward(2);
        System.Threading.Thread.Sleep(1000);
        
        Debug.Log("Player gets 3rd correct answer = 9 → Car should appear on cube 9");
        ActivateReward(3);
        System.Threading.Thread.Sleep(1000);
        
        Debug.Log("Player gets 4th correct answer = 2 → Tree should appear on cube 2");
        ActivateReward(4);
        System.Threading.Thread.Sleep(1000);
        
        Debug.Log("=== MULTI-CUBE TEST COMPLETE ===");
        Debug.Log("Now test wrong answer scenario...");
        
        // Test wrong answer scenario
        Debug.Log("Player gives WRONG answer → All rewards should deactivate, counter resets");
        DeactivateAllRewards();
        System.Threading.Thread.Sleep(1000);
        
        Debug.Log("=== WRONG ANSWER TEST COMPLETE ===");
        Debug.Log("All rewards should be gone, ready for fresh start!");
    }
    
    [ContextMenu("Test Real Game Flow")]
    void TestRealGameFlow()
    {
        Debug.Log("=== TESTING REAL GAME FLOW ===");
        Debug.Log("Simulating actual gameplay progression...");
        
        // Start fresh
        DeactivateAllRewards();
        
        // Simulate player progression
        Debug.Log("🎯 GAME START: Player begins with no rewards");
        System.Threading.Thread.Sleep(500);
        
        Debug.Log("✅ CORRECT ANSWER #1: Player answers '2+1=3' correctly");
        Debug.Log("   → House should appear on cube 3");
        ActivateReward(1);
        System.Threading.Thread.Sleep(1000);
        
        Debug.Log("✅ CORRECT ANSWER #2: Player answers '3+4=7' correctly");
        Debug.Log("   → Pet should appear on cube 7");
        ActivateReward(2);
        System.Threading.Thread.Sleep(1000);
        
        Debug.Log("✅ CORRECT ANSWER #3: Player answers '5+4=9' correctly");
        Debug.Log("   → Car should appear on cube 9");
        ActivateReward(3);
        System.Threading.Thread.Sleep(1000);
        
        Debug.Log("✅ CORRECT ANSWER #4: Player answers '1+1=2' correctly");
        Debug.Log("   → Tree should appear on cube 2");
        ActivateReward(4);
        System.Threading.Thread.Sleep(1000);
        
        Debug.Log("🎉 PLAYER WINS: All 4 rewards collected across different cubes!");
        System.Threading.Thread.Sleep(1000);
        
        Debug.Log("❌ WRONG ANSWER: Player gives wrong answer");
        Debug.Log("   → All rewards deactivate, progress resets to 0");
        DeactivateAllRewards();
        System.Threading.Thread.Sleep(1000);
        
        Debug.Log("🔄 GAME RESET: Ready for new game, counter = 0");
        Debug.Log("=== REAL GAME FLOW TEST COMPLETE ===");
    }
    
    #endif
}
