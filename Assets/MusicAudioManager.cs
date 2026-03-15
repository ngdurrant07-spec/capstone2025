using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicAudioManager : MonoBehaviour
{
    public static MusicAudioManager Instance;

    [SerializeField] private MusicLibrary musicLibrary;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private string tutorialTrackName = "Tutorial";
    [SerializeField] private string level1TrackName = "Level1";
    [SerializeField] private string level2TrackName = "Level2";
    [SerializeField] private string level3TrackName = "Level3";
    [SerializeField] private string levelClearTrackName = "LevelClear";

    [SerializeField] private string bossTrackName = "Boss";

    [SerializeField] private string WinGameTrackName = "WinGame";



    private Coroutine crossfadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        if (musicSource == null)
        {
            musicSource = GetComponentInChildren<AudioSource>(true);
        }

        if (musicLibrary == null)
        {
            musicLibrary = GetComponent<MusicLibrary>();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    public void PlayMusic(string trackName, float fadeDuration = 0.5f, bool loop = true)
    {
        if (musicLibrary == null || musicSource == null)
        {
            return;
        }

        AudioClip nextTrack = musicLibrary.GetClipFromName(trackName);
        if (nextTrack == null)
        {
            return;
        }

        if (crossfadeRoutine != null)
        {
            StopCoroutine(crossfadeRoutine);
        }

        crossfadeRoutine = StartCoroutine(AnimateMusicCrossfade(nextTrack, fadeDuration, loop));
    }

    public static void PlayTutorialMusic(float fadeDuration = 0.5f)
    {
        if (Instance == null)
        {
            return;
        }

        Instance.PlayMusic(Instance.tutorialTrackName, fadeDuration, true);
    }

    public static void PlayLevel1Music(float fadeDuration = 0.5f)
    {
        if (Instance == null)
        {
            return;
        }

        Instance.PlayMusic(Instance.level1TrackName, fadeDuration, true);
    }

    public static void PlayLevelClearMusic(float fadeDuration = 0.5f)
    {
        if (Instance == null)
        {
            return;
        }

        Instance.PlayMusic(Instance.levelClearTrackName, fadeDuration, false);
    }

    public static void StopMusic()
    {
        if (Instance == null || Instance.musicSource == null)
        {
            return;
        }

        if (Instance.crossfadeRoutine != null)
        {
            Instance.StopCoroutine(Instance.crossfadeRoutine);
            Instance.crossfadeRoutine = null;
        }

        Instance.musicSource.Stop();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Tutorial":
                PlayMusic(tutorialTrackName);
                break;
            case "Level1":
                PlayMusic(level1TrackName);
                break;
            case "Level2":
                PlayMusic(level2TrackName);
                break;
            case "Level3":
            case "Level3_recovered":
                PlayMusic(level3TrackName);
                break;
        }
    }

    private IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.5f, bool loop = true)
    {
        float startVolume = musicSource.volume;

        if (musicSource.isPlaying && fadeDuration > 0f)
        {
            float percent = 0f;
            while (percent < 1f)
            {
                percent += Time.deltaTime / fadeDuration;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, percent);
                yield return null;
            }
        }

        musicSource.clip = nextTrack;
        musicSource.loop = loop;
        musicSource.Play();

        if (fadeDuration <= 0f)
        {
            musicSource.volume = startVolume;
            crossfadeRoutine = null;
            yield break;
        }

        float fadeInPercent = 0f;
        while (fadeInPercent < 1f)
        {
            fadeInPercent += Time.deltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(0f, startVolume, fadeInPercent);
            yield return null;
        }

        musicSource.volume = startVolume;
        crossfadeRoutine = null;
    }
}
