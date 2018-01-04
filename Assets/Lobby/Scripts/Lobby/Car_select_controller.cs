using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Car_select_controller : MonoBehaviour
{
    [SerializeField]
    private Prototype.NetworkLobby.LobbyManager lobby;

    private int car_id = 0;
    private int color_id = 0;

    private int Max_slots = 6;
    private List<CarSlot> carSlots = new List<CarSlot>();
    [SerializeField]
    private int Start_HeroID = 0;

    public static List<int> cars = new List<int>();

    [SerializeField]
    protected int currentId = 0;
    protected int currentSelectedId = 0;

    protected float elapsed_time;
    protected float shift = 500;
    protected float startPos = 0;
    protected Vector2 startPosPanel;
    [SerializeField]
    protected RectTransform grid;
    [SerializeField]
    protected ScrollRect scroll;

    [HideInInspector]
    [SerializeField]
    private GameObject[] buttons;
    [HideInInspector]
    [SerializeField]
    private GameObject carPanel;
    [HideInInspector]
    [SerializeField]
    private GameObject mapPanel;
    [SerializeField]
    protected LobbyController lobbyController;
    [SerializeField]
    private GameObject lobbyPanel;

    [SerializeField]
    private GameObject colorsPanel;
    [SerializeField]
    private GameObject carSlotPrefab;
    [SerializeField]
    private Buy_controller buyController;

    private Button[] colors;
    private Dictionary<int, Texture2D> carsTexture;
    private bool carSelectionMode = true;
    private bool loaded = false;

    public virtual void Init()
    {
        carPanel.SetActive(true);
        Load();
        SetColors();
        LoadSavedIndex();
        InitButtons();
        UnlockHeroes();

        InitGrid(carSlots.Count);

        startPosPanel = carPanel.GetComponent<RectTransform>().localPosition;
        carPanel.SetActive(false);
    }

    private void Load()
    {
        LoadFromXml();
        FillGrid();
    }

    private void OnEnable()
    {
        if (carSlots.Count > 0)
        {
            carPanel.SetActive(true);
            Open();
            carPanel.GetComponent<RectTransform>().localPosition = startPosPanel;
            lobbyPanel.SetActive(false);
        }
    }

    public virtual void OnEnablePanel()
    {
        if (PhotonNetwork.offlineMode == false)
        {
            var list = CarColors.GetChosenColors();

            if (list.Contains(color_id))
            {
                list.Remove(color_id);
            }
            CheckEnabledColors(list);
        }

        OnColorClick(color_id);

        carPanel.SetActive(true);
    }

    private void LoadFromXml()
    {
        cars = RyXmlTools.LoadFromXml();
    }

    private void FillGrid()
    {
        ClearGrid();

        foreach (var car in cars)
        {
            var carSlot = Instantiate(carSlotPrefab, grid).GetComponent<CarSlot>();

            carSlots.Add(carSlot);
            carSlot.SetCar(CarSpecification.GetCar(car), TextureLoader.GetSpriteCar(CarSpecification.GetCar(car).id));

            carSlot.transform.SetSiblingIndex(carSlot.transform.GetSiblingIndex() - 1);
        }
    }

    private void ClearGrid()
    {
        CarSlot[] list = grid.GetComponentsInChildren<CarSlot>();

        carSlots.Clear();
        foreach (CarSlot carSlot in list)
        {
            Destroy(carSlot.gameObject);
        }
    }

    public void SwitchMode()
    {
        carSelectionMode = !carSelectionMode;

        if (carSelectionMode)
        {
            lobby.SetDelegate(lobby.LeaveLobby);
            mapPanel.SetActive(false);
            carPanel.SetActive(true);
        }
        else
        {
            currentId = 0;
            currentSelectedId = 0;
            lobby.SetDelegate(SwitchMode);
            mapPanel.SetActive(true);
            carPanel.SetActive(false);
            buttons[0].gameObject.SetActive(false);
            buttons[1].gameObject.SetActive(false);
        }
    }

    private void SetColors()
    {
        colors = colorsPanel.GetComponentsInChildren<Button>();

        for (int i = 0; i < CarColors.Colors.Length; i++)
        {
            colors[i].image.color = CarColors.Colors[i];
            int index = i;
            colors[i].onClick.AddListener(delegate { OnColorClick(index); });
        }
    }

    private void CheckEnabledColors(List<int> chosenColors)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < chosenColors.Count; i++)
        {
            colors[chosenColors[i]].gameObject.SetActive(false);
        }
    }

    public void OnColorClick(int index)
    {
        for (int i = 0; i < carSlots.Count; i++)
        {
            carSlots[i].SetColor(index);
        }

        color_id = index;
    }

    private void LoadSavedIndex()
    {
        car_id = Start_HeroID;
        currentId = car_id;
        currentSelectedId = car_id;

        if (PlayerPrefs.HasKey("car_id"))
        {
            car_id = PlayerPrefs.GetInt("car_id");
        }

        if (car_id < 0 || car_id > cars.Count)
            car_id = 0;

        if (Purse.Purchased(cars[car_id]) == false)
            car_id = 0;
    }

    protected virtual void InitGrid(int max)
    {
        Max_slots = max;

        float rate = (float)Screen.width / (float)Screen.height;

        if (rate.AlmostEquals(1.33f, 0.02f))
        {
            grid.gameObject.GetComponent<HorizontalLayoutGroup>().spacing = 100;
        }
        else if (rate.AlmostEquals(2.05f, 0.02f))
        {
            grid.gameObject.GetComponent<HorizontalLayoutGroup>().spacing = 300;
        }
        else if (rate.AlmostEquals(1.6f, 0.02f))
        {
            grid.gameObject.GetComponent<HorizontalLayoutGroup>().spacing = 200;
        }

        scroll.normalizedPosition = Vector2.zero;
        startPos = grid.anchoredPosition.x;

        shift = 2 * startPos / (Max_slots - 1);
        div = 1 / (float)Max_slots;

        //grid.anchoredPosition = new Vector2(grid.anchoredPosition.x, 100);
    }

    private void InitButtons()
    {
        if (Purse.Purchased(cars[car_id]) == true)
        {
            buttons[0].SetActive(false);
            buttons[1].SetActive(true);
        }
        else
        {
            buttons[0].SetActive(true);
            buttons[1].SetActive(false);
        }
    }

    private void UnlockHeroes()
    {
        for (int i = 0; i < cars.Count; i++)
        {
            if (Purse.Purchased(cars[i]) == true)
            {
                BuyHero(i);
            }
        }
    }

    protected float div;
    protected bool moving = false;
    protected int targetId = 0;

    protected virtual void Update()
    {
        float pos = scroll.normalizedPosition.x;

        if (pos >= 0 && pos <= 1)
        {
            float val = pos / div;

            val = Mathf.FloorToInt(val);

            if (val >= Max_slots)
                val = Max_slots - 1;

            currentId = (int)val;
        }

        if (currentId != currentSelectedId)
        {
            OldSelected();

            currentSelectedId = currentId;

            NewSelected();
        }

        if (moving)
        {
            grid.anchoredPosition = Vector3.Lerp(grid.anchoredPosition, new Vector2(startPos - targetId * shift, grid.anchoredPosition.y), elapsed_time * 10);

            elapsed_time += Time.deltaTime;

            if (grid.anchoredPosition.AlmostEquals(new Vector2(startPos - targetId * shift, grid.anchoredPosition.y), 0.01f))
                moving = false;
        }
    }

    protected virtual void OldSelected()
    {
        if (Purse.Purchased(cars[currentSelectedId]) == true)
        {
            carSlots[currentSelectedId].UnColorCar();
        }
        else
        {
            carSlots[currentSelectedId].ColorChain();
        }
    }

    protected virtual void NewSelected()
    {
        if (Purse.Purchased(cars[currentId]) == true)
        {
            UnlockHero();

        }
        else
        {
            LockHero();
            carSlots[currentSelectedId].UnColorChain();
        }
    }

    private void LockHero()
    {
        buttons[0].SetActive(false);
        buttons[1].SetActive(true);
    }

    protected virtual void UnlockHero()
    {
        car_id = currentId;
        carSlots[currentId].ColorCar();
        buttons[0].SetActive(true);
        buttons[1].SetActive(false);
        SaveHero(car_id);
    }

    public void Onclick(int n)
    {
        elapsed_time = 0;
        moving = true;
        targetId = n;
    }

    private void SaveHero(int input)
    {
        PlayerPrefs.SetInt("car_id", input);
        PlayerPrefs.Save();
    }

    public void Pick()
    {
        Start_HeroID = car_id;
    }

    public void OnPickClick()
    {
        targetId = car_id;
        grid.anchoredPosition = new Vector2(startPos - targetId * shift, grid.anchoredPosition.y);
    }

    public void OnPickButtonClick()
    {
        if (PhotonNetwork.offlineMode == true)
        {
            SwitchMode();
        }
        else
        {
            // carPanel.SetActive(false);
            lobbyController.SetCar(cars[car_id], color_id);
        }
    }

    protected void MoveToTarget(int index)
    {
        if (index > 0)
        {
            elapsed_time = 100000;
        }

        grid.anchoredPosition = new Vector2(startPos - index * shift, grid.anchoredPosition.y);
    }

    private void Open()
    {
        MoveToTarget(car_id);

        for (int i = 0; i < carSlots.Count; i++)
        {
            if (Purse.Purchased(cars[i]) == true)
            {
                carSlots[currentSelectedId].UnColorCar();
            }

            carSlots[i].SetColor(0);
            int index = i;
            carSlots[i].GetComponent<Button>().onClick.AddListener(delegate { Onclick(index); });
        }

        carSlots[car_id].ColorCar();
        carSlots[car_id].SetColor(0);

        if (Purse.Purchased(cars[car_id]) == true)
        {
            UnlockHero();
        }
        else
        {
            LockHero();
        }
    }

    public void BuyItem()
    {
        buyController.OnBuyClick(cars[currentSelectedId], ProcessBuy);
    }

    public void ProcessBuy(int id)
    {
        UnlockHero();
        BuyHero(cars.IndexOf(id));
    }

    private void BuyHero(int index)
    {
        if (currentSelectedId == index)
            carSlots[index].ColorCar();
        else
            carSlots[index].UnColorCar();

        carSlots[index].UnlockSlot();
    }

    public int GetPickedCar()
    {
        return cars[car_id];
    }

    public int GetCar(int index)
    {
        return cars[index];
    }

    public int GetPickedColor()
    {
        return color_id;
    }

    public void SetColor(int newColor)
    {
        color_id = newColor;
    }

    public Vector2 GetStartPos()
    {
        return startPosPanel;
    }
}
