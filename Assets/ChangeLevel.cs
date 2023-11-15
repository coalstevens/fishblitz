using OysterUtils;
using UnityEngine;

public class ChangeLevel : MonoBehaviour
{
    [SerializeField] private string toScene;

    private void OnTriggerEnter2D(Collider2D other)
    {
        SmoothSceneManager.LoadScene(toScene);
    }
}
