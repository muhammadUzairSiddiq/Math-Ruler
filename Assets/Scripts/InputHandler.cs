using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public AnswerVerifier answerVerifier;
    
    [Header("Input Settings")]
    public KeyCode leftKey = KeyCode.LeftArrow;
    public KeyCode rightKey = KeyCode.RightArrow;
    public KeyCode submitKey = KeyCode.Space;
    public KeyCode submitKeyAlt = KeyCode.Return;
    
    void Update()
    {
        if (answerVerifier != null && !answerVerifier.IsGameActive())
            return;
            
        // Movement input
        if (playerController != null)
        {
            if (Input.GetKeyDown(leftKey))
            {
                playerController.MoveLeft();
            }
            
            if (Input.GetKeyDown(rightKey))
            {
                playerController.MoveRight();
            }
        }
        
        // Submit input
        if (answerVerifier != null && (Input.GetKeyDown(submitKey) || Input.GetKeyDown(submitKeyAlt)))
        {
            answerVerifier.SubmitAnswer();
        }
    }
} 