using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FirebaseInput : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField fullNameInputField;
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

        // Validate input fields
        if (string.IsNullOrEmpty(emailInputField.text))
        {
            ShowError("Please enter your email address");
            return;
        }

        if (!Regex.IsMatch(emailInputField.text, EmailPattern))
        {
            ShowError("Please enter a valid email address (e.g., name@example.com)");
            return;
        }

        if (string.IsNullOrEmpty(fullNameInputField.text))
        {
            ShowError("Please enter your name");
            return;
        }

        if (favouriteExhibitDropdown.value == 0)  // First option is our default "Select your favorite exhibit"
        {
            ShowError("Please select your favorite exhibit");
            return;
        }

        // Disable submit button while processing
        if (submitButton != null) submitButton.interactable = false;

        try
        {
            // Create user data object
            UserData userData = new UserData
            {
                Email = emailInputField.text,
                FullName = fullNameInputField.text,
                FavouriteExhibit = favouriteExhibitDropdown.options[favouriteExhibitDropdown.value].text,
                CompletedGame = true  // Default value for new users
            };

            await firebaseService.SaveUserData(userData);
            ShowSuccess("Thank you for your submission!");
            ClearInputFields();
            gameObject.SetActive(false); // Hide the input form after submission
            SessionManager.Instance.RevealSecretPlace();
        }
        catch (System.Exception e)
        {
            ShowError("Unable to save your information. Please try again later.");
        }
        finally
        {
            // Re-enable submit button
            if (submitButton != null) submitButton.interactable = true;
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
        emailInputField.text = "";
        fullNameInputField.text = "";
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