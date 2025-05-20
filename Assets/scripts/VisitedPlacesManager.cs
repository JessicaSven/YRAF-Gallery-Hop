using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VisitedPlacesManager : MonoBehaviour
{
    public static VisitedPlacesManager instance;
    private HashSet<string> visitedPlaces = new HashSet<string>();
    private const string VISITED_PLACES_KEY = "VisitedPlaces";
    
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

    public void LoadVisitedPlaces()
    {
        string savedPlaces = PlayerPrefs.GetString(VISITED_PLACES_KEY, "");
        if (!string.IsNullOrEmpty(savedPlaces))
        {
            string[] places = savedPlaces.Split(',');
            visitedPlaces = new HashSet<string>(places);
            
            // Update finished places and create prefabs for all visited places
            foreach (string placeName in visitedPlaces)
            {
                PlaceSO place = sessionManager.places.FirstOrDefault(p => p.PlaceName == placeName);
                if (place != null)
                {
                    sessionManager.finishedPlaces.addPlace(place);
                }
            }
            
            
            UpdatePlacesVisibility();
        }
    }

    private void SaveVisitedPlaces()
    {
        string placesString = string.Join(",", visitedPlaces);
        PlayerPrefs.SetString(VISITED_PLACES_KEY, placesString);
        PlayerPrefs.Save();
    }

    public void MarkPlaceAsVisited(PlaceSO place)
    {
        if (!visitedPlaces.Contains(place.PlaceName))
        {
            print("Visited: " + place.PlaceName);
            visitedPlaces.Add(place.PlaceName);
            UpdatePlacesVisibility();
            sessionManager.HandleSucessfulSubmission(place);
            SaveVisitedPlaces();
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

    public void ClearAllVisitedPlaces()
    {
        visitedPlaces.Clear();
        PlayerPrefs.DeleteKey(VISITED_PLACES_KEY);
        PlayerPrefs.Save();
        UpdatePlacesVisibility();
        sessionManager.HandleClearAllVisitedPlaces();
    }
} 