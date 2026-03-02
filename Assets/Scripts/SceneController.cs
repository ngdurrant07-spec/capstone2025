using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [SerializeField] private float _sceneFadeDuration = 0.5f;

    private SceneFade _sceneFade;
    private bool _isTransitioning;

    private void Awake()
    {
        Instance = this;
        _sceneFade = GetComponentInChildren<SceneFade>(true);
    }

    private IEnumerator Start()
    {
        if (_sceneFade == null)
            yield break;

        yield return _sceneFade.FadeInCoroutine(_sceneFadeDuration);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void LoadScene(string sceneName)
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.LoadSceneCoroutine(sceneName));
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        if (_isTransitioning)
            yield break;

        _isTransitioning = true;

        if (_sceneFade != null)
            yield return _sceneFade.FadeOutCoroutine(_sceneFadeDuration);

        SceneManager.LoadScene(sceneName);
    }
}
