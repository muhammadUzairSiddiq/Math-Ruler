using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GotoAR_Scene()
    {

        // Load scene
        SceneManager.LoadScene("ARGameScene");
    }
    public void GotoAR_MobileScene()
    {

        // Load scene
        SceneManager.LoadScene("MobileGame");
    }
}
