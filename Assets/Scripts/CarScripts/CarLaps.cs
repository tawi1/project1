using UnityEngine;
using System.Collections;
using System;

public class CarLaps : Photon.PunBehaviour
{
    private int time = 0;
    private int bestLap = 9999999;
    private int currLap = 0;
    private int currTime = 0;
    private int laps = 0;
    private int totalTime = 0;
    private MatchController matchController;
    private TotalScoreController totalScoreController;
    private DistanceManager distanceManager;
    private ScoreManager scoreManager;
    private Prototype.NetworkLobby.AdsController adsController;
    private CarScript carscript;
    private CarCheckpoint carCheckpont;

    private int maxLaps = -1;

    void Awake()
    {
        carscript = GetComponent<CarScript>();
        carCheckpont = GetComponent<CarCheckpoint>();
        distanceManager = FindObjectOfType<DistanceManager>();
        matchController = FindObjectOfType<MatchController>();
        totalScoreController = FindObjectOfType<TotalScoreController>();
        scoreManager = FindObjectOfType<ScoreManager>();
        adsController = FindObjectOfType<Prototype.NetworkLobby.AdsController>();

        if (PhotonNetwork.room != null)
            if (PhotonNetwork.room.CustomProperties.ContainsKey("maxLaps"))
                maxLaps = (int)PhotonNetwork.room.CustomProperties["maxLaps"];
    }

    public void LapsCounter()
    {
        if (!photonView.isMine) return;

        if (laps < maxLaps)
        {
            laps++;

            currTime = PhotonNetwork.ServerTimestamp;
            currLap = currTime - time;
            time = currTime;

            if (currLap < bestLap)
            {
                bestLap = currLap;
            }

            totalTime += currLap;

            if (!carscript.AI)
                if (!matchController.LobbyGame)
                    distanceManager.UpdateLap(laps);
            StartCoroutine(UpdateLapCycle(photonView.viewID, bestLap, laps, totalTime));

            bool winnerCar = matchController.CheckWinner(this);

            if (maxLaps - laps == 1)
            {
                scoreManager.MaxPlayers = matchController.PlayersCount();
            }

            if (winnerCar)
            {
                string plrList = (string)PhotonNetwork.room.CustomProperties["plrList"];

                string[] prop = plrList.Split(';');
                int place = 1;
                if (plrList != "")
                    place = prop.Length + 1;

                if (!matchController.LobbyGame)
                    StartCoroutine(SetWinnerCycle(photonView.viewID, place));

                if (!carscript.AI)
                {
                    scoreManager.Place = place;

                    scoreManager.SolvFinish();

                    MatchController.SetCustomProperties(PhotonNetwork.player, true, false);
                    if (plrList != "")
                        plrList = gameObject.GetPhotonView().ownerId.ToString() + ";" + plrList;
                    else
                        plrList = gameObject.GetPhotonView().ownerId.ToString();

                }
                else
                {
                    if (plrList != "")
                        plrList = carscript.LocalId.ToString() + ";" + plrList;
                    else
                        plrList = carscript.LocalId.ToString();
                }

                MatchController.SetCustomPropertiesRoom((int)PhotonNetwork.room.CustomProperties["step"], (int)PhotonNetwork.room.CustomProperties["maxLaps"], (string)PhotonNetwork.room.CustomProperties["winnerName"], plrList);

                photonView.RPC("DisableCar", PhotonTargets.All, photonView.viewID);

                if (!carscript.AI)
                {
                    ShowWinPanel();
                }
            }
        }
    }

    private IEnumerator UpdateLapCycle(int viewId, int bestLap, int lap, int totalTime)
    {
        yield return new WaitForSeconds(0.15f);
        photonView.RPC("UpdateLap", PhotonTargets.All, viewId, bestLap, lap, totalTime);
    }

    [PunRPC]
    void UpdateLap(int idCar, int bestLap, int lap, int timeValue)
    {
        CarLaps car = PhotonView.Find(idCar).gameObject.GetComponent<CarLaps>();

        car.BestLap = bestLap;
        car.Laps = lap;
        car.TotalTime = timeValue;
        totalScoreController.UpdateTotalScore();
    }

    private IEnumerator SetWinnerCycle(int viewId, int place)
    {
        yield return new WaitForSeconds(0.2f);
        photonView.RPC("SetWinner", PhotonTargets.All, viewId, place);
    }

    [PunRPC]
    void SetWinner(int idCar, int place)
    {
        distanceManager.SetWinner(idCar, place);
    }

    private void ShowWinPanel()
    {
        carscript.ResetCar();
        carscript.DisableButtons();
        matchController.EnableMenu();
        adsController.AdsLaunch();
        totalScoreController.UpdateTotalScore();
        if (!matchController.LobbyGame)
            distanceManager.StopTimer(totalTime);
    }

    [PunRPC]
    private void DisableCar(int id)
    {
        PhotonView view = PhotonView.Find(id);

        if (view != null)
        {
            CarScript car = view.GetComponent<CarScript>();

            car.Finished = true;
            car.gameObject.GetComponent<Bonus>().ResetCar();
            car.gameObject.GetComponent<CarSkin>().EnableTransparent();
            car.GetComponent<StepsController>().DisableSmoke();
            car.DisableNitro();

            car.Control = false;
            car.BurnoutAllowed = false;
            car.DisableRpm();
            car.GetComponent<CarSound>().DisableSound();

            CameraSelection c = GameObject.FindObjectOfType<CameraSelection>();

            if (c != null)
            {
                c.UpdateList();
            }
        }
    }

    public int Time
    {
        get
        {
            return time;
        }
        set
        {
            time = value;
        }
    }

    public int TotalTime
    {
        get
        {
            return totalTime;
        }
        set
        {
            totalTime = value;
        }
    }

    public int Laps
    {
        get
        {
            return laps;
        }
        set
        {
            laps = value;
        }
    }

    public int BestLap
    {
        get
        {
            return bestLap;
        }
        set
        {
            bestLap = value;
        }
    }

    public int MaxLaps
    {
        get
        {
            return maxLaps;
        }
    }

    public void Reset()
    {
        laps = 0;
        totalTime = 0;
        bestLap = 9999999;
    }

    public int ViewID
    {
        get
        {
            return photonView.viewID;
        }
    }

    public CarCheckpoint GetCarCheckpoint()
    {
        return carCheckpont;
    }
}

