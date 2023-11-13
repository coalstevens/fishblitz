using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

// The scene loading coroutine must be done on this monobehaviour
// because it will persist across scenes.  If we were to start the scene load
// coroutine from the trigger object, the coroutine would die right when the new scene loads
// and the old scene unloads.
public class SceneTransitionOverlay : MonoBehaviour
{
  [SerializeField] private float transitionDuration = 0.5f;
  private Animator animator;

  private void Awake()
  {
    animator = GetComponent<Animator>();
    DontDestroyOnLoad(gameObject);
  }

  private IEnumerator LoadSceneAsync(string name)
  {
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name);
    while (!asyncLoad.isDone)
    {
      yield return null;
    }
  }

  public void LoadScene(string toSceneName)
  {
    StartCoroutine(SceneRoutine(toSceneName));
  }

  private IEnumerator SceneRoutine(string toSceneName)
  {
    // Let the transition in animation play
    animator.SetBool("SceneVisible", false);
    yield return new WaitForSeconds(transitionDuration);

    // Load the new scene first, so the setup routine can operate on it properly
    yield return LoadSceneAsync(toSceneName);

    // Let the transition out animation play
    animator.SetBool("SceneVisible", true);
    yield return new WaitForSeconds(transitionDuration);
    Destroy(gameObject);
  }
}


