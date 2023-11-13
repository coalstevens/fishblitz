using UnityEngine;

namespace OysterUtils
{
  public static class SmoothSceneManager
  {
    private static GameObject SceneTransitionOverlayPrefab;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnAfterSceneLoad()
    {
      SceneTransitionOverlayPrefab = Resources.Load<GameObject>("Prefabs/scene-transition-overlay");
    }

    public static void LoadScene(string toSceneName)
    {
      GameObject transitionObject = GameObject.Instantiate(SceneTransitionOverlayPrefab, Vector3.zero, Quaternion.identity);
      transitionObject.GetComponent<SceneTransitionOverlay>().LoadScene(toSceneName);
    }
  }
}