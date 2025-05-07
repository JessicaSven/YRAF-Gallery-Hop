using System;
using System.Collections.Generic;
using UnityEngine;

public class FinishedPlaces : MonoBehaviour
{
    public List<PlaceSO> places = new List<PlaceSO>();
    public Transform placement;
    public FinishedIcon iconPrefab;
    private Dictionary<string, FinishedIcon> placeIcons = new Dictionary<string, FinishedIcon>();

    public void addPlace(PlaceSO temp)
    {
        places.Add(temp);
        FinishedIcon icon = Instantiate(iconPrefab, placement.position, Quaternion.identity, placement);
        icon.initalize(temp);
        placeIcons[temp.PlaceName] = icon;
        
        // Check if this place has been visited
        if (VisitedPlacesManager.instance != null)
        {
            icon.UpdateVisibility(VisitedPlacesManager.instance.HasVisitedPlace(temp.PlaceName));
        }
    }

    public void UpdateVisibility(HashSet<string> visitedPlaces)
    {
        foreach (var iconPair in placeIcons)
        {
            iconPair.Value.UpdateVisibility(visitedPlaces.Contains(iconPair.Key));
        }
    }
}
