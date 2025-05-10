using UnityEngine;
using ZXing; // ZXing barcode reader
using ZXing.Common;
using System.Collections; // Required for IEnumerator
using UnityEngine.UI;
using TMPro;

public class QRScanner : MonoBehaviour
{
    [Header("Camera Settings")]
    private WebCamTexture camTexture;
    private BarcodeReader barcodeReader;
    public string scannedText = "";

    [Header("UI References")]
    [SerializeField] private RawImage cameraDisplay;
    [SerializeField] private Image overlayImage;
    [SerializeField] private Button exitButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI scannedTextDisplay;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    private float currentFadeTime = 0f;
    private bool isFading = false;

    private bool cameraStarted = false;
    private bool permissionDenied = false;

    void OnEnable()
    {
        if (cameraDisplay != null)
        {
            // Start with fully transparent
            Color color = cameraDisplay.color;
            color.a = 0f;
            cameraDisplay.color = color;
        }
        StartCoroutine(StartCameraWhenReady());
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }
    }

    void OnDisable()
    {
        if (camTexture != null)
        {
            camTexture.Stop();
            cameraStarted = false;
        }
        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }
    }

    private void OnExitButtonClicked()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator StartCameraWhenReady()
    {
        // Request camera permission
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogError("Camera permission denied.");
            permissionDenied = true;
            UpdateStatusText("Camera access denied");
            yield break;
        }

        // Wait until WebCam devices are available
        float timeout = 5f;
        while (WebCamTexture.devices.Length == 0 && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("No camera devices found.");
            permissionDenied = true;
            UpdateStatusText("No camera found");
            yield break;
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        string selectedCameraName = null;

        foreach (var device in devices)
        {
            if (!device.isFrontFacing)
            {
                selectedCameraName = device.name;
                break;
            }
        }

        if (selectedCameraName == null)
            selectedCameraName = devices[0].name;

        camTexture = new WebCamTexture(selectedCameraName);
        camTexture.Play();

        // Wait for the camera to start
        while (!camTexture.isPlaying)
        {
            yield return null;
        }

        if (cameraDisplay != null)
        {
            cameraDisplay.texture = camTexture;
            // Adjust the RawImage's aspect ratio to match the camera
            float aspectRatio = (float)camTexture.width / camTexture.height;
            cameraDisplay.rectTransform.sizeDelta = new Vector2(
                cameraDisplay.rectTransform.rect.height * aspectRatio,
                cameraDisplay.rectTransform.rect.height
            );

            // Start fade in
            currentFadeTime = 0f;
            isFading = true;
        }

        barcodeReader = new BarcodeReader();
        cameraStarted = true;
        UpdateStatusText("Scanning...");
    }

    void Update()
    {
        if (isFading && cameraDisplay != null)
        {
            currentFadeTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(currentFadeTime / fadeInDuration);
            Color color = cameraDisplay.color;
            color.a = alpha;
            cameraDisplay.color = color;

            if (alpha >= 1f)
            {
                isFading = false;
            }
        }

        if (permissionDenied || !cameraStarted)
            return;

        if (camTexture != null && camTexture.isPlaying && camTexture.width > 100)
        {
            try
            {
                var colors = camTexture.GetPixels32();
                var width = camTexture.width;
                var height = camTexture.height;

                var result = barcodeReader.Decode(colors, width, height);

                if (result != null)
                {
                    scannedText = result.Text;
                    Debug.Log("Scanned QR Code: " + scannedText);
                    UpdateScannedText("Scanned: " + scannedText);

                    if (SessionManager.Instance != null)
                    {
                        SessionManager.Instance.HandleScannedURL(scannedText);
                    }
                }
            }
            catch { }
        }
    }

    private void UpdateStatusText(string text)
    {
        if (statusText != null)
        {
            statusText.text = text;
        }
    }

    private void UpdateScannedText(string text)
    {
        if (scannedTextDisplay != null)
        {
            scannedTextDisplay.text = text;
        }
    }
}
