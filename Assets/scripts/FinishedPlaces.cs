using System;
using System.Collections.Generic;
using UnityEngine;

public class FinishedPlaces : MonoBehaviour
{
    public List<PlaceSO> places = new List<PlaceSO>();
    public Transform placement;
    public FinishedIcon iconPrefab;
    public void addPlace(PlaceSO temp)
    {
        places.Add(temp);
        FinishedIcon icon = Instantiate(iconPrefab, placement.position, Quaternion.identity, placement);
        icon.initalize(temp);
    }

}
