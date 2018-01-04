using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraSelection : Photon.PunBehaviour
{
    private int idSelected = 0;
    private int idCar = 0;
    private List<int> plrList;
    [SerializeField]
    private CameraController cam;
    [SerializeField]
    private Button Left;
    [SerializeField]
    private Button Right;
    [SerializeField]
    private Button Menu;
    [SerializeField]
    private Menu gameMenu;
    private int maxLaps = 3;
    private CarLaps[] carList;

    private void Start()
    {
        maxLaps = (int)PhotonNetwork.room.CustomProperties["maxLaps"];
    }

    private void OnEnable()
    {
        carList = GameObject.FindObjectsOfType<CarLaps>();
        UpdateList();
    }

    public void OnRightClick()
    {
        if (plrList.Count > 0)
        {
            if (idSelected + 1 < plrList.Count)
            {
                idSelected++;
            }
            else
            {
                idSelected = 0;
            }
            idCar = plrList[idSelected];
            SetTarget(plrList[idSelected]);
        }
    }

    public void OnLeftClick()
    {
        if (plrList.Count > 0)
        {
            if (idSelected - 1 > -1)
            {
                idSelected--;
            }
            else
            {
                idSelected = plrList.Count - 1;
            }
            idCar = plrList[idSelected];
            SetTarget(plrList[idSelected]);
        }
    }

    void SetTarget(int id)
    {
        for (int i = 0; i < carList.Length; i++)
        {
            if (carList[i] != null)
                if (carList[i].gameObject.GetComponent<PhotonView>().viewID == id)
                {
                    cam.setTarget(carList[i].gameObject.transform);
                    break;
                }
        }
    }

    void SetPlayer(int id)
    {
        for (int i = 0; i < carList.Length; i++)
        {
            if (carList[i] != null)
                if (carList[i].gameObject.GetComponent<PhotonView>().ownerId == id && !carList[i].GetComponent<CarScript>().AI)
                {
                    cam.setTarget(carList[i].gameObject.transform);
                    break;
                }
        }
    }

    public void UpdateList()
    {
        plrList = new List<int>();

        for (int i = 0; i < carList.Length; i++)
        {
            if (carList[i] != null)
            {
                if (carList[i].Laps != maxLaps)
                {
                    plrList.Add(carList[i].gameObject.GetPhotonView().viewID);
                }
            }
        }

        if (plrList.Count == 0)
        {
            OnBackMenuClick();
        }
        else
        {
            plrList.Sort();

            if (idSelected >= plrList.Count)
            {
                SetTarget(plrList[0]);
            }
            else
            if (idCar != plrList[idSelected])
                SetTarget(plrList[0]);
            else
                SetTarget(plrList[idSelected]);
        }
    }

    public void OnBackMenuClick()
    {
        SetPlayer(PhotonNetwork.player.ID);
       // gameMenu.EnableMatchMenu();
        gameObject.SetActive(false);
    }
}
