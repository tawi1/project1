using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class LobbyController : Photon.PunBehaviour
{
    [SerializeField]
    private Prototype.NetworkLobby.LobbyManager lobbyManager;
    [SerializeField]
    private Car_select_controller carsController;
    [SerializeField]
    private Map_Select_Controller mapsController;
    [SerializeField]
    private GameObject carsPanel;
    [SerializeField]
    private GameObject mapsPanel;
    [SerializeField]
    private GameObject lobbyPanel;
    [SerializeField]
    private Text lapsValue;
    [SerializeField]
    private Text mapValue;
    [SerializeField]
    private Button mapButton;
    [SerializeField]
    private GameObject grid;
    [SerializeField]
    private Text startText;
    [SerializeField]
    private RectMover rectMover;
    [SerializeField]
    private InputField input;
    [SerializeField]
    private GameObject searchButton;

    private string aiPropsCurrent;
    private CarLobbySlot[] carSlots;
    private int currentMap;
    private bool start = false;
    private Vector2 startPos;

    public void Init()
    {
        carSlots = grid.GetComponentsInChildren<CarLobbySlot>(true);
        startPos = lobbyPanel.GetComponent<RectTransform>().localPosition;
    }

    public void LoadProperties()
    {
        if (PhotonNetwork.player.IsMasterClient)
        {
            currentMap = mapsController.Map;
            int laps = mapsController.Laps;

            CheckSearchButton();
            RoomManager.SetCustomPropertiesRoom(currentMap, laps, "");
            if (PhotonNetwork.offlineMode == false)
                StartCoroutine(MatchStarter());
        }
    }

    private void CheckSearchButton()
    {
        if (PhotonNetwork.room.IsVisible == false)
        {
            searchButton.SetActive(true);
        }
    }

    public void SetupPlayer()
    {
        if (!PhotonNetwork.player.IsLocal)
            return;

        int car = 0;
        if (PlayerPrefs.HasKey("car"))
        {
            car = PlayerPrefs.GetInt("car");
        }

        bool purchased = Purse.Purchased(carsController.GetCar(car));

        if (purchased == false)
            car = 0;

        int color = CarColors.CheckColor(PhotonNetwork.player.ID, -1);

        carsController.SetColor(color);
        lobbyManager.OpenCarSelectionPanel();
        lobbyPanel.SetActive(true);
        EnableCarSelection();
        RectMover.MoveTarget(mapsPanel.GetComponent<RectTransform>(), new Vector2(mapsController.GetStartPos().x, mapsController.GetStartPos().y - Screen.width * 1.5f));
        RectMover.MoveTarget(lobbyPanel.GetComponent<RectTransform>(), new Vector2(startPos.x, startPos.y - Screen.width * 1.5f));

        RoomManager.SetCustomProperties(PhotonNetwork.player, PhotonNetwork.player.NickName, carsController.GetCar(car), color, false);
        if (!PhotonNetwork.isMasterClient)
            UpdateSettings();
    }

    public override void OnMasterClientSwitched(PhotonPlayer player)
    {
        StartCoroutine(MatchStarter());
        CheckSearchButton();
    }

    public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        if (lobbyPanel.activeSelf)
            UpdateLobby();
    }

    public override void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (lobbyPanel.activeSelf)
        {
            UpdateSettings();
            if (propertiesThatChanged["aiProps"] != null)
                if (aiPropsCurrent != (string)propertiesThatChanged["aiProps"])
                {
                    aiPropsCurrent = (string)propertiesThatChanged["aiProps"];
                    UpdateLobby();
                }
        }
    }

    public void OnRoomPropertiesChanged()
    {
        UpdateSettings();
    }

    void UpdateSettings()
    {
        if (PhotonNetwork.room.CustomProperties.ContainsKey("maxLaps"))
        {
            lapsValue.text = PhotonNetwork.room.CustomProperties["maxLaps"].ToString();
            currentMap = (int)PhotonNetwork.room.CustomProperties["map"];
            SetMap();
        }
    }

    private void SetMap()
    {
        mapButton.image.overrideSprite = TextureLoader.GetMapSprite(currentMap);
        mapValue.text = TextureLoader.GetMapName(currentMap);
    }

    void UpdateLobby()
    {
        ClearLobby();

        bool playersAdded = AddPlayerList();

        if (playersAdded)
        {
            AddAiList();
        }
    }

    private void FixedUpdate()
    {
        CheckStartButtonText();
    }

    private void CheckStartButtonText()
    {
        if (PhotonNetwork.isMasterClient)
        {
            if (start)
            {
                startText.text = "Start game";
            }
            else
            {
                startText.text = "Waiting for players";
            }
        }
        else
        {
            startText.text = "Waiting for Host";
        }
    }

    private bool AddPlayerList()
    {
        bool success = true;
        List<int> plrList = new List<int>();

        foreach (PhotonPlayer p in PhotonNetwork.playerList)
        {
            plrList.Add(p.ID);
        }
        plrList.Sort();

        int i = 0;

        foreach (int item in plrList)
        {
            PhotonPlayer p = PhotonPlayer.Find(item);

            if (p.CustomProperties.ContainsKey("nickName"))
            {
                int curColor = -1;

                if (p.CustomProperties["playerColor"] != null)
                    curColor = (int)p.CustomProperties["playerColor"];

                int color = CarColors.CheckColor(p.ID, curColor);
                if (curColor != color)
                {
                    RoomManager.SetCustomProperties(p, (string)p.CustomProperties["nickName"], (int)p.CustomProperties["car"], color, (bool)p.CustomProperties["ready"]);
                    success = false;
                    break;
                }
                else
                {
                    AddPlayer(i, p.NickName, (int)p.CustomProperties["playerColor"], (int)p.CustomProperties["car"], PhotonNetwork.isMasterClient, p.IsMasterClient, p.IsLocal, (bool)p.CustomProperties["ready"], false, true, 0);
                    i++;
                }

            }
        }
        return success;
    }

    private void AddAiList()
    {
        string aiProps = (string)PhotonNetwork.room.CustomProperties["aiProps"];

        var props = AiPropPacker.Unpack(aiProps);
        int count = props.Count;

        if (PhotonNetwork.playerList.Length + count <= PhotonNetwork.room.MaxPlayers || PhotonNetwork.room.MaxPlayers == 0)
        {
            List<int> colorInUse = CarColors.GetChosenColors();

            int index = 0;
            foreach (var prop in props)
            {
                int aiColor = 0;
                for (int j = 0; j < CarColors.Colors.Length; j++)
                {
                    if (colorInUse.Contains(j) == false)
                    {
                        aiColor = j;
                        break;
                    }
                }

                colorInUse.Add(aiColor);

                bool deleteAllowed = true;
                if (PhotonNetwork.offlineMode == true && count == 1)
                {
                    deleteAllowed = false;
                }

                AddPlayer(index, "AI" + (index + 1), aiColor, prop.idCar, PhotonNetwork.isMasterClient, false, false, true, true, deleteAllowed, prop.aiLevel);
                index++;
            }
        }
        else
        {
            if (PhotonNetwork.isMasterClient)
            {
                int deleteCount = PhotonNetwork.playerList.Length + count - PhotonNetwork.room.MaxPlayers;
                DeleteAi(-1);
            }
        }
    }

    private void AddPlayer(int index, string nickName, int color, int car, bool master, bool playerMaster, bool local, bool ready, bool aiPlayer, bool deleteAllowed, int aiLevel)
    {
        int carSlotIndex = index;

        if (aiPlayer)
            carSlotIndex += PhotonNetwork.playerList.Length;

        CarLobbySlot carSlot = carSlots[carSlotIndex];

        if (aiPlayer == false)
        {
            carSlot.SetCar(car, nickName);
        }
        else
            carSlot.SetAi(car, nickName, Map_Select_Controller.GetAiLevelName(aiLevel));
        carSlot.SetColor(color);
        carSlot.SetReady(ready, local);

        if (local)
        {
            if (aiPlayer == false)
            {
                carSlot.OnCarClick(() => { EnableCarSelection(); });
                input.interactable = !ready;
            }
            carSlot.OnReadyClick(() => { OnReadyClicked(); });
        }
        else
        {
            if (master)
            {
                /* if (deleteAllowed)
                     o.transform.Find("RemoveIcon").GetComponent<Button>().interactable = true;
                 else
                     o.transform.Find("RemoveIcon").GetComponent<Button>().interactable = false;*/

                if (deleteAllowed)
                    if (!aiPlayer)
                    {
                        carSlot.SetKick(() => { OnKickClicked(index); });
                    }
                    else
                    {
                        carSlot.OnSetAiLevelClick(() => { OnChangeAiLevelClick(index); });
                        carSlot.SetKick(() => { DeleteAi(index); });
                    }
            }
        }

        if (ready == true)
        {
            if (local)
            {
                mapButton.interactable = false;
            }
        }
        else
        {
            if (local)
            {
                mapButton.interactable = true;
            }
        }
    }

    private void EnableCarSelection()
    {
        carsController.OnEnablePanel();

        rectMover.SetTargets(lobbyPanel.GetComponent<RectTransform>(), carsPanel.GetComponent<RectTransform>(), new Vector2(startPos.x, startPos.y - Screen.width * 1.5f), new Vector2(carsController.GetStartPos().x, carsController.GetStartPos().y));
        lobbyManager.SetDelegate(CloseCarsPanel);
    }

    public void OnAddClick()
    {
        string aiProps = (string)PhotonNetwork.room.CustomProperties["aiProps"];
        int count = AiPropPacker.Unpack(aiProps).Count;
        if (count < 3)
        {
            count++;

            var colors = CarColors.GetChosenColors();

            aiProps = AiPropPacker.AddProp(1, 0, colors, aiProps);


            RoomManager.SetCustomPropertiesRoom((int)PhotonNetwork.room.CustomProperties["map"], (int)PhotonNetwork.room.CustomProperties["maxLaps"], aiProps);
        }
    }

    private void DeleteAi(int index)
    {
        string aiProps = (string)PhotonNetwork.room.CustomProperties["aiProps"];

        if (index == -1)
        {
            index = AiPropPacker.Unpack(aiProps).Count - 1;
        }

        aiProps = AiPropPacker.DeleteProp(index, aiProps);
        RoomManager.SetCustomPropertiesRoom((int)PhotonNetwork.room.CustomProperties["map"], (int)PhotonNetwork.room.CustomProperties["maxLaps"], aiProps);
    }

    private void OnChangeAiLevelClick(int id)
    {
        string aiProps = (string)PhotonNetwork.room.CustomProperties["aiProps"];

        int level = AiPropPacker.Unpack(aiProps)[id].aiLevel;

        level--;
        if (level < 0)
            level = 2;

        aiProps = AiPropPacker.ChangeProp(id, level, aiProps);

        RoomManager.SetCustomPropertiesRoom((int)PhotonNetwork.room.CustomProperties["map"], (int)PhotonNetwork.room.CustomProperties["maxLaps"], aiProps);
    }

    void ClearLobby()
    {
        for (int i = 0; i < carSlots.Length; i++)
        {
            carSlots[i].UnSetCar();
            if (PhotonNetwork.isMasterClient)
                carSlots[i].OnCarClick(() => { OnAddClick(); });
        }
    }

    void OnKickClicked(int index)
    {
        int id = 0;
        int i = 0;

        List<PhotonPlayer> list = PhotonNetwork.playerList.ToList();

        List<PhotonPlayer> sortedList = list.OrderBy(item => item.ID).ToList();

        foreach (PhotonPlayer player in sortedList)
        {
            if (index == i)
            {
                id = player.ID;
            }
            else
            {
                i++;
            }
        }

        photonView.RPC("Kicked", PhotonPlayer.Find(id));
    }

    [PunRPC]
    void Kicked()
    {
        lobbyManager.KickedMessage();
        lobbyManager.LeaveRoom();
    }

    void OnReadyClicked()
    {
        if (PhotonNetwork.offlineMode == false)
        {
            if (PhotonNetwork.player.CustomProperties["ready"] != null)
            {
                RoomManager.SetCustomProperties(PhotonNetwork.player, (string)PhotonNetwork.player.CustomProperties["nickName"], (int)PhotonNetwork.player.CustomProperties["car"], (int)PhotonNetwork.player.CustomProperties["playerColor"], !(bool)PhotonNetwork.player.CustomProperties["ready"]);
            }
        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer disPlayer)
    {
        foreach (PhotonPlayer p in PhotonNetwork.playerList)
        {
            if (p != disPlayer)
                RoomManager.SetCustomProperties(p, (string)p.CustomProperties["nickName"], (int)p.CustomProperties["car"], (int)p.CustomProperties["playerColor"], false);
        }
    }

    public void OnMapClick()
    {
        if (PhotonNetwork.isMasterClient)
        {
            mapsController.OnEnablePanel();
            lobbyManager.SetDelegate(CloseMapPanel);
            rectMover.SetTargets(lobbyPanel.GetComponent<RectTransform>(), mapsPanel.GetComponent<RectTransform>(), new Vector2(startPos.x, startPos.y - Screen.width * 1.5f), new Vector2(mapsController.GetStartPos().x, mapsController.GetStartPos().y));
        }
    }

    private IEnumerator MatchStarter()
    {
        bool allReady = false;
        while (allReady == false)
        {
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
            {
                if (p.CustomProperties["ready"] != null)
                {
                    if ((bool)p.CustomProperties["ready"] == true)
                        allReady = true;
                    else
                    {
                        allReady = false;
                        break;
                    }
                }
                else
                {
                    allReady = false;
                    break;
                }
            }

            if (allReady && PhotonNetwork.playerList.Length > 1)
                start = true;
            else
                start = false;

            if (PhotonNetwork.room != null)
                if (PhotonNetwork.playerList.Length < PhotonNetwork.room.MaxPlayers)
                    allReady = false;

            yield return null;
        }

        StartGame();
    }

    public void StartGame()
    {
        if (start && PhotonNetwork.isMasterClient)
            StartCoroutine(Starter());
    }

    private IEnumerator Starter()
    {
        CallRoadRace();
        yield return new WaitForSeconds(0.15f);
        LoadRace(TextureLoader.GetMapName(currentMap));
    }

    public void CallRoadRace()
    {
        PhotonNetwork.room.IsOpen = false;
        PhotonNetwork.room.IsVisible = false;
        photonView.RPC("LoadRace", PhotonTargets.Others, TextureLoader.GetMapName(currentMap));
    }

    [PunRPC]
    void LoadRace(string level)
    {
        lobbyManager.EnableLoadPanel();
        PhotonNetwork.LoadLevel(level);
    }

    public void OnSearchClick()
    {
        if (PhotonNetwork.room.IsVisible == false)
        {
            PhotonNetwork.room.IsVisible = true;
            searchButton.SetActive(false);
        }
    }

    public void OnNameChanged()
    {
        RoomManager.SetCustomProperties(PhotonNetwork.player, PhotonNetwork.player.NickName, (int)PhotonNetwork.player.CustomProperties["car"], (int)PhotonNetwork.player.CustomProperties["playerColor"], (bool)PhotonNetwork.player.CustomProperties["ready"]);
    }

    public void SetCar(int newCar, int newColor)
    {
        RoomManager.SetCustomProperties(PhotonNetwork.player, (string)PhotonNetwork.player.CustomProperties["nickName"], newCar, newColor, (bool)PhotonNetwork.player.CustomProperties["ready"]);
        CloseCarsPanel();
    }

    private void CloseCarsPanel()
    {
        rectMover.SetTargets(lobbyPanel.GetComponent<RectTransform>(), carsPanel.GetComponent<RectTransform>(), startPos, new Vector2(carsController.GetStartPos().x, carsController.GetStartPos().y - Screen.width * 1.5f));

        lobbyManager.SetDelegate(lobbyManager.LeaveRoom);
    }

    public void SetMap(int newMap, int maxLaps)
    {
        CloseMapPanel();
        RoomManager.SetCustomPropertiesRoom(newMap, maxLaps, (string)PhotonNetwork.room.CustomProperties["aiProps"]);
    }

    private void CloseMapPanel()
    {
        rectMover.SetTargets(lobbyPanel.GetComponent<RectTransform>(), mapsPanel.GetComponent<RectTransform>(), new Vector2(startPos.x, startPos.y), new Vector2(mapsController.GetStartPos().x, mapsController.GetStartPos().y - Screen.width * 1.5f));
        lobbyManager.SetDelegate(lobbyManager.LeaveRoom);
    }
}
