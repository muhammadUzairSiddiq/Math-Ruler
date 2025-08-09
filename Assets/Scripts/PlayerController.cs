using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int minNumber = -20;
    public int maxNumber = 20;
    public int currentNumber = 0; // Start at 0 or wherever you want
    public float moveDuration = 1.0f; // Increased for more animated movement
    public float animationStartDelay = 0.1f; // Delay before movement starts
    public string numberSpriteParentName = "RulerParent";
    public Vector3 characterOffset = new Vector3(0, 0.5f, 0); // Adjust Y as needed

    private Animator animator;
    private bool isMoving = false;
    private Transform numberSpritesParent;
    private SpriteRenderer spriteRenderer;
    private AnswerVerifier answerVerifier;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        numberSpritesParent = GameObject.Find(numberSpriteParentName).transform;
        answerVerifier = FindObjectOfType<AnswerVerifier>();
        MoveToNumber(currentNumber, true);
    }

    public void MoveLeft()
    {
        if (isMoving || currentNumber <= minNumber) 
        {
            Debug.Log($"MoveLeft blocked - isMoving: {isMoving}, currentNumber: {currentNumber}, minNumber: {minNumber}");
            return;
        }
        
        // Skip current reward if any button is active
        if (answerVerifier != null && IsAnyRewardButtonActive())
        {
            answerVerifier.SkipCurrentReward();
        }
        
        int targetNumber = currentNumber - 1;
        StartCoroutine(MoveToNumberCoroutine(targetNumber, -1));
    }

    public void MoveRight()
    {
        if (isMoving || currentNumber >= maxNumber) 
        {
            Debug.Log($"MoveRight blocked - isMoving: {isMoving}, currentNumber: {currentNumber}, maxNumber: {maxNumber}");
            return;
        }
        
        // Skip current reward if any button is active
        if (answerVerifier != null && IsAnyRewardButtonActive())
        {
            answerVerifier.SkipCurrentReward();
        }
        
        int targetNumber = currentNumber + 1;
        StartCoroutine(MoveToNumberCoroutine(targetNumber, 1));
    }
    
    public void MoveToNumber(int targetNumber)
    {
        if (isMoving) return;
        
        if (targetNumber < minNumber || targetNumber > maxNumber)
        {
            Debug.LogWarning($"Target number {targetNumber} is out of range [{minNumber}, {maxNumber}]");
            return;
        }
        
        int direction = targetNumber > currentNumber ? 1 : -1;
        StartCoroutine(MoveToNumberCoroutine(targetNumber, direction));
    }
    
    bool IsAnyRewardButtonActive()
    {
        if (answerVerifier == null) return false;
        
        // Check if any reward button is active
        return (answerVerifier.houseRewardButton != null && answerVerifier.houseRewardButton.gameObject.activeInHierarchy) ||
               (answerVerifier.petRewardButton != null && answerVerifier.petRewardButton.gameObject.activeInHierarchy) ||
               (answerVerifier.carRewardButton != null && answerVerifier.carRewardButton.gameObject.activeInHierarchy) ||
               (answerVerifier.treeRewardButton != null && answerVerifier.treeRewardButton.gameObject.activeInHierarchy);
    }

    System.Collections.IEnumerator MoveToNumberCoroutine(int targetNumber, int direction)
    {
        isMoving = true;
        
        // Start animation BEFORE movement
        animator.SetBool("isMoving", true);
        
        // Small delay to let animation start
        yield return new WaitForSeconds(animationStartDelay);

        // Flip character
        spriteRenderer.flipX = direction < 0;

        GameObject target = GameObject.Find(targetNumber.ToString());
        if (target == null)
        {
            isMoving = false;
            animator.SetBool("isMoving", false);
            yield break;
        }

        Vector3 startPos = transform.position;
        Vector3 endPos = target.transform.position + characterOffset;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
        currentNumber = targetNumber; // Update current number to new position

        // Stop animation AFTER movement completes
        isMoving = false;
        animator.SetBool("isMoving", false);
    }

    // For instant placement (e.g., at start)
    void MoveToNumber(int targetNumber, bool instant)
    {
        GameObject target = GameObject.Find(targetNumber.ToString());
        if (target == null) return;
        currentNumber = targetNumber;
        if (instant)
        {
            transform.position = target.transform.position + characterOffset;
        }
        else
        {
            StartCoroutine(MoveToNumberCoroutine(targetNumber, 1));
        }
    }
}