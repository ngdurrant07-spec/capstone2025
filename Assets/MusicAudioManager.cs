using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MusicAudioManager : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";

    private static MusicAudioManager instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private Slider musicSlider;

    [Header("Music Clips")]
    [SerializeField] private AudioClip tutorialMusic;
    [SerializeField] private AudioClip Level1Music;
    [SerializeField] private AudioClip LevelClearMusic;

    private float currentVolume = 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            instance.CopyMissingReferencesFrom(this);
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        if (musicSource == null)
            musicSource = GetComponentInChildren<AudioSource>(true);

        currentVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));

        if (musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            musicSource.volume = currentVolume;
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
        BindSliderIfPresent();
        ApplyVolumeToSlider();
        PlayMusicForScene(SceneManager.GetActiveScene());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindSliderIfPresent();
        ApplyVolumeToSlider();
        PlayMusicForScene(scene);
    }

    public void OnMusicSliderChanged()
    {
        if (musicSlider == null)
            return;

        SetMusicVolume(musicSlider.value);
    }

    public static void PlayTutorialMusic()
    {
        if (instance == null)
            return;

        instance.PlayClip(instance.tutorialMusic, true);
    }

    public static void PlayLevel1Music()
    {
        if (instance == null)
            return;

        instance.PlayClip(instance.Level1Music, true);
    }

    public static void PlayLevelClearMusic()
    {
        if (instance == null)
            return;

        instance.PlayClip(instance.LevelClearMusic, false);
    }

    public static void StopMusic()
    {
        if (instance == null || instance.musicSource == null)
            return;

        instance.musicSource.Stop();
    }

    public static void SetVolume(float volume)
    {
        if (instance == null)
            return;

        instance.SetMusicVolume(volume);
    }

    private void SetMusicVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MusicVolumeKey, currentVolume);
        PlayerPrefs.Save();

        if (musicSource != null)
            musicSource.volume = currentVolume;

        ApplyVolumeToSlider();
    }

    private void PlayMusicForScene(Scene scene)
    {
        if (scene.name == "Tutorial")
        {
            PlayClip(tutorialMusic, true);
            return;
        }

        if (scene.name == "Level1")
        {
            PlayClip(Level1Music, true);
        }
    }

    private void PlayClip(AudioClip clip, bool loop)
    {
        if (musicSource == null || clip == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying && musicSource.loop == loop)
            return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = currentVolume;
        musicSource.Play();
    }

    private void BindSliderIfPresent()
    {
        if (musicSlider != null && musicSlider.gameObject.scene.IsValid())
            return;

        musicSlider = null;

        foreach (Slider slider in FindObjectsByType<Slider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (slider != null && slider.name.Contains("MusicVolumeSlider"))
            {
                musicSlider = slider;
                break;
            }
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
            musicSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void ApplyVolumeToSlider()
    {
        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(currentVolume);
    }

    private void OnSliderValueChanged(float value)
    {
        SetMusicVolume(value);
    }

    private void CopyMissingReferencesFrom(MusicAudioManager other)
    {
        if (other == null)
            return;

        if (musicSource == null && other.musicSource != null)
            musicSource = other.musicSource;

        if (tutorialMusic == null && other.tutorialMusic != null)
            tutorialMusic = other.tutorialMusic;

        if (Level1Music == null && other.Level1Music != null)
            Level1Music = other.Level1Music;

        if (LevelClearMusic == null && other.LevelClearMusic != null)
            LevelClearMusic = other.LevelClearMusic;
    }
}
