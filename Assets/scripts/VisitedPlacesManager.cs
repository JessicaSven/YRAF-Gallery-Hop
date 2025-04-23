using System.Collections.Generic;
using UnityEngine;

public class VisitedPlacesManager : MonoBehaviour
{
    public static VisitedPlacesManager instance;
    private HashSet<string> visitedPlaces = new HashSet<string>();
    
    public SessionManager sessionManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MarkPlaceAsVisited(string placeName)
    {
        if (!visitedPlaces.Contains(placeName))
        {
            print("Visited: " + placeName);
            visitedPlaces.Add(placeName);
            UpdatePlacesVisibility();
        }
    }

    public bool HasVisitedPlace(string placeName)
    {
        return visitedPlaces.Contains(placeName);
    }

    private void UpdatePlacesVisibility()
    {
        sessionManager.UpdateVisibility(visitedPlaces);
    }
} 