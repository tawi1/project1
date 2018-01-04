using UnityEngine;
using UnityEngine.UI;
using System;

public class CarLobbySlot : MonoBehaviour
{
    [SerializeField]
    private Button carButton;
    [SerializeField]
    private Text nickNameText;
    [SerializeField]
    private Sprite[] readyImage;
    [SerializeField]
    private Button readyButton;
    [SerializeField]
    private Button aiButton;
    [SerializeField]
    private Text aiLevel;
    [SerializeField]
    private GameObject plus;
    [SerializeField]
    private Button kickButton;

    public void SetCar(int idCar, string nickName)
    {
        this.carButton.image.sprite = TextureLoader.GetSpriteCar(idCar);
        this.carButton.image.color = new Color32(255, 255, 255, 255);
        nickNameText.text = nickName;
        readyButton.gameObject.SetActive(true);
        aiButton.gameObject.SetActive(false);
        plus.SetActive(false);
        kickButton.gameObject.SetActive(false);
        nickNameText.gameObject.SetActive(true);
    }

    public void UnSetCar()
    {
        carButton.image.sprite = null;
        carButton.image.color = new Color32(28, 58, 71, 202);
        carButton.onClick.RemoveAllListeners();
        readyButton.onClick.RemoveAllListeners();
        plus.SetActive(true);
        kickButton.gameObject.SetActive(false);
        nickNameText.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(true);
        aiButton.gameObject.SetActive(false);
        SetReady(false, true);
    }

    public void SetAi(int idCar, string nickName, string aiLevel)
    {
        SetCar(idCar, nickName);
        this.aiLevel.text = aiLevel;

        readyButton.gameObject.SetActive(false);
        aiButton.gameObject.SetActive(true);

    }

    public void OnCarClick(UnityEngine.Events.UnityAction action)
    {
        carButton.onClick.RemoveAllListeners();
        carButton.onClick.AddListener(action);
    }

    public void OnReadyClick(UnityEngine.Events.UnityAction action)
    {
        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(action);
    }

    public void OnSetAiLevelClick(UnityEngine.Events.UnityAction action)
    {
        aiButton.onClick.RemoveAllListeners();
        aiButton.onClick.AddListener(action);
    }

    public void SetKick(UnityEngine.Events.UnityAction action)
    {
        kickButton.gameObject.SetActive(true);
        kickButton.onClick.RemoveAllListeners();
        kickButton.onClick.AddListener(action);
    }

    public void SetReady(bool ready, bool local)
    {
        if (ready)
        {
            readyButton.image.overrideSprite = this.readyImage[0];
            carButton.interactable = false;
        }
        else
        {
            readyButton.image.overrideSprite = this.readyImage[1];

            readyButton.interactable = local;
            carButton.interactable = local;         
        }
    }

    public void SetColor(int colorIndex)
    {
        Color32 newColor = new Color(CarColors.Colors[colorIndex].r, CarColors.Colors[colorIndex].g, CarColors.Colors[colorIndex].b, carButton.image.color.a);
        carButton.image.color = newColor;
    }


}
