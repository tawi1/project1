using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerSelect : Photon.PunBehaviour
{
    private CloudRegionCode currentRegion;
    [SerializeField]
    private Prototype.NetworkLobby.LobbyManager lobbyManager;
    [SerializeField]
    private Transform grid;
    [SerializeField]
    private GameObject serverPrefab;
    [SerializeField]
    private Text regionText;

    public void Init()
    {
        foreach (CloudRegionCode region in CloudRegionCode.GetValues(typeof(CloudRegionCode)))
        {
            var server = Instantiate(serverPrefab, grid);

            if (region != CloudRegionCode.none)
                server.GetComponentInChildren<Text>().text = region.ToString();
            else
                server.GetComponentInChildren<Text>().text = "Best";

            server.GetComponent<Button>().onClick.AddListener(delegate { OnRegionClick(region); });
        }
    }

    public void SetRegion(CloudRegionCode newRegion)
    {
        currentRegion = newRegion;
        regionText.text = "Server: " + newRegion.ToString();
    }

    public void OnServerClick()
    {
        grid.gameObject.SetActive(!grid.gameObject.activeSelf);
    }

    public void OnRegionClick(CloudRegionCode region)
    {
        OnServerClick();
        if (region != currentRegion)
        {
            SetRegion(region);
            PhotonNetwork.OverrideBestCloudServer(region);
            lobbyManager.ChangeRegion = true;
            lobbyManager.ChangeRegionPanel();
            PhotonNetwork.Disconnect();
        }
    }
}
