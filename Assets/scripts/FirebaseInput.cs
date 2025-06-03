using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FirebaseInput : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private TMP_Dropdown favouriteExhibitDropdown;

    [Header("Dependencies")]
    [SerializeField] private FirebaseService firebaseService;

    [Header("UI Feedback")]
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private TextMeshProUGUI successText;

    [Header("Submit Button")]
    [SerializeField] private Button submitButton;

    private readonly string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

    private void Start()
    {
        InitializeExhibitDropdown();

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(HandleSubmit);
        }

        if (firebaseService == null)
        {
            firebaseService = FindAnyObjectByType<FirebaseService>();
            if (firebaseService == null)
            {
                ShowError("Unable to connect to the server. Please try again later.");
            }
        }

        // Hide feedback texts initially
        if (errorText != null) errorText.gameObject.SetActive(false);
        if (successText != null) successText.gameObject.SetActive(false);
    }

    private void InitializeExhibitDropdown()
    {
        if (favouriteExhibitDropdown != null && SessionManager.Instance != null)
        {
            favouriteExhibitDropdown.ClearOptions();

            List<string> options = new List<string>
            {
                "Select your favorite exhibit"  // Default option
            };

            // Add all place names from SessionManager
            foreach (var place in SessionManager.Instance.GetAllPlaces())
            {
                if (place != null)
                {
                    options.Add(place.PlaceName);
                }
            }

            favouriteExhibitDropdown.AddOptions(options);
        }
    }

    private async void HandleSubmit()
    {
        // Clear previous feedback
        HideError();
        HideSuccess();

        // Validate dropdown selection
        if (favouriteExhibitDropdown == null)
        {
            ShowError("Dropdown not found. Please try again.");
            return;
        }

        if (favouriteExhibitDropdown.value == 0)
        {
            ShowError("Please select your favorite exhibit before submitting.");
            return;
        }

        // Get the selected exhibit name
        string selectedExhibit = favouriteExhibitDropdown.options[favouriteExhibitDropdown.value].text;

        try
        {
            // Submit the vote
            bool success = await firebaseService.SubmitVote(selectedExhibit);

            if (success)
            {
                ShowSuccess($"Thank you! Your vote for '{selectedExhibit}' has been submitted successfully.");
                // Optionally clear the form or keep the selection
                // ClearInputFields();
            }
            else
            {
                ShowError("Failed to submit your vote. Please try again.");
            }
        }
        catch (System.Exception ex)
        {
            ShowError($"An error occurred: {ex.Message}");
            Debug.LogError($"Error submitting vote: {ex}");
        }
        finally
        {
            gameObject.SetActive(false); 
        }
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError(message);
        }
    }

    private void HideError()
    {
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }

    private void ShowSuccess(string message)
    {
        if (successText != null)
        {
            successText.text = message;
            successText.gameObject.SetActive(true);
        }
    }

    private void HideSuccess()
    {
        if (successText != null)
        {
            successText.gameObject.SetActive(false);
        }
    }

    private void ClearInputFields()
    {
        favouriteExhibitDropdown.value = 0;
    }

    private void OnDestroy()
    {
        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(HandleSubmit);
        }
    }
} 