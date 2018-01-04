using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MatchController : Photon.PunBehaviour
{
    [SerializeField]
    private float m_StartDelay = 3f;           // The delay between the start of RoundStarting and RoundPlaying phases.
    private Menu menu;
    [SerializeField]
    private ScoreManager scoreManager;
    [SerializeField]
    private CarSpawner carSpawner;

    private bool m_GameIsFinished = false;
    private bool leaver = false;
    private DistanceManager distanceManager;
    private TotalScoreController totalScoreController;
    private TrashCleaner trashCleaner;
    private int aiCount = 0;
    [SerializeField]
    private AudioSource countdownSource;
    [SerializeField]
    private AudioClip[] countdown;
    private int maxID = 0;
    private int ready = 1;

    [SerializeField]
    private bool lobbyGame = false;
    private bool gameEnabled = false;

    private void Start()
    {
#if UNITY_ANDROID||UNITY_IPHONE || UNITY_IPAD
        Application.targetFrameRate = 60;
#endif
        Init();
    }

    private void Init()
    {
        distanceManager = FindObjectOfType<DistanceManager>();
        totalScoreController = FindObjectOfType<TotalScoreController>();
        trashCleaner = FindObjectOfType<TrashCleaner>();

        menu = FindObjectOfType<Menu>();

        if (lobbyGame)
        {
            if (PhotonNetwork.room != null)
            {
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.Disconnect();
            }
            StartCoroutine(WaitForRoom());
        }
        else
        {
            Load();
        }
    }

    private IEnumerator WaitForRoom()
    {
        while (PhotonNetwork.room != null)
        {
            yield return null;
        }
        Load();
    }

    private void Load()
    {
        if (PhotonNetwork.room == null)
        {
            PhotonNetwork.offlineMode = true;
            PhotonNetwork.CreateRoom(null);
            if (string.IsNullOrEmpty(PhotonNetwork.player.NickName) == true)
                PhotonNetwork.player.NickName = "Player";
        }

        if (lobbyGame == false)
        {
            Prototype.NetworkLobby.SettingsController.InitSound();
            if (Prototype.NetworkLobby.SettingsController.GetSound() == false)
                AudioListener.pause = true;

            if (PhotonNetwork.room.CustomProperties.ContainsKey("aiProps"))
                aiCount = AiPropPacker.Unpack((string)PhotonNetwork.room.CustomProperties["aiProps"]).Count;

            foreach (PhotonPlayer p in PhotonNetwork.playerList)
            {
                if (p.ID >= maxID)
                {
                    maxID = p.ID + 1;
                }
            }
        }
        else
        {
            aiCount = 3;
            maxID = 2;
        }
        FindObjectOfType<ObjectManager>().SpawnObjects();

        if (PhotonNetwork.isMasterClient || lobbyGame)
        {
            if (lobbyGame)
                ready++;

            carSpawner.Spawn();
            StartCoroutine(GameLoop(0));
        }
        else
        {
            photonView.RPC("Ready", PhotonTargets.All);
        }
    }

    [PunRPC]
    private void Ready()
    {
        ready++;
    }

    public void ReloadLobbyGame()
    {
        GameEnabled = true;

        PhotonNetwork.offlineMode = true;
        PhotonNetwork.CreateRoom(null);
        EnableCarsControl();

        StartCoroutine(GameLoop(1));
    }

    /*public void ReloadGame()
    {
        brokenCars.Clear();
        frozenBrokenCars.Clear();
        spriteCars.Clear();
        frozenCars.Clear();
        Start();
    }*/      

    // This is called from start and will run each phase of the game one after another. ONLY ON SERVER (as Start is only called on server)
    private IEnumerator GameLoop(int step)
    {
        if (step == 0)
            goto start;
        else if (step == 1)
            goto step1;
        else if (step == 2)
            goto step2;

        start:

        string plrList = "";

        while (PhotonNetwork.playerList.Length > ready)
        {
            yield return null;
        }

        if (lobbyGame)
        {
            if (Prototype.NetworkLobby.StateManager.State == 0)
                FindObjectOfType<Prototype.NetworkLobby.LobbyManager>().DisableLoadPanel();
        }

        if (PhotonNetwork.room.CustomProperties.ContainsKey("plrList"))
        {
            plrList = (string)PhotonNetwork.room.CustomProperties["plrList"];
        }
        else
        {
            plrList = carSpawner.GetPlayerList();
        }

        int maxLaps = 3;

        if ((PhotonNetwork.room.CustomProperties.ContainsKey("maxLaps") && (PhotonNetwork.lobby.Name != "Rank")))
        {
            maxLaps = (int)PhotonNetwork.room.CustomProperties["maxLaps"];
            SetCustomPropertiesRoom(0, maxLaps, "", plrList);
        }
        else
        {
            SetCustomPropertiesRoom(0, maxLaps, "", plrList);
        }

        foreach (PhotonPlayer p in PhotonNetwork.playerList)
        {
            SetCustomProperties(p, false, false);
        }

        // Start off by running the 'RoundStarting' coroutine but don't return until it's finished.
        yield return StartCoroutine(RoundStarting());

        if (!lobbyGame)
            SetCustomPropertiesRoom(1, (int)PhotonNetwork.room.CustomProperties["maxLaps"], (string)PhotonNetwork.room.CustomProperties["winnerName"], "");
        step1:
        // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
        yield return StartCoroutine(RoundPlaying());
        Room room = PhotonNetwork.room;
        if (room != null)
            if (!lobbyGame)
                SetCustomPropertiesRoom(2, (int)room.CustomProperties["maxLaps"], (string)room.CustomProperties["winnerName"], (string)room.CustomProperties["plrList"]);
            step2:
        m_GameIsFinished = true;

        //This code is not run until 'RoundEnding' has finished.  At which point, check if there is a winner of the game.
        if (m_GameIsFinished == true)
        {// If there is a game winner, wait for certain amount or all player confirmed to start a game again
            if (PhotonNetwork.connected)
                photonView.RPC("GameFinished", PhotonTargets.Others);
            if (leaver == false)
            {
                bool allAreReady = false;
                while (!allAreReady)
                {
                    yield return null;

                    allAreReady = true;
                    foreach (PhotonPlayer p in PhotonNetwork.playerList)
                    {
                        if (p != null)
                        {
                            if (p.CustomProperties["ready"] != null)
                            {
                                if ((bool)p.CustomProperties["ready"] == true)
                                {
                                    allAreReady = true;
                                }
                                else
                                {
                                    allAreReady = false;
                                    break;
                                }

                            }
                            else
                            {
                                allAreReady = true;
                            }
                        }
                        else
                        {
                            allAreReady = false;
                            break;
                        }
                    }
                }

                StartCoroutine(GameLoop(0));
            }
            else
            {
                CameraSelection c = FindObjectOfType<CameraSelection>();
                if (c != null)
                {
                    c.gameObject.SetActive(false);
                }

                DisableCarControl();
                Rpcleaver();
            }
        }
    }

    [PunRPC]
    private void GameFinished()
    {
        m_GameIsFinished = true;
    }

    private void Rpcleaver()
    {
        if (!lobbyGame)
        {
            menu.EnableMenu(true);

            GameObject g = GameObject.Find("TitleWinner");
            if (g != null)
                g.GetComponent<Text>().text = "All players left!";
        }
        /*else
        {
            if (PhotonNetwork.room == null && PhotonNetwork.offlineMode == true)
            {
                PhotonNetwork.CreateRoom(null);
            }
            EnableCarsControl();
            StartCoroutine(GameLoop(1));
        }*/
    }

    private IEnumerator RoundStarting()
    {
        //we notify all clients that the round is starting

        photonView.RPC("RpcRoundStarting", PhotonTargets.All);

        yield return StartCoroutine(ServerCountdownCoroutine());
    }

    [PunRPC]
    private void RpcRoundStarting()
    {
        m_GameIsFinished = false;
        // As soon as the round starts reset the tanks and make sure they can't move.
        DisableCarControl();
        ResetAllCars(-1);
    }

    [PunRPC]
    private void RpcClientCountDown(float count)
    {
        StartCoroutine(ClientCountdownCoroutine(count));
    }

    private void ClientCountDown(int count)
    {
        GameObject countdownText = GameObject.Find("CountdownText");

        if (countdownText != null)
            if (count > 0)
            {
                countdownText.GetComponent<Text>().text = count.ToString();
                if (AudioListener.pause == false)
                {
                    countdownSource.clip = countdown[0];
                    countdownSource.Play();
                }
            }
            else
            {
                countdownText.GetComponent<Text>().text = "";
                if (AudioListener.pause == false)
                {
                    countdownSource.clip = countdown[1];
                    countdownSource.Play();
                }
            }
    }

    private IEnumerator ServerCountdownCoroutine()
    {
        float currentTime = PhotonNetwork.ServerTimestamp;

        float endTime = currentTime;
        endTime += m_StartDelay * 1000;

        float remainingTime = endTime - currentTime;
        remainingTime /= 1000;

        int floorTime = Mathf.CeilToInt(remainingTime);

        photonView.RPC("RpcClientCountDown", PhotonTargets.Others, endTime);
        ClientCountDown(floorTime);

        while (remainingTime > 0f)
        {
            yield return null;

            remainingTime -= Time.deltaTime;
            int newFloorTime = Mathf.CeilToInt(remainingTime);

            if (remainingTime > 2.5f)
                ResetAllCars(endTime);

            if (newFloorTime != floorTime)
            {//to avoid flooding the network of message, we only send a notice to client when the number of plain seconds change.
                floorTime = newFloorTime;

                ClientCountDown(floorTime);
            }
        }
        EnableCarsControl();
    }

    private IEnumerator ClientCountdownCoroutine(float endTime)
    {
        float currentTime = PhotonNetwork.ServerTimestamp;

        float remainingTime = endTime - currentTime;
        remainingTime /= 1000;

        int floorTime = Mathf.CeilToInt(remainingTime);

        ClientCountDown(floorTime);

        while (remainingTime > 0f)
        {
            yield return null;

            remainingTime -= Time.deltaTime;
            int newFloorTime = Mathf.CeilToInt(remainingTime);

            if (remainingTime > 2f)
            {
                ResetAllCars(endTime);
            }

            if (newFloorTime != floorTime)
            {//to avoid flooding the network of message, we only send a notice to client when the number of plain seconds change.
                floorTime = newFloorTime;

                ClientCountDown(floorTime);
            }
        }
        EnableCarsControl();
    }

    private IEnumerator RoundPlaying()
    {
        // While there is all not done...
        while (!GameIsFinished())
        {
            // ... return on the next frame.
            yield return null;
        }
    }

    private bool GameIsFinished()
    {
        bool allDone = false;

        // Go through all the cars...
        if ((PhotonNetwork.playerList.Length > 1) || (PhotonNetwork.offlineMode == true))
        {
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
            {
                if ((bool)p.CustomProperties["complete"] == true)
                    allDone = true;
                else
                {
                    allDone = false;
                    break;
                }
            }
        }
        else
        {
            allDone = true;
            leaver = true;
        }

        if (allDone)
            m_GameIsFinished = true;

        return allDone;
    }

    // This function is used to turn all the cars back on and reset their positions and properties.
    private void ResetAllCars(float startTime)
    {
        List<int> plrList = new List<int>();

        trashCleaner.ClearTrack();
        string[] plrs = (PhotonNetwork.room.CustomProperties["plrList"]).ToString().Split(';');
        for (int i = 0; i < plrs.Length; i++)
        {
            int id = 0;
            int.TryParse(plrs[i], out id);
            if (id != 0)
                plrList.Add(id);
        }

        if (!lobbyGame)
        {
            distanceManager.Reset();
            distanceManager.InitCars();
        }

        scoreManager.ResetScore();
        scoreManager.MaxPlayers = PlayersCount();
        scoreManager.MaxLaps = (int)PhotonNetwork.room.CustomProperties["maxLaps"];

        CarScript[] g = GameObject.FindObjectsOfType<CarScript>();

        List<int> lostIds = new List<int>();

        for (int j = 0; j < g.Length; j++)
        {
            bool found = false;
            if (g[j].LocalId >= maxID)
            {
                for (int i = 0; i < plrList.Count; i++)
                {
                    if (g[j].AI && g[j].LocalId == plrList[i])
                    {
                        found = true;
                        break;
                    }
                }
                if (found == false)
                {
                    lostIds.Add(g[j].LocalId);
                }
            }
        }

        for (int j = 0; j < lostIds.Count; j++)
        {
            for (int i = 0; i < g.Length; i++)
            {
                if (g[i].gameObject.GetComponent<CarScript>().LocalId == lostIds[j] && g[i].AI)
                {
                    ResetCar(g[i], j, (int)startTime);
                    break;
                }
            }
        }

        int leftPlrs = 0;

        for (int j = 0; j < plrList.Count; j++)
        {
            if (plrList[j] < maxID)
            {
                bool added = false;
                for (int i = 0; i < g.Length; i++)
                {
                    if (g[i].gameObject.GetComponent<PhotonView>().ownerId == plrList[j] && !g[i].AI)
                    {
                        ResetCar(g[i], j + lostIds.Count - leftPlrs, (int)startTime);
                        added = true;
                        break;
                    }
                }
                if (added == false)
                {
                    leftPlrs++;
                }
            }
            else
            {
                for (int i = 0; i < g.Length; i++)
                {
                    if (g[i].gameObject.GetComponent<CarScript>().LocalId == plrList[j] && g[i].AI)
                    {
                        ResetCar(g[i], j + lostIds.Count - leftPlrs, (int)startTime);
                        break;
                    }
                }
            }
        }
        FindObjectOfType<Steps>().ClearTrack();
        if (menu != null)
        {
            menu.DisableMenu();
        }
    }

    private void ResetCar(CarScript car, int pos, int startTime)
    {
        car.ResetCar();
        car.GetComponent<CarSound>().EnableSound();
        car.GetComponent<CarSound>().ResetPitch();
        car.BurnoutAllowed = true;
        car.EnableRpm();
        car.gameObject.GetComponent<CarLaps>().Reset();
        car.Finished = false;

        foreach (Collider2D c in car.GetComponents<Collider2D>())
        {
            c.isTrigger = true;
        }

        car.gameObject.GetComponent<CarSkin>().DisableTransparent();

        car.gameObject.transform.position = carSpawner.GetSpawnPosition(pos);
        car.gameObject.transform.rotation = carSpawner.GetSpawnRotation(pos);

        if (car.AI)
        {
            car.gameObject.GetComponent<PathFollower>().ResetIndex();
        }

        if (startTime == -1)
            car.gameObject.GetComponent<Bonus>().ResetCar();
        else
            car.gameObject.GetComponent<CarLaps>().Time = startTime;

        car.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);

        foreach (Collider2D c in car.GetComponents<Collider2D>())
        {
            c.isTrigger = false;
        }
    }

    public static void SetCustomProperties(PhotonPlayer plr, bool complete, bool ready)
    {
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
        customProperties.Add("complete", complete);
        customProperties.Add("ready", ready);
        plr.SetCustomProperties(customProperties);
    }

    public static void SetCustomPropertiesRoom(int step, int maxlaps, string winnerName, string plrList)
    {
        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
        h.Add("step", step);
        h.Add("maxLaps", maxlaps);
        h.Add("winnerName", winnerName);
        h.Add("plrList", plrList);
        PhotonNetwork.room.SetCustomProperties(h);
    }

    public override void OnMasterClientSwitched(PhotonPlayer player)
    {
        if (PhotonNetwork.room.CustomProperties.ContainsKey("step"))
        {
            StartCoroutine(GameLoop((int)PhotonNetwork.room.CustomProperties["step"]));
        }
        else
        {
            if (FindObjectsOfType<CarScript>().Length == 0)
            {
                carSpawner.Spawn();
            }
            StartCoroutine(GameLoop(0));
        }

        DeletePlayerCar();
    }

    public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        if (totalScoreController != null)
            totalScoreController.UpdateTotalScore();
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer disPlayer)
    {
        if (lobbyGame == false)
            distanceManager.RemoveCar(disPlayer.ID);
        CameraSelection c = GameObject.FindObjectOfType<CameraSelection>();
        if (c != null)
        {
            c.UpdateList();
        }

        if (PhotonNetwork.isMasterClient)
        {
            DeletePlayerCar();
        }
    }

    private void DeletePlayerCar()
    {
        CarScript[] g = GameObject.FindObjectsOfType<CarScript>();

        for (int i = 0; i < g.Length; i++)
        {
            if (!g[i].AI)
            {
                bool match = false;
                foreach (PhotonPlayer p in PhotonNetwork.playerList)
                {
                    if (g[i].gameObject.GetPhotonView().ownerId == p.ID)
                    {
                        match = true;
                        break;
                    }
                }

                if (match == false)
                    PhotonNetwork.Destroy(g[i].gameObject);
            }
        }
        StartCoroutine(UpdateTotalScoreCycle());
    }

    private IEnumerator UpdateTotalScoreCycle()
    {
        yield return new WaitForSeconds(0.3f);
        photonView.RPC("UpdateTotalScore", PhotonTargets.All);
    }

    [PunRPC]
    private void UpdateTotalScore()
    {
        totalScoreController.UpdateTotalScore();
    }

    private void EnableCarsControl()
    {
        if (lobbyGame == false)
            distanceManager.StartTimer();
        CarScript[] g = GameObject.FindObjectsOfType<CarScript>();
        for (int i = 0; i < g.Length; i++)
        {
            g[i].Control = true;
        }
    }

    private void DisableCarControl()
    {
        if (!lobbyGame)
            distanceManager.StopTimer(0);
        CarScript[] g = GameObject.FindObjectsOfType<CarScript>();
        for (int i = 0; i < g.Length; i++)
        {
            g[i].Control = false;
        }
    }

    public bool CheckWinner(CarLaps car)
    {
        if (!lobbyGame)
        {
            if (car.Laps == (int)PhotonNetwork.room.CustomProperties["maxLaps"])
            {
                if ((string)PhotonNetwork.room.CustomProperties["winnerName"] == "")
                {
                    SetCustomPropertiesRoom((int)PhotonNetwork.room.CustomProperties["step"], (int)PhotonNetwork.room.CustomProperties["maxLaps"], car.gameObject.GetComponent<CarScript>().NickName, (string)PhotonNetwork.room.CustomProperties["plrList"]);
                }
                return true;
            }
            else
                return false;
        }
        else
        {
            return false;
        }
    }

    public static string GetWinner()
    {
        return (string)PhotonNetwork.room.CustomProperties["winnerName"];
    }

    public int PlayersCount()
    {
        return PhotonNetwork.playerList.Length + aiCount;
    }

    public bool Leaver
    {
        get
        {
            return leaver;
        }
    }

    public bool Finished
    {
        get
        {
            return m_GameIsFinished;
        }
    }

    public void EnableMenu()
    {
        menu.EnableMenu(true);
    }

    public int MaxId
    {
        get
        {
            return maxID;
        }
    }

    public bool LobbyGame
    {
        get

        {
            return lobbyGame;
        }
    }

    public bool GameEnabled
    {
        get
        {
            return gameEnabled;
        }
        set
        {
            gameEnabled = value;
        }
    }

    public int AiCount
    {
        get
        {
            return aiCount;
        }
    }
}
