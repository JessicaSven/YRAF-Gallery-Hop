using UnityEngine;
using UnityEngine.UI;

public class InteractablePlace : MonoBehaviour
{
    public PlaceSO place;
    public Image image;
    public GameObject visitedIndicator;

    internal void initialize(PlaceSO item)
    {
        place = item;
        image.sprite = item.icon;
        UpdateVisibility(VisitedPlacesManager.instance.HasVisitedPlace(item.placeName));
        if (visitedIndicator != null)
        {
            visitedIndicator.SetActive(false);
        }
    }

    public void Ontap()
    {
        UIController.instance.ShowPlaceDetails(place);
    }

    public void UpdateVisibility(bool hasBeenVisited)
    {
        print("Updating visibility for " + place.name + " to " + hasBeenVisited);
        image.color = hasBeenVisited ? new Color(1f, 1f, 1f, 0.5f) : new Color(1f, 1f, 1f, 1f);
        if (visitedIndicator != null)
        {
            visitedIndicator.SetActive(hasBeenVisited);
        }
    }
}
