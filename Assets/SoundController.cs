using UnityEngine;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
    [Header("Sound Icons")]
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;
    
    [Header("UI References")]
    [SerializeField] private Image soundIconImage;
    [SerializeField] private Button soundToggleButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (soundToggleButton != null)
        {
            soundToggleButton.onClick.AddListener(ToggleSound);
        }
        UpdateSoundIcon();
    }

    private void ToggleSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ToggleSound();
            UpdateSoundIcon();
        }
    }

    private void UpdateSoundIcon()
    {
        if (soundIconImage != null && soundOnSprite != null && soundOffSprite != null)
        {
            soundIconImage.sprite = SoundManager.Instance.IsSoundEnabled() ? soundOnSprite : soundOffSprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
