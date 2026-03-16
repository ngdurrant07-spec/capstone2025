using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectLibrary : MonoBehaviour
{
    [SerializeField] private SoundEffectGroup[] soundEffectGroups;
    [SerializeField] private SoundEffectGroup[] fallbackSoundEffectGroups;
    private Dictionary<string, List<AudioClip>> soundDictionary;
    private Dictionary<string, float> volumeScaleDictionary;

    private void Awake()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        soundDictionary = new Dictionary<string, List<AudioClip>>();
        volumeScaleDictionary = new Dictionary<string, float>();
        foreach (SoundEffectGroup soundEffectGroup in soundEffectGroups)
        {
            soundDictionary[soundEffectGroup.name] = soundEffectGroup.audioclips;
            volumeScaleDictionary[soundEffectGroup.name] = NormalizeVolumeScale(soundEffectGroup.volumeScale);
        }

        foreach (SoundEffectGroup fallbackGroup in fallbackSoundEffectGroups)
        {
            if (soundDictionary.ContainsKey(fallbackGroup.name) && soundDictionary[fallbackGroup.name] != null && soundDictionary[fallbackGroup.name].Count > 0)
                continue;

            soundDictionary[fallbackGroup.name] = fallbackGroup.audioclips;
            volumeScaleDictionary[fallbackGroup.name] = NormalizeVolumeScale(fallbackGroup.volumeScale);
        }
    }

    public AudioClip GetRandomClip(string name)
    {
        if (soundDictionary.ContainsKey(name))
        {
            List<AudioClip> audioClips = soundDictionary[name];
            if (audioClips.Count > 0)
            {
                return audioClips[UnityEngine.Random.Range(0, audioClips.Count)];
            }
        }
        return null;
    }

    public float GetVolumeScale(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return 1f;

        if (volumeScaleDictionary != null && volumeScaleDictionary.TryGetValue(name, out float volumeScale))
            return volumeScale;

        return 1f;
    }

    private static float NormalizeVolumeScale(float volumeScale)
    {
        return volumeScale > 0f ? volumeScale : 1f;
    }
}

[System.Serializable]
public struct SoundEffectGroup
{
    public string name;
    public List<AudioClip> audioclips;
    public float volumeScale;
}
