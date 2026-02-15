using UnityEngine;

public class MusicAudioManager : MonoBehaviour
{
    private static MusicAudioManager instance;

    [Header("------------ AudioSource ----------")]
    [SerializeField] private AudioSource musicSource;

    [Header("------------ AudioClip ----------")]

    public AudioClip tutorialMusic;

    public AudioClip LevelClearMusic;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();
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
}
