using UnityEngine;
using ZXing; // ZXing barcode reader
using ZXing.Common;
using System.Collections; // Required for IEnumerator

public class QRScanner : MonoBehaviour
{
    private WebCamTexture camTexture;
    private BarcodeReader barcodeReader;
    public string scannedText = "";

    private bool cameraStarted = false;
    private bool permissionDenied = false;

    void Start()
    {
        StartCoroutine(StartCameraWhenReady());
    }

    private IEnumerator StartCameraWhenReady()
    {
        // Request camera permission
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogError("Camera permission denied.");
            permissionDenied = true;
            yield break; // Exit
        }

        // Wait until WebCam devices are available
        float timeout = 5f; // 5 seconds max
        while (WebCamTexture.devices.Length == 0 && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("No camera devices found.");
            permissionDenied = true;
            yield break; // Exit
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        string selectedCameraName = null;

        // Try to select rear camera if possible
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

        // Create and start the camera
        camTexture = new WebCamTexture(selectedCameraName);
        camTexture.Play();

        barcodeReader = new BarcodeReader();

        cameraStarted = true;
    }

    void Update()
    {
        if (permissionDenied || !cameraStarted)
            return; // Don't do anything if not allowed or not started

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

                    camTexture.Stop(); // Optional
                }
            }
            catch { }
        }
    }

    private void OnGUI()
    {
        if (permissionDenied)
        {
            GUI.Label(new Rect(10, 10, 400, 50), "Camera access denied or no camera found.");
            return;
        }

        if (camTexture != null)
        {
            float camAspect = (float)camTexture.width / (float)camTexture.height;
            float screenAspect = (float)Screen.width / (float)Screen.height;

            float scaledHeight, scaledWidth;
            if (camAspect > screenAspect)
            {
                scaledWidth = Screen.width;
                scaledHeight = Screen.width / camAspect;
            }
            else
            {
                scaledHeight = Screen.height;
                scaledWidth = Screen.height * camAspect;
            }

            float x = (Screen.width - scaledWidth) / 2;
            float y = (Screen.height - scaledHeight) / 2;

            GUI.DrawTexture(new Rect(x, y, scaledWidth, scaledHeight), camTexture, ScaleMode.ScaleToFit);

            if (string.IsNullOrEmpty(scannedText))
            {
                GUI.Label(new Rect(10, 10, 300, 50), "Scanning...");
            }
            else
            {
                GUI.Label(new Rect(10, 10, 500, 50), "Scanned: " + scannedText);
            }
        }
    }
}
