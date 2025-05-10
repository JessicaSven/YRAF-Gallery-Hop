using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    private bool isSoundEnabled = true;
    private float previousVolume = 1f;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] ravenSounds;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip scanSuccess;
    [SerializeField] private AudioClip backgroundMusic;

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
        }
    }

    public void ToggleSound()
    {
        isSoundEnabled = !isSoundEnabled;
        if (isSoundEnabled)
        {
            audioSource.volume = previousVolume;
        }
        else
        {
            previousVolume = audioSource.volume;
            audioSource.volume = 0f;
        }
    }

    public bool IsSoundEnabled()
    {
        return isSoundEnabled;
    }

    public void SetVolume(float volume)
    {
        if (isSoundEnabled)
        {
            audioSource.volume = Mathf.Clamp01(volume);
            previousVolume = audioSource.volume;
        }
    }

    public void PlayRandomRavenSound()
    {
        if (!isSoundEnabled) return;
        if (ravenSounds != null && ravenSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, ravenSounds.Length);
            if (ravenSounds[randomIndex] != null)
                audioSource.PlayOneShot(ravenSounds[randomIndex]);
        }
    }

    public void PlaySuccess()
    {
        if (!isSoundEnabled) return;
        if (successSound != null)
            audioSource.PlayOneShot(successSound);
    }

    public void PlayButtonClick()
    {
        if (!isSoundEnabled) return;
        if (buttonClick != null)
            audioSource.PlayOneShot(buttonClick);
    }

    public void PlayScanSuccess()
    {
        if (!isSoundEnabled) return;
        if (scanSuccess != null)
            audioSource.PlayOneShot(scanSuccess);
    }

    public void PlayBackgroundMusic()
    {
        if (!isSoundEnabled) return;
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        audioSource.Stop();
    }
} 