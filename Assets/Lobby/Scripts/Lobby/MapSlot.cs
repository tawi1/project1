using UnityEngine;
using UnityEngine.UI;

public class MapSlot : MonoBehaviour
{
    [SerializeField]
    private Image mapImage;
    [SerializeField]
    private Text mapName;

    public void SetMap(string mapName, Texture2D mapImage)
    {
        this.mapName.text = mapName;

        Sprite sprite = Sprite.Create(mapImage, new Rect(0, 0, mapImage.width, mapImage.height), new Vector2(0.5f, 0.5f));
        this.mapImage.sprite = sprite;
    }

    public void UnColorMap()
    {
        mapImage.color = new Color(mapImage.color.r, mapImage.color.g, mapImage.color.b, 0.3f);
    }

    public void ColorMap()
    {
        mapImage.color = new Color(mapImage.color.r, mapImage.color.g, mapImage.color.b, 1f);
    }
}
