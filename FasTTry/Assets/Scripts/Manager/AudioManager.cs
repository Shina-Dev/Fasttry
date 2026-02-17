using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;

    [Header("UI SFX")]
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip spinButtonSFX;
    [SerializeField] private AudioClip gameOverSFX;

    [Header("Player SFX")]
    [SerializeField] private AudioClip playerShootSFX;
    [SerializeField] private AudioClip playerHitSFX;

    [Header("Enemy SFX")]
    [SerializeField] private AudioClip enemyShootSFX;
    [SerializeField] private AudioClip enemyHitSFX;
    [SerializeField] private AudioClip enemyDeathSFX;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;

    private void Awake()
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

        // Configurar fuentes de audio
        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.volume = sfxVolume;
        }
    }

    // ==================== MÚSICA ====================

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return; // Ya está sonando esta música

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    // ==================== SFX UI ====================

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSFX);
    }

    public void PlaySpinButton()
    {
        PlaySFX(spinButtonSFX, 0.8f); // Un poco más bajo para que no sature
    }

    public void PlayGameOverSound()
    {
        PlaySFX(gameOverSFX, 0.9f);
    }

    // ==================== SFX JUGADOR ====================

    public void PlayPlayerShoot()
    {
        PlaySFX(playerShootSFX, 0.5f); // Más bajo porque dispara seguido
    }

    public void PlayPlayerHit()
    {
        PlaySFX(playerHitSFX);
    }

    // ==================== SFX ENEMIGOS ====================

    public void PlayEnemyShoot()
    {
        PlaySFX(enemyShootSFX, 0.4f);
    }

    public void PlayEnemyHit()
    {
        PlaySFX(enemyHitSFX, 0.6f);
    }

    public void PlayEnemyDeath()
    {
        PlaySFX(enemyDeathSFX);
    }

    // ==================== MÉTODOS AUXILIARES ====================

    private void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (sfxSource == null || clip == null) return;

        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    // Método para pausar/reanudar música
    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }
}