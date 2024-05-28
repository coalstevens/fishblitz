using UnityEngine;

namespace OysterUtils
{
    public static class SmoothSceneManager
    {
        private static GameObject SceneTransitionOverlayPrefab;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            SceneTransitionOverlayPrefab = Resources.Load<GameObject>("Scene/scene-transition-overlay");
        }

        public static void LoadScene(string toSceneName)
        {
            SceneSaveLoadManager _saveLoadManager = GameObject.FindObjectOfType<SceneSaveLoadManager>();
            if (_saveLoadManager != null) {
                _saveLoadManager.SaveScene();
            }
            GameObject transitionObject = GameObject.Instantiate(SceneTransitionOverlayPrefab, Vector3.zero, Quaternion.identity);
            transitionObject.GetComponent<SceneTransitionOverlay>().LoadScene(toSceneName);
        }
    }
}