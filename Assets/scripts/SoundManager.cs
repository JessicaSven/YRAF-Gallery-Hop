using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip ravenTalk;
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

    public void PlayRavenTalk()
    {
        if (ravenTalk != null)
            audioSource.PlayOneShot(ravenTalk);
    }

    public void PlaySuccess()
    {
        if (successSound != null)
            audioSource.PlayOneShot(successSound);
    }

    public void PlayButtonClick()
    {
        if (buttonClick != null)
            audioSource.PlayOneShot(buttonClick);
    }

    public void PlayScanSuccess()
    {
        if (scanSuccess != null)
            audioSource.PlayOneShot(scanSuccess);
    }

    public void PlayBackgroundMusic()
    {
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

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }
} 