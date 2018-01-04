using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Map_Select_Controller : Car_select_controller
{
    private List<MapSlot> mapSlots = new List<MapSlot>();
    [SerializeField]
    private RoomManager roomManager;
    [SerializeField]
    private GameObject MapPanel;

    [SerializeField]
    private GameObject mapSlotPrefab;
    private int map_id = 0;
    [SerializeField]
    private Slider mapSlider;
    [SerializeField]
    private GameObject difficultSlider;
    [SerializeField]
    private Text lapsValue;
    private int lapCount = 1;
    private int aiLevel = 2;
    private const string lapsText = "Laps: ";
    private static string[] difficultNames = { "Hard", "Normal", "Easy" };
    [SerializeField]
    private Text difficultText;
    [SerializeField]
    private Text startButtonText;

    public override void Init()
    {
        MapPanel.SetActive(true);
        FillGrid();

        LoadSavedIndex();
        InitGrid(mapSlots.Count);
        scroll.normalizedPosition = Vector2.zero;
        SetMaps();
        LoadSavedLap();
        startPosPanel = MapPanel.GetComponent<RectTransform>().localPosition;
        MapPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (mapSlots.Count > 0)
        {
            MoveToTarget(map_id);
            MapPanel.GetComponent<RectTransform>().localPosition = startPosPanel;
        }
        CheckText();
    }

    public override void OnEnablePanel()
    {
        CheckText();
        MapPanel.SetActive(true);
    }

    private void CheckText()
    {
        if (PhotonNetwork.offlineMode == true)
        {
            startButtonText.text = "Start game";
            difficultSlider.SetActive(true);
        }
        else
        {
            startButtonText.text = "Pick";
            difficultSlider.SetActive(false);
            // MapPanel.SetActive(true);
        }
    }

    protected override void Update()
    {
        if (startPos.AlmostEquals(0, 0.01f) == false)
        {
            base.Update();
        }
        else
        {
            InitGrid(mapSlots.Count);
            OnEnable();
        }
    }

    protected override void OldSelected()
    {
        mapSlots[currentSelectedId].UnColorMap();
    }

    protected override void NewSelected()
    {
        UnlockHero();
    }

    private void FillGrid()
    {
        ClearGrid();

        List<Texture2D> maps = TextureLoader.GetMaps();

        foreach (var map in maps)
        {
            var mapSlot = Instantiate(mapSlotPrefab, grid).GetComponent<MapSlot>();

            mapSlots.Add(mapSlot);
            mapSlot.SetMap(map.name, map);
            mapSlot.transform.SetSiblingIndex(mapSlot.transform.GetSiblingIndex() - 1);
        }
    }

    private void ClearGrid()
    {
        MapSlot[] listMaps = grid.GetComponentsInChildren<MapSlot>();

        mapSlots.Clear();
        foreach (MapSlot mapSlot in listMaps)
        {
            Destroy(mapSlot.gameObject);
        }
    }

    protected override void InitGrid(int max)
    {
        base.InitGrid(max);
    }

    private void SetMaps()
    {
        for (int i = 0; i < mapSlots.Count; i++)
        {
            int index = i;
            if (i > 0)
                mapSlots[i].UnColorMap();
            mapSlots[i].GetComponent<Button>().onClick.AddListener(delegate { Onclick(index); });
        }
    }

    protected override void UnlockHero()
    {
        map_id = currentId;
        mapSlots[currentId].ColorMap();
        SaveMap(map_id);
    }

    public void OnStartGameClick()
    {
        if (PhotonNetwork.offlineMode == true)
        {
            roomManager.CreateSingleGame();
        }
        else
        {
            //MapPanel.SetActive(false);
            lobbyController.SetMap(map_id, Laps);
        }
    }

    private void LoadSavedLap()
    {
        lapCount = LoadSavedLapsCount();
        mapSlider.value = lapCount;
        lapsValue.text = lapsText + lapCount.ToString();
    }

    public void OnMapCountChanged()
    {
        lapCount = (int)mapSlider.value;
        SaveCountLaps(lapCount);
        lapsValue.text = lapsText + lapCount.ToString();
    }

    private int LoadSavedLapsCount()
    {
        int lapCount = 1;

        if (PlayerPrefs.HasKey("lapCount"))
        {
            lapCount = PlayerPrefs.GetInt("lapCount");
        }

        if (lapCount < 0 || lapCount > 10)
        {
            lapCount = 1;
        }

        return lapCount;
    }

    private void SaveCountLaps(int input)
    {
        PlayerPrefs.SetInt("lapCount", input);
        PlayerPrefs.Save();
    }

    private void LoadSavedIndex()
    {
        map_id = 0;
        currentId = map_id;
        currentSelectedId = map_id;

        if (PlayerPrefs.HasKey("map_id"))
        {
            map_id = PlayerPrefs.GetInt("map_id");
        }
    }

    private void SaveMap(int input)
    {
        PlayerPrefs.SetInt("map_id", input);
        PlayerPrefs.Save();
    }

    public void OnDiffucultChanged(Slider slider)
    {
        int val = (int)slider.value;
        difficultText.text = GetAiLevelSliderName(val);
        aiLevel = val;
    }

    public static string GetAiLevelSliderName(int val)
    {
        if (val == 0)
        {
            val = 2;
        }
        else if (val == 2)
        {
            val = 0;
        }

        return difficultNames[val];
    }

    public static string GetAiLevelName(int val)
    {  
        return difficultNames[val];
    }

    public int Laps
    {
        get
        {
            return lapCount;
        }
    }

    public int AiLevel
    {
        get
        {
            return aiLevel;
        }
    }

    public int Map
    {
        get
        {
            return map_id;
        }
    }
}
