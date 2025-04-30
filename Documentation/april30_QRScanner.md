# QR Scanner System Documentation

## Overview

The QR Scanner system is designed to allow users to scan QR codes associated with different places in the gallery application. When a QR code is scanned, the system automatically marks the corresponding place as visited and updates the UI accordingly.

## Components

### 1. QRScanner (QRScanner.cs)

The core component responsible for handling camera access and QR code detection.

#### Key Features:

- Uses ZXing library for QR code detection
- Handles camera permissions automatically
- Attempts to use rear camera by default
- Real-time QR code scanning
- Visual feedback through UI

#### Technical Details:

- Camera initialization happens in `StartCameraWhenReady` coroutine
- Scans are performed in the Update loop
- Automatically handles camera orientation and scaling
- Shows scanning status and results on the UI

### 2. SessionManager (SessionManager.cs)

Manages the QR scanner state and coordinates between scanned URLs and place visits.

#### Key Methods:

```csharp
// Enable the QR scanner
SessionManager.Instance.EnableQRScanner();

// Disable the QR scanner
SessionManager.Instance.DisableQRScanner();
```

#### URL Processing:

When a QR code is scanned, `HandleScannedURL(string url)` is called, which:

1. Searches through all registered places
2. Matches the scanned URL with place URLs
3. Marks matching places as visited
4. Automatically disables the scanner after processing

### 3. VisitedPlacesManager (VisitedPlacesManager.cs)

Handles the state of visited places and updates the UI accordingly.

## Usage

### Setting Up Places

1. Create a PlaceSO (Place Scriptable Object) for each location
2. Configure the following for each place:
   - `placeName`: Unique identifier for the place
   - `url`: The URL that will be encoded in the QR code
   - `description`: Place description
   - `icon`: Visual representation
   - `position`: Position on the map

### QR Code Generation (External)

1. Generate QR codes containing the exact URLs specified in the PlaceSO objects
2. Place these QR codes in the physical locations

### Implementation Flow

1. **Scanner Activation**

   ```csharp
   // To start scanning
   SessionManager.Instance.EnableQRScanner();
   ```

2. **Automatic Processing**

   - Camera activates and begins scanning
   - When a valid QR code is detected:
     - URL is extracted and checked against registered places
     - Matching place is marked as visited
     - Scanner deactivates automatically

3. **UI Updates**
   - Place indicators update visibility
   - Progress bar updates
   - Gallery list reflects visited status

## Best Practices

1. **QR Code Placement**

   - Ensure good lighting conditions
   - Avoid reflective surfaces
   - Maintain appropriate size for scanning distance

2. **URL Management**

   - Use consistent URL format
   - Test QR codes before deployment
   - Consider URL validation in the scanning process

3. **Error Handling**
   - Camera permission denials are handled gracefully
   - Invalid QR codes are ignored
   - Missing cameras show appropriate error messages

## Technical Requirements

- Unity 2020.3 or later
- ZXing library for QR code processing
- Camera permissions on target device
- Sufficient lighting for QR code detection

## Integration Example

```csharp
// Example of integrating scanner activation into a UI button
public class ScanButton : MonoBehaviour
{
    public void OnScanButtonPressed()
    {
        SessionManager.Instance.EnableQRScanner();
    }
}
```

## Troubleshooting

1. **Camera Not Working**

   - Check camera permissions
   - Verify device has camera
   - Check camera access in system settings

2. **QR Codes Not Scanning**

   - Verify URL format matches PlaceSO
   - Check lighting conditions
   - Ensure QR code is within camera view

3. **Places Not Marking as Visited**
   - Verify URLs match exactly
   - Check VisitedPlacesManager is properly referenced
   - Verify place names are unique

## Future Improvements

- Add URL validation
- Implement error feedback
- Add timeout for failed scans
- Support for multiple QR code formats
- Improved error messaging
