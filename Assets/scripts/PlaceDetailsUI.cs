using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaceDetailsUI : MonoBehaviour
{
    public Image image;

    public TextMeshProUGUI placeName, description;

    public void UpdatePlace(PlaceSO item)
    {
        image.sprite = item.icon;
        placeName.text = item.placeName;
        description.text = item.description;
    }   
}
