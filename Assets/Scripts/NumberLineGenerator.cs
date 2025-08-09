using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using TMPro;

public class NumberLineGenerator : MonoBehaviour
{
    public GameObject numberPrefab; // Assign your prefab here
    public int minNumber = -20;
    public int maxNumber = 20;
    public float spacing = 1.5f;

    [ContextMenu("Generate Number Line")]
    public void GenerateNumberLine()
    {
        // Clean up old children
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        for (int i = minNumber; i <= maxNumber; i++)
        {
            GameObject go = Instantiate(numberPrefab, transform);
            go.name = i.ToString();
            go.transform.localPosition = new Vector3((i - minNumber) * spacing, 0, 0);

            // Find TextMeshPro in children and set the number
            var text = go.GetComponentInChildren<TextMeshPro>();
            if (text != null)
                text.text = i.ToString();
        }
    }
}