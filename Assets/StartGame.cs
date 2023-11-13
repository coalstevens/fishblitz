using OysterUtils;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SmoothSceneManager.LoadScene("SampleScene");
        }
    }
}
