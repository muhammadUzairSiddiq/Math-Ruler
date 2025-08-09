using UnityEngine;
using System.Collections;

public class FeedbackDisplay : MonoBehaviour
{
    public GameObject cross;
    public GameObject tick;
    public GameObject gameWon;
    public float feedbackDuration = 2f;

    void Awake()
    {
        HideAll();
    }

    public void ShowTick()
    {
        Debug.Log("[FeedbackDisplay] ShowTick called");
        HideAll();
        if (tick != null)
        {
            Debug.Log("[FeedbackDisplay] Activating tick");
            tick.SetActive(true);
            StartCoroutine(HideAfterDelay(tick));
        }
        else
        {
            Debug.LogWarning("[FeedbackDisplay] Tick reference is null!");
        }
    }

    public void ShowCross()
    {
        Debug.Log("[FeedbackDisplay] ShowCross called");
        HideAll();
        if (cross != null)
        {
            Debug.Log("[FeedbackDisplay] Activating cross");
            cross.SetActive(true);
            StartCoroutine(HideAfterDelay(cross));
        }
        else
        {
            Debug.LogWarning("[FeedbackDisplay] Cross reference is null!");
        }
    }

    public void ShowGameWon()
    {
        Debug.Log("[FeedbackDisplay] ShowGameWon called");
        HideAll();
        if (gameWon != null)
        {
            Debug.Log("[FeedbackDisplay] Activating gameWon");
            gameWon.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[FeedbackDisplay] GameWon reference is null!");
        }
    }

    public void HideAll()
    {
        Debug.Log("[FeedbackDisplay] HideAll called");
        if (cross != null) cross.SetActive(false);
        if (tick != null) tick.SetActive(false);
        if (gameWon != null) gameWon.SetActive(false);
    }

    IEnumerator HideAfterDelay(GameObject go)
    {
        yield return new WaitForSeconds(feedbackDuration);
        if (go != null) go.SetActive(false);
    }
} 