using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }
    public PlaceSO[] places;
    public InteractablePlace interactablePlacePrefab;
    public List<InteractablePlace> interactablePlaces;
    public FinishedPlaces finishedPlaces;
    public GalleryList galleryList;

    public Transform interactableItemsParent;

    public UnityEvent<float> onProgressChanged = new UnityEvent<float>();

    public GameObject qrScannerObject;

    // Event to broadcast the scanned URL
    public event Action<string> OnQRCodeScanned;

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

    private void Start() {
        foreach (var item in places) {
            InteractablePlace temp = Instantiate(interactablePlacePrefab, interactableItemsParent);
            temp.transform.localPosition = item.Position;
            temp.initialize(item);
            interactablePlaces.Add(temp);
            finishedPlaces.addPlace(item);
            galleryList.AddGalleryItem(item);
        }
    }

    public void UpdateVisibility(HashSet<string> visitedPlaces)
    {
        foreach (var place in interactablePlaces)
        {
            print(visitedPlaces);
            print(place.place.PlaceName + " visited: " + visitedPlaces.Contains(place.place.PlaceName));
            place.UpdateVisibility(visitedPlaces.Contains(place.place.PlaceName));
        }
        finishedPlaces.UpdateVisibility(visitedPlaces);

        // Calculate progress and invoke event
        float progress = (float)visitedPlaces.Count / places.Length;
        onProgressChanged.Invoke(progress);
    }

    public void EnableQRScanner()
    {
        if (qrScannerObject != null)
        {
            qrScannerObject.SetActive(true);
        }
    }

    public void DisableQRScanner()
    {
        if (qrScannerObject != null)
        {
            qrScannerObject.SetActive(false);
        }
    }

    public void HandleScannedURL(string url)
    {
        // Look for a matching place with this URL
        foreach (var place in places)
        {
            if (place.Url == url)
            {
                // Mark the place as visited using the VisitedPlacesManager
                if (VisitedPlacesManager.instance != null)
                {
                    VisitedPlacesManager.instance.MarkPlaceAsVisited(place.PlaceName);
                }
                break;
            }
        }
        
        // Disable the scanner after scan
        DisableQRScanner();
    }

    public void IntroRaven()
    {
        // Check if the raven is already talking
        if (TalkingRaven._instance != null)
        {   
            List<string> RavenLines = new List<string>() {
                "Caw! Hello there!",
                "Click or press space to continue...",
                "This is the last line!"
            };
            TalkingRaven._instance.StartTalking(RavenLines);
        }
    }
}
