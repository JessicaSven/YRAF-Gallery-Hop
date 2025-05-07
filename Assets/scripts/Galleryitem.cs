using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GalleryItem : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI titleText;

    public void Initialize(PlaceSO place)
    {
        iconImage.sprite = place.Icon;
        titleText.text = place.PlaceName;
    }
}
