using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject startingCanvas; // Reference to your starting canvas GameObject
    public GameObject mainCanvas; // Reference to your main canvas GameObject

    public PlaceDetailsUI placePopup;
    public GameObject finishCanvas;
    public GalleryList galleryList;
    public static  UIController instance; // Reference to the UIController script


    void Start()
    {
        // Set starting conditions
        startingCanvas.SetActive(true);
        mainCanvas.SetActive(false);
        placePopup.gameObject.SetActive(false); // Assuming this is the popup for place details
        instance = this;
        galleryList.gameObject.SetActive(false); // Hide the gallery list initially
    }

    // Function to toggle between starting and main canvas
    public void ToggleCanvas()
    {
        startingCanvas.SetActive(!startingCanvas.activeSelf);
        mainCanvas.SetActive(!mainCanvas.activeSelf);
    }

    // Example function to show place details (replace with your logic)
    public void ShowPlaceDetails(PlaceSO place)
    {
        placePopup.UpdatePlace(place); // Assuming UpdatePlace is a method in PlaceDetailsUI to update the UI with place details
        placePopup.gameObject.SetActive(true); // Assuming placeSO is a GameObject reference to your details panel
    }
    public void ShowgalleryList()
    {
        Debug.Log("test");
        galleryList.gameObject.SetActive(true); // Show the gallery list
    }
    public void ShowSubmitResult()
    {
        finishCanvas.SetActive(true); // Show the submit result popup
    }
}
