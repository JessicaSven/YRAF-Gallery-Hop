using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class TalkingRaven : MonoBehaviour
{
    // Singleton instance
    public static TalkingRaven _instance;

    [SerializeField]
    private GameObject speechBubble;  // Reference to the speech bubble GameObject
    
    [SerializeField]
    private TextMeshProUGUI dialogueText; // Reference to the TextMeshProUGUI component
    
    private List<string> dialogueLines = new List<string>();
    private bool isTalking = false;
    private int currentLineIndex = 0;
    private Coroutine autoAdvanceCoroutine;
    private const float INACTIVITY_TIMEOUT = 10f; // 10 seconds timeout

    private void Awake()
    {
        // Ensure singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Start the crow talking with the provided dialogue lines
    /// </summary>
    /// <param name="lines">List of strings for the crow to say</param>
    public void StartTalking(List<string> lines)
    {
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning("No dialogue lines provided to TalkingCrow");
            return;
        }

        dialogueLines = new List<string>(lines);
        currentLineIndex = 0;
        isTalking = true;
        SoundManager.Instance.PlayRandomRavenSound();
        
        if (speechBubble != null)
        {
            speechBubble.SetActive(true);
        }
        
        // Display the first line
        DisplayCurrentLine();

        // Start the auto-advance coroutine
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
        }
        autoAdvanceCoroutine = StartCoroutine(AutoAdvanceDialogue());
    }

    /// <summary>
    /// Stop the crow from talking
    /// </summary>
    public void StopTalking()
    {
        isTalking = false;
        currentLineIndex = 0;
        SessionManager.Instance.OnRavenFinished();
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }
        
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        // Stop the auto-advance coroutine
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }
    }

    /// <summary>
    /// Coroutine to automatically advance dialogue after inactivity timeout
    /// </summary>
    private IEnumerator AutoAdvanceDialogue()
    {
        while (isTalking)
        {
            yield return new WaitForSeconds(INACTIVITY_TIMEOUT);
            if (isTalking)
            {
                AdvanceDialogue();
            }
        }
    }

    /// <summary>
    /// Advance to the next line of dialogue
    /// </summary>
    public void AdvanceDialogue()
    {
        currentLineIndex++;
        
        // Check if we've reached the end of the dialogue
        if (currentLineIndex >= dialogueLines.Count)
        {
            StopTalking();
            return;
        }
        
        DisplayCurrentLine();
    }

    /// <summary>
    /// Display the current line in the TextMeshProUGUI component
    /// </summary>
    private void DisplayCurrentLine()
    {
        if (dialogueText != null && currentLineIndex < dialogueLines.Count)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }
    }

    // Optional: Method to add new lines to the existing dialogue
    public void AddDialogueLine(string line)
    {
        if (!string.IsNullOrEmpty(line))
        {
            dialogueLines.Add(line);
        }
    }

    // Optional: Method to clear all dialogue lines
    public void ClearDialogue()
    {
        dialogueLines.Clear();
        StopTalking();
    }
} 