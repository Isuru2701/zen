using System.Collections.Generic;
using UnityEngine;

public enum MusicTrack
{
    FloatingLeaf,
    Forest,
    LiveByTheSword,
    Rooftops,
    Wounded
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public string name;
        public MusicTrack musicTrack;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop = false;
    }

    [Header("Library")]
    public List<Sound> music = new List<Sound>();
    public List<Sound> sfx = new List<Sound>();

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    public bool mute = false;

    AudioSource musicSource;
    AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        ApplySettings();
    }

    void ApplySettings()
    {
        musicSource.volume = mute ? 0f : musicVolume;
        sfxSource.volume = mute ? 0f : sfxVolume;
    }

    Sound Find(List<Sound> list, string name)
    {
        return list.Find(s => s != null && s.name == name);
    }

    // Music controls
    public void PlayMusic(MusicTrack track, bool restart = false)
    {
        var s = music.Find(m => m != null && m.musicTrack == track);
        if (s == null || s.clip == null) return;
        if (musicSource.clip == s.clip && musicSource.isPlaying && !restart) return;

        musicSource.clip = s.clip;
        musicSource.loop = s.loop;
        musicSource.volume = mute ? 0f : s.volume * musicVolume;
        musicSource.Play();
    }

    public void PlayMusic(string name, bool restart = false)
    {
        var s = Find(music, name);
        if (s == null || s.clip == null) return;
        if (musicSource.clip == s.clip && musicSource.isPlaying && !restart) return;

        musicSource.clip = s.clip;
        musicSource.loop = s.loop;
        musicSource.volume = mute ? 0f : s.volume * musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }

    // SFX controls
    public void PlaySFX(string name)
    {
        var s = Find(sfx, name);
        if (s == null || s.clip == null) return;
        sfxSource.PlayOneShot(s.clip, mute ? 0f : s.volume * sfxVolume);
    }

    // Volume & mute
    public void SetMusicVolume(float vol)
    {
        musicVolume = Mathf.Clamp01(vol);
        musicSource.volume = mute ? 0f : musicVolume;
    }

    public void SetSFXVolume(float vol)
    {
        sfxVolume = Mathf.Clamp01(vol);
        sfxSource.volume = mute ? 0f : sfxVolume;
    }

    public void SetMute(bool shouldMute)
    {
        mute = shouldMute;
        ApplySettings();
    }
}