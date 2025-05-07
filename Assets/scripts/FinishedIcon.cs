using System;
using UnityEngine;
using UnityEngine.UI;

public class FinishedIcon : MonoBehaviour
{
    public Image image;
    private PlaceSO place;

    public Button button;

    public void initalize(PlaceSO temp)
    {
        place = temp;
        image.sprite = temp.Icon;
    }

    public void UpdateVisibility(bool hasBeenVisited)
    {
        // For finished places: When visited, show full opacity (1.0)
        // When not visited yet, show half opacity (0.5)
        image.color = hasBeenVisited ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.5f);
    }

    public void OnClick()
    {
        VisitedPlacesManager.instance.MarkPlaceAsVisited(place.PlaceName);
    }
}
