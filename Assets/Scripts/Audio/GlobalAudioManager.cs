using UnityEngine;

/// <summary>
/// 全局音频管理器，负责背景音乐与战斗音效
/// </summary>
public class GlobalAudioManager : MonoBehaviour
{
    private const float DefaultMusicVolume = 0.6f;
    private const float DefaultSfxVolume = 1f;

    public static GlobalAudioManager Instance { get; private set; }

    [Header("背景音乐")]
    [SerializeField] private AudioClip backgroundMusicClip;

    [Header("战斗音效")]
    [SerializeField] private AudioClip whipClip;
    [SerializeField] private AudioClip healClip;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeSources();
        PlayDefaultMusic();
    }

    /// <summary>
    /// 播放指定的背景音乐
    /// </summary>
    /// <param name="musicClip">需要播放的音乐片段</param>
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicSource == null || musicClip == null)
        {
            return;
        }

        if (musicSource.clip == musicClip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.volume = DefaultMusicVolume;
        musicSource.Play();
    }

    /// <summary>
    /// 播放鞭打音效
    /// </summary>
    public void PlayWhipSfx()
    {
        PlaySfx(whipClip);
    }

    /// <summary>
    /// 播放治疗音效
    /// </summary>
    public void PlayHealSfx()
    {
        PlaySfx(healClip);
    }

    private void InitializeSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.playOnAwake = false;
        sfxSource.playOnAwake = false;
    }

    private void PlayDefaultMusic()
    {
        if (backgroundMusicClip != null)
        {
            PlayMusic(backgroundMusicClip);
        }
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
        {
            return;
        }

        sfxSource.volume = DefaultSfxVolume;
        sfxSource.PlayOneShot(clip);
    }
}

