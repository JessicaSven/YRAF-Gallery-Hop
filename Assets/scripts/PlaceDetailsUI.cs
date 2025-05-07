using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaceDetailsUI : MonoBehaviour
{
    public Image image;

    public TextMeshProUGUI placeName, description;

    public void UpdatePlace(PlaceSO item)
    {
        image.sprite = item.Icon;
        placeName.text = item.PlaceName;
        description.text = item.Content;
    }   
}
