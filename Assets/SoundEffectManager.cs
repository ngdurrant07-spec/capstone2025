using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundEffectManager : MonoBehaviour
{
    private const string SfxVolumeKey = "SFXVolume";
    private static SoundEffectManager Instance;
    private static AudioSource audioSource;
    private static SoundEffectLibrary soundEffectLibrary;
    private static float currentVolume = 1f;
    [SerializeField] private Slider sfxSlider;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            soundEffectLibrary = GetComponent<SoundEffectLibrary>();
            currentVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
            SetVolume(currentVolume);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBindSliderInScene();
        ApplyVolumeToSlider();
    }

    public static void Play(string soundName)
    {
        TryPlay(soundName);
    }

    public static bool TryPlay(string soundName)
    {
        if (Instance == null || audioSource == null || soundEffectLibrary == null)
        {
            Debug.LogWarning("[SoundEffectManager] Missing instance/audioSource/soundEffectLibrary.");
            return false;
        }

        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);
        if (audioClip != null)
        {
            float clipVolumeScale = soundEffectLibrary.GetVolumeScale(soundName);
            audioSource.PlayOneShot(audioClip, currentVolume * clipVolumeScale);
            return true;
        }

        Debug.LogWarning($"[SoundEffectManager] Sound not found or has no clips: {soundName}");
        return false;
    }

    void Start()
    {
        TryBindSliderInScene();
        ApplyVolumeToSlider();
    }

    public static void SetVolume (float volume)
    {
        currentVolume = Mathf.Clamp01(volume);
        if (audioSource == null)
            return;
        audioSource.volume = currentVolume;
    }

    public void OnValueChanged()
    {
        if (sfxSlider == null)
            return;

        currentVolume = sfxSlider.value;
        PlayerPrefs.SetFloat(SfxVolumeKey, currentVolume);
        PlayerPrefs.Save();
        SetVolume(sfxSlider.value);
    }

    private void TryBindSliderInScene()
    {
        // If the serialized slider was from an old scene, clear it.
        if (sfxSlider != null && !sfxSlider.gameObject.scene.IsValid())
            sfxSlider = null;

        if (sfxSlider == null)
        {
            foreach (Slider slider in FindObjectsByType<Slider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (slider != null && slider.name.Contains("SFXVolumeSlider"))
                {
                    sfxSlider = slider;
                    break;
                }
            }
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
            sfxSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void ApplyVolumeToSlider()
    {
        SetVolume(currentVolume);
        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(currentVolume);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        currentVolume = value;
        PlayerPrefs.SetFloat(SfxVolumeKey, currentVolume);
        PlayerPrefs.Save();
        SetVolume(currentVolume);
    }
}
