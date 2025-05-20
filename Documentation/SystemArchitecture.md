# System Architecture Documentation

## Class Diagram

```mermaid
classDiagram
    class SessionManager {
        -PlaceSO[] places
        -InteractablePlace interactablePlacePrefab
        -List<InteractablePlace> interactablePlaces
        -FinishedPlaces finishedPlaces
        -GalleryList galleryList
        -GameObject qrScannerObject
        +UnityEvent<float> onProgressChanged
        +Awake()
        +Start()
        +UpdateVisibility()
        +EnableQRScanner()
        +DisableQRScanner()
        +HandleScannedURL()
        +HandleSucessfulSubmission()
        +IntroRaven()
        +ShowRandomRavenHint()
        +GetAllPlaces()
        +OnRavenFinished()
    }

    class UIController {
        +GameObject startingCanvas
        +GameObject mainCanvas
        +PlaceDetailsUI placePopup
        +FirebaseInput firebaseInput
        +GalleryList galleryList
        +static UIController instance
        +Start()
        +ToggleCanvas()
        +ShowPlaceDetails(PlaceSO)
        +ShowgalleryList()
        +ShowSubmitResult()
    }

    class TalkingRaven {
        +static TalkingRaven _instance
        -GameObject speechBubble
        -TextMeshProUGUI dialogueText
        -List<string> dialogueLines
        -bool isTalking
        -int currentLineIndex
        +StartTalking(List<string>)
        +StopTalking()
        +AdvanceDialogue()
        +AddDialogueLine(string)
        +ClearDialogue()
    }

    class SoundManager {
        +static SoundManager Instance
        -AudioSource audioSource
        -AudioClip[] ravenSounds
        -AudioClip successSound
        -bool isSoundEnabled
        -float previousVolume
        +PlayRandomRavenSound()
        +PlaySuccess()
        +ToggleSound()
        +SetVolume(float)
        +IsSoundEnabled()
    }

    class FirebaseService {
        +SaveUserData(UserData)
        +GetUserData()
    }

    class FirebaseInput {
        -TMP_InputField emailInputField
        -TMP_InputField fullNameInputField
        -TMP_Dropdown favouriteExhibitDropdown
        -FirebaseService firebaseService
        -TextMeshProUGUI errorText
        -TextMeshProUGUI successText
        -Button submitButton
        +Start()
        +HandleSubmit()
        +InitializeExhibitDropdown()
        +ShowError(string)
        +ShowSuccess(string)
    }

    class QRScanner {
        -WebCamTexture camTexture
        -BarcodeReader barcodeReader
        -RawImage cameraDisplay
        -Image overlayImage
        -Button exitButton
        -TextMeshProUGUI statusText
        -TextMeshProUGUI scannedTextDisplay
        -float fadeInDuration
        +StartCameraWhenReady()
        +HandleScannedURL()
    }

    class VisitedPlacesManager {
        -HashSet<string> visitedPlaces
        +MarkPlaceAsVisited(PlaceSO)
        +GetUnvisitedPlaces()
        +GetRandomUnvisitedPlaceHint()
    }

    class PlaceSO {
        +string PlaceName
        +string Url
        +string RavenSucessScript
        +Vector3 Position
    }

    class InteractablePlace {
        -PlaceSO place
        +initialize(PlaceSO)
        +UpdateVisibility(bool)
    }

    class GalleryList {
        +List<GalleryItem> items
        +AddGalleryItem(PlaceSO)
    }

    class FinishedPlaces {
        +List<PlaceSO> places
        +addPlace(PlaceSO)
        +UpdateVisibility()
    }

    SessionManager --> PlaceSO : manages
    SessionManager --> InteractablePlace : creates
    SessionManager --> FinishedPlaces : manages
    SessionManager --> GalleryList : manages
    SessionManager --> QRScanner : controls
    SessionManager --> TalkingRaven : triggers
    SessionManager --> SoundManager : uses

    UIController --> PlaceDetailsUI : manages
    UIController --> FirebaseInput : manages
    UIController --> GalleryList : manages

    TalkingRaven --> SoundManager : uses
    TalkingRaven --> SessionManager : notifies

    FirebaseInput --> FirebaseService : uses
    FirebaseInput --> SessionManager : uses

    QRScanner --> SessionManager : notifies

    InteractablePlace --> PlaceSO : references
    VisitedPlacesManager --> PlaceSO : tracks
```

## System Flow

```mermaid
sequenceDiagram
    participant User
    participant UIController
    participant SessionManager
    participant QRScanner
    participant VisitedPlacesManager
    participant SoundManager
    participant TalkingRaven
    participant FirebaseService

    User->>UIController: Start Game
    UIController->>SessionManager: Initialize
    SessionManager->>TalkingRaven: IntroRaven
    TalkingRaven->>SoundManager: PlayRandomRavenSound

    User->>QRScanner: Scan QR Code
    QRScanner->>SessionManager: HandleScannedURL
    SessionManager->>VisitedPlacesManager: MarkPlaceAsVisited
    VisitedPlacesManager-->>SessionManager: Update Progress
    SessionManager->>SoundManager: PlaySuccess
    SessionManager->>TalkingRaven: Show Success Message

    User->>UIController: Show Submit Form
    UIController->>FirebaseInput: Show Form
    User->>FirebaseInput: Submit Form
    FirebaseInput->>FirebaseService: SaveUserData
    FirebaseService-->>FirebaseInput: Success
    FirebaseInput->>SessionManager: HandleSucessfulSubmission
```

## Key Features and Responsibilities

### SessionManager

- Central manager for the application
- Handles place management and progression
- Coordinates between different systems
- Manages QR scanner and raven interactions
- Controls game flow and state

### UIController

- Manages all UI state and transitions
- Controls canvas visibility
- Handles place details display
- Manages gallery list visibility
- Coordinates form submission

### SoundManager

- Handles all audio playback
- Manages sound state (on/off)
- Controls volume settings
- Provides sound effects for various events
- Maintains volume state when toggling

### FirebaseService

- Handles data persistence
- Manages user data storage
- Provides data retrieval functionality
- Handles async operations

### QRScanner

- Manages camera access
- Handles QR code scanning
- Provides visual feedback
- Implements smooth fade-in transitions
- Manages UI elements for scanning

### VisitedPlacesManager

- Tracks visited places
- Manages place completion state
- Provides hints for unvisited places
- Maintains visit history

### TalkingRaven

- Manages dialogue system
- Coordinates with SoundManager for audio
- Provides user guidance and feedback
- Handles dialogue progression
- Manages speech bubble visibility
