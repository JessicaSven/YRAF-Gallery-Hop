using System.Collections.Generic;
using UnityEngine;

public class GalleryList : MonoBehaviour
{
    public Transform contentParent;         // Where to spawn the items (like inside a ScrollView)
    public GameObject galleryItemPrefab;    // Assign the GalleryItem prefab here

    private List<GameObject> galleryItems = new List<GameObject>();

    public void AddGalleryItem(PlaceSO place)
    {
        GameObject newItem = Instantiate(galleryItemPrefab, contentParent);
        GalleryItem itemComponent = newItem.GetComponent<GalleryItem>();
        itemComponent.Initialize(place);
        galleryItems.Add(newItem);
    }
}
