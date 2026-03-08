using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MusicAudioManager : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";
    private static MusicAudioManager instance;
    private static float currentVolume = 1f;

    [Header("------------ AudioSource ----------")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private Slider musicSlider;

    [Header("------------ AudioClip ----------")]

    public AudioClip tutorialMusic;

    public AudioClip LevelClearMusic;

    public AudioClip Level1Music;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            instance.CopyMissingReferencesFrom(this);
            Destroy(gameObject);
            return;
        }

        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        currentVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        SetVolume(currentVolume);
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
        TryBindSliderInScene();
        ApplyVolumeToSlider();
        HandleSceneMusic(SceneManager.GetActiveScene());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBindSliderInScene();
        ApplyVolumeToSlider();
        HandleSceneMusic(scene);
    }

    public static void SetVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);
        if (instance == null || instance.musicSource == null)
            return;

        instance.musicSource.volume = currentVolume;
    }

    public void OnMusicSliderChanged()
    {
        if (musicSlider == null)
            return;

        currentVolume = musicSlider.value;
        PlayerPrefs.SetFloat(MusicVolumeKey, currentVolume);
        PlayerPrefs.Save();
        SetVolume(currentVolume);
    }

    public static void PlayLevelClearMusic()
    {
        if (instance == null || instance.musicSource == null || instance.LevelClearMusic == null)
        {
            Debug.LogWarning("[MusicAudioManager] Missing instance/audio source/level clear clip.");
            return;
        }

        if (instance.musicSource.clip == instance.LevelClearMusic && instance.musicSource.isPlaying)
            return;

        instance.musicSource.Stop();
        instance.musicSource.loop = false;
        instance.musicSource.clip = instance.LevelClearMusic;
        instance.musicSource.Play();
    }

    public static void PlayLevel1Music()
    {
        if (instance == null || instance.musicSource == null || instance.Level1Music == null)
            return;

        if (instance.musicSource.clip == instance.Level1Music && instance.musicSource.isPlaying)
            return;

        instance.musicSource.Stop();
        instance.musicSource.loop = true;
        instance.musicSource.clip = instance.Level1Music;
        instance.musicSource.Play();
    }

    public static void PlayTutorialMusic()
    {
        if (instance == null || instance.musicSource == null || instance.tutorialMusic == null)
            return;

        if (instance.musicSource.clip == instance.tutorialMusic && instance.musicSource.isPlaying)
            return;

        instance.musicSource.Stop();
        instance.musicSource.loop = true;
        instance.musicSource.clip = instance.tutorialMusic;
        instance.musicSource.Play();
    }

    private void TryBindSliderInScene()
    {
        if (musicSlider != null && !musicSlider.gameObject.scene.IsValid())
            musicSlider = null;

        if (musicSlider == null)
        {
            foreach (Slider slider in FindObjectsByType<Slider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (slider != null && slider.name.Contains("MusicVolumeSlider"))
                {
                    musicSlider = slider;
                    break;
                }
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
        SetVolume(currentVolume);
        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(currentVolume);
    }

    private void OnSliderValueChanged(float value)
    {
        currentVolume = value;
        PlayerPrefs.SetFloat(MusicVolumeKey, currentVolume);
        PlayerPrefs.Save();
        SetVolume(currentVolume);
    }

    public static void StopMusic()
    {
        if (instance == null || instance.musicSource == null)
            return;

        instance.musicSource.Stop();
    }

    private static void HandleSceneMusic(Scene scene)
    {
        if (scene.name == "Level1")
            PlayLevel1Music();
        else if (scene.name == "Tutorial")
            PlayTutorialMusic();
    }

    private void CopyMissingReferencesFrom(MusicAudioManager other)
    {
        if (other == null)
            return;

        if (Level1Music == null && other.Level1Music != null)
            Level1Music = other.Level1Music;

        if (LevelClearMusic == null && other.LevelClearMusic != null)
            LevelClearMusic = other.LevelClearMusic;

        if (tutorialMusic == null && other.tutorialMusic != null)
            tutorialMusic = other.tutorialMusic;
    }

}
