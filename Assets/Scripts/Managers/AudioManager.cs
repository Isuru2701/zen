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

public enum PlayerSFX
{
    None,
    FootstepsGround,
    FootstepsWood,
    FootstepsStone,
    ClarityEnter,
    ClarityExit,
    Jump,
    Land,
    Dodge,
    Attack,
    AttackCombo,
    DamageTaken,
    Death
}

public enum BaseEnemySFX
{
    None,
    Footsteps,

    Attack,
    Damaged,
    Death
}

public enum BossSFX
{
    None,
    Attack,
    Charge,
    Damaged,
    Death,
    Roar
}

public enum EnvironmentSFX
{
    None,
    DoorOpen,
    DoorClose,
    ItemPickup,
    Checkpoint,
    Water,
    Ambient
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class MusicSound
    {
        public MusicTrack track;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop = true;
    }

    [System.Serializable]
    public class PlayerSFXSound
    {
        public PlayerSFX sfx;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [System.Serializable]
    public class BaseEnemySFXSound
    {
        public BaseEnemySFX sfx;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [System.Serializable]
    public class BossSFXSound
    {
        public BossSFX sfx;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [System.Serializable]
    public class EnvironmentSFXSound
    {
        public EnvironmentSFX sfx;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("Music Library")]
    public List<MusicSound> music = new List<MusicSound>();

    [Header("SFX Library")]
    public List<PlayerSFXSound> playerSFX = new List<PlayerSFXSound>();
    public List<BaseEnemySFXSound> baseEnemySFX = new List<BaseEnemySFXSound>();
    public List<BossSFXSound> bossSFX = new List<BossSFXSound>();
    public List<EnvironmentSFXSound> environmentSFX = new List<EnvironmentSFXSound>();

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

    // Music controls
    public void PlayMusic(MusicTrack track, bool restart = false)
    {
        var s = music.Find(m => m != null && m.track == track);
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
    public void PlaySFX(PlayerSFX sfx)
    {
        var s = playerSFX.Find(x => x != null && x.sfx == sfx);
        if (s == null || s.clip == null) return;
        sfxSource.PlayOneShot(s.clip, mute ? 0f : s.volume * sfxVolume);
    }

    public void PlaySFX(BaseEnemySFX sfx)
    {
        var s = baseEnemySFX.Find(x => x != null && x.sfx == sfx);
        if (s == null || s.clip == null) return;
        sfxSource.PlayOneShot(s.clip, mute ? 0f : s.volume * sfxVolume);
    }

    public void PlaySFX(BossSFX sfx)
    {
        var s = bossSFX.Find(x => x != null && x.sfx == sfx);
        if (s == null || s.clip == null) return;
        sfxSource.PlayOneShot(s.clip, mute ? 0f : s.volume * sfxVolume);
    }

    public void PlaySFX(EnvironmentSFX sfx)
    {
        var s = environmentSFX.Find(x => x != null && x.sfx == sfx);
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