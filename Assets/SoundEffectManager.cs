using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{
    private static SoundEffectManager Instance;
    private static AudioSource audioSource;
    private static SoundEffectLibrary soundEffectLibrary;
    [SerializeField] private Slider sfxSlider;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            soundEffectLibrary = GetComponent<SoundEffectLibrary>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
            audioSource.PlayOneShot(audioClip);
            return true;
        }

        Debug.LogWarning($"[SoundEffectManager] Sound not found or has no clips: {soundName}");
        return false;
    }

    void Start()
    {
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(delegate { OnValueChanged(); });
    }

    public static void SetVolume (float volume)
    {
        if (audioSource == null)
            return;
        audioSource.volume = volume;
    }

    public void OnValueChanged()
    {
        SetVolume(sfxSlider.value);
    }

}
