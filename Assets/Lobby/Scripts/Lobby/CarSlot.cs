using UnityEngine;
using UnityEngine.UI;

public class CarSlot : MonoBehaviour
{
    [SerializeField]
    private Text priceValue;
    [SerializeField]
    private Image carImage;
    [SerializeField]
    private GameObject[] overlays;
    [SerializeField]
    private Image chain;

    public void SetCar(CarSpecification.CarSpec car, Sprite carImage)
    {
        priceValue.text = car.price.ToString();
        this.carImage.sprite = carImage;
    }

    public void SetColor(int colorIndex)
    {
        Color32 newColor = new Color(CarColors.Colors[colorIndex].r, CarColors.Colors[colorIndex].g, CarColors.Colors[colorIndex].b, this.carImage.color.a);
        this.carImage.color = newColor;
    }

    public void UnlockSlot()
    {
        for (int i = 0; i < overlays.Length; i++)
        {
            overlays[i].SetActive(false);
        }
    }

    public void UnColorCar()
    {
        carImage.color = new Color(carImage.color.r, carImage.color.g, carImage.color.b, 0.4f);
    }

    public void ColorCar()
    {
        carImage.color = new Color(carImage.color.r, carImage.color.g, carImage.color.b, 1);
    }

    public void UnColorChain()
    {
        chain.color = new Color32(147, 147, 147, 110);
    }

    public void ColorChain()
    {
        chain.color = new Color32(147, 147, 147, 200);
    }
}
