using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class RoomManager : Photon.PunBehaviour
{
    [SerializeField]
    private Car_select_controller carSelect;
    [SerializeField]
    private Map_Select_Controller mapSelect;
    [SerializeField]
    private Prototype.NetworkLobby.LobbyManager lobbyManager;
    [SerializeField]
    private LobbyController lobbyController;
    private bool createClicked = false;
    private bool party = false;

    public void CreateSingleGame()
    {
        if (PhotonNetwork.room != null)
            PhotonNetwork.LeaveRoom();

        PhotonNetwork.offlineMode = true;

        createClicked = true;
        PhotonNetwork.CreateRoom("");
    }

    public void JoinRandomRoom()
    {
        if (PhotonNetwork.room != null)
            PhotonNetwork.LeaveRoom();
        PhotonNetwork.offlineMode = false;
        PhotonNetwork.JoinRandomRoom();

        party = false;
        lobbyManager.DisplayIsConnecting();
    }

    public override void OnPhotonRandomJoinFailed(object[] cause)
    {
        //  if (lobbyManager.CancelClicked == false)        
        CreateRandomRoom();
    }

    public void CreateRandomRoom()
    {
        CreateRoom(PhotonNetwork.player.NickName);
    }

    private void CreateRoom(string roomName)
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        party = false;
        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
    }

    public void CreateFriendRoom()
    {
        RoomOptions options = new RoomOptions();

        options.MaxPlayers = 4;
        options.IsVisible = false;
        party = true;

        PhotonNetwork.CreateRoom(PhotonNetwork.playerName + "PARTY", options, TypedLobby.Default);
    }

    public void JoinFriendRoom(string name)
    {
        PhotonNetwork.JoinRoom(name);
    }

    public override void OnPhotonCreateRoomFailed(object[] cause)
    {
        if (cause[0].ToString() == "32766")
        {
            string roomName = PhotonNetwork.player.NickName;

            roomName = RoomName.Rename(roomName);
            roomName = RoomName.GetServerName(roomName);

            CreateRoom(roomName);
        }
        else
        {
            lobbyManager.DisableInfoPanel();
            Debug.LogError(cause[1]);
        }
    }

    public override void OnCreatedRoom()
    {
        if (createClicked || PhotonNetwork.offlineMode == false)
            if (PhotonNetwork.offlineMode == true)
            {
                SetCustomProperties(PhotonNetwork.player, PhotonNetwork.player.NickName, carSelect.GetPickedCar(), carSelect.GetPickedColor(), true);

                List<int> colors = new List<int>();
                colors.Add(carSelect.GetPickedColor());

                string props = AiPropPacker.Pack(3, mapSelect.AiLevel, colors);

                SetCustomPropertiesRoom(mapSelect.Map, mapSelect.Laps, props);
                CallRoadRace(TextureLoader.GetMapName(mapSelect.Map));
                lobbyManager.SetDelegate(lobbyManager.LeaveLobby);
            }
            else
            {
                lobbyController.SetupPlayer();
                lobbyController.LoadProperties();
                lobbyManager.SetDelegate(lobbyManager.LeaveRoom);
                lobbyManager.DisableInfoPanel();
            }
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.offlineMode == false)
        {
            if (PhotonNetwork.isMasterClient == false)
            {
                lobbyController.SetupPlayer();
                //lobbyManager.SetDelegate(lobbyManager.LeaveRoom);
                lobbyManager.DisableInfoPanel();
            }
        }
    }

    private IEnumerator WaitForStart()
    {
        yield return new WaitForSeconds(1f);
        CallRoadRace(TextureLoader.GetMapName(mapSelect.Map));
    }

    public void CallRoadRace(string level)
    {
        PhotonNetwork.room.IsOpen = false;
        PhotonNetwork.room.IsVisible = false;
        LoadRace(level);
    }


    void LoadRace(string level)
    {
        lobbyManager.EnableLoadPanel();
        PhotonNetwork.LoadLevel(level);
    }

    public static void SetCustomProperties(PhotonPlayer plr, string nickName, int car, int playerColor, bool ready)
    {
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
        customProperties.Add("nickName", nickName);
        customProperties.Add("car", car);
        customProperties.Add("playerColor", playerColor);
        customProperties.Add("ready", ready);
        plr.SetCustomProperties(customProperties);
    }

    public static void SetCustomPropertiesRoom(int map, int laps, string aiProps)
    {
        ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable();
        h.Add("map", map);
        h.Add("maxLaps", laps);
        h.Add("aiProps", aiProps);

        PhotonNetwork.room.SetCustomProperties(h);
    }
}
