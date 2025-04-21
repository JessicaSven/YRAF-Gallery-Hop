using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public PlaceSO[] places;
    public InteractablePlace interactablePlacePrefab;
    public FinishedPlaces finishedPlaces;
    public GalleryList galleryList;

    public Transform interactableItemsParent;

    private void Start() {
        foreach (var item in places) {
            Debug.Log(item.name);
            InteractablePlace temp = Instantiate(interactablePlacePrefab, interactableItemsParent);
            temp.transform.localPosition = item.position;
            temp.initialize(item);
            finishedPlaces.addPlace(item);
            galleryList.AddGalleryItem(item);
        }

    }
}
