using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    public void MarkPlaceAsVisited(PlaceSO place)
    {
        if (!visitedPlaces.Contains(place.PlaceName))
        {
            print("Visited: " + place.PlaceName);
            visitedPlaces.Add(place.PlaceName);
            UpdatePlacesVisibility();
            sessionManager.HandleSucessfulSubmission(place);
        }
    }

    public bool HasVisitedPlace(string placeName)
    {
        return visitedPlaces.Contains(placeName);
    }

    public List<string> GetUnvisitedPlaces()
    {
        // Get all place names from SessionManager
        var allPlaceNames = sessionManager.places.Select(p => p.PlaceName).ToList();
        // Return only the places that haven't been visited
        return allPlaceNames.Where(placeName => !visitedPlaces.Contains(placeName)).ToList();
    }

    public string GetRandomUnvisitedPlaceHint()
    {
        var unvisitedPlaces = GetUnvisitedPlaces();
        if (unvisitedPlaces.Count == 0)
            return null;

        // Get a random unvisited place name
        string randomPlaceName = unvisitedPlaces[Random.Range(0, unvisitedPlaces.Count)];
        
        // Find the matching PlaceSO and return its hint
        var place = sessionManager.places.FirstOrDefault(p => p.PlaceName == randomPlaceName);
        return place?.RavenHintScript;
    }

    private void UpdatePlacesVisibility()
    {
        sessionManager.UpdateVisibility(visitedPlaces);
    }
} 