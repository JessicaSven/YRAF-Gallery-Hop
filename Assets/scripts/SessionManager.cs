using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SessionManager : MonoBehaviour
{
    public PlaceSO[] places;
    public InteractablePlace interactablePlacePrefab;
    public List<InteractablePlace> interactablePlaces;
    public FinishedPlaces finishedPlaces;
    public GalleryList galleryList;

    public Transform interactableItemsParent;

    public UnityEvent<float> onProgressChanged = new UnityEvent<float>();

    private void Start() {
        foreach (var item in places) {
            InteractablePlace temp = Instantiate(interactablePlacePrefab, interactableItemsParent);
            temp.transform.localPosition = item.position;
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
            print(place.place.placeName + " visited: " + visitedPlaces.Contains(place.place.placeName));
            place.UpdateVisibility(visitedPlaces.Contains(place.place.placeName));
        }
        finishedPlaces.UpdateVisibility(visitedPlaces);

        // Calculate progress and invoke event
        float progress = (float)visitedPlaces.Count / places.Length;
        onProgressChanged.Invoke(progress);
    }
}
