using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TotalScoreController : Photon.PunBehaviour
{
    private Dictionary<int, int> winnerList = new Dictionary<int, int>();
    [SerializeField]
    private GameObject playerTotalScoreEntryPrefab;
    [SerializeField]
    private GameObject playerTotalScoreList;
    private string winnerName = "";
    private Dictionary<int, int> detectList = new Dictionary<int, int>();
    private bool updating = false;
    [SerializeField]
    private GameObject match;
    [SerializeField]
    private GameObject cameraButton;

    private void CreateTotalScore(int index, string Input, string lap, string time, bool toggle)
    {
        if (match.activeSelf)
        {
            PlayerScoreSlot slot = Instantiate(playerTotalScoreEntryPrefab, playerTotalScoreList.transform).GetComponent<PlayerScoreSlot>();

            slot.SetPlayer(Input, time, toggle, !PhotonNetwork.offlineMode);
        }
    }

    public void UpdateTotalScore()
    {
        if (match != null)
            if (match.activeSelf && !updating)
            {
                updating = true;
                ClearList();

                if (FindObjectOfType<MatchController>().Leaver == false)
                {
                    if (GameObject.Find("TitleWinner") != null)
                    {
                        winnerName = (string)PhotonNetwork.room.CustomProperties["winnerName"];
                        GameObject.Find("TitleWinner").GetComponent<Text>().text = winnerName + " is winner!";
                    }
                }

                winnerList = new Dictionary<int, int>();
                detectList = new Dictionary<int, int>();
                CarLaps[] carList = GameObject.FindObjectsOfType<CarLaps>();
                int maxLaps = 3;
                if (PhotonNetwork.room.CustomProperties.ContainsKey("maxLaps"))
                    maxLaps = (int)PhotonNetwork.room.CustomProperties["maxLaps"];

                int maxTime = 0;
                int winners = 0;

                foreach (CarLaps car in carList)
                {
                    int viewID = car.gameObject.GetPhotonView().viewID;
                    if (car.Laps == maxLaps)
                    {
                        if (maxTime < car.TotalTime)
                            maxTime = car.TotalTime;

                        if (car.TotalTime > 0)
                            winnerList.Add(viewID, car.TotalTime);
                        else
                            winnerList.Add(viewID, 9999990);
                        winners++;
                    }
                    else
                    {
                        detectList.Add(viewID, car.Laps);
                    }
                }

                if (winners == carList.Length)
                {
                    cameraButton.SetActive(false);
                }
                else
                {
                    cameraButton.SetActive(true);
                }

                int j = 0;
                foreach (KeyValuePair<int, int> item in winnerList.OrderBy(key => key.Value))
                {
                    j++;
                    int lapValue = 0;
                    bool toggleValue = false;

                    CarLaps car = PhotonView.Find(item.Key).GetComponent<CarLaps>();

                    if (!car.gameObject.GetComponent<CarScript>().AI)
                    {
                        PhotonPlayer plr = PhotonPlayer.Find(car.gameObject.GetPhotonView().ownerId);
                        if (plr != null)
                            toggleValue = (bool)plr.CustomProperties["ready"];
                    }
                    else
                    {
                        toggleValue = true;
                    }

                    lapValue = car.BestLap;

                    string nickName = car.gameObject.GetComponent<CarScript>().NickName;

                    if (item.Value == 9999990)
                        CreateTotalScore(j, nickName, TimeLib.GetTime(lapValue), "0:00.000", toggleValue);
                    else
                        CreateTotalScore(j, nickName, TimeLib.GetTime(lapValue), TimeLib.GetTime(item.Value), toggleValue);
                }

                if (detectList.Count > 0)
                    foreach (KeyValuePair<int, int> item in detectList.OrderByDescending(key => key.Value))
                    {
                        j++;
                        int lapValue = 0;
                        bool toggleValue = false;
                        foreach (CarLaps car in carList)
                        {
                            int viewID = car.gameObject.GetPhotonView().viewID;
                            if (viewID == item.Key)
                            {
                                if (car.gameObject.GetComponent<CarScript>().AI)
                                    toggleValue = true;

                                lapValue = car.BestLap;
                                int totalTime = car.TotalTime;

                                if (lapValue > 999999)
                                    lapValue = 0;
                                string nickName = car.gameObject.GetComponent<CarScript>().NickName;

                                CreateTotalScore(j, nickName, TimeLib.GetTime(lapValue), TimeLib.GetTime(totalTime), toggleValue);
                                break;
                            }
                        }
                    }
                updating = false;
            }
    }

    private void ClearList()
    {
        Transform[] list = playerTotalScoreList.GetComponentsInChildren<Transform>();

        if (list != null)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] != playerTotalScoreList.transform)
                    Destroy(list[i].gameObject);
            }
        }
    }

    public void onReadyClick()
    {
        var ready = (bool)PhotonNetwork.player.CustomProperties["ready"];

        MatchController.SetCustomProperties(PhotonNetwork.player, (bool)PhotonNetwork.player.CustomProperties["complete"], !ready);
    }
}
