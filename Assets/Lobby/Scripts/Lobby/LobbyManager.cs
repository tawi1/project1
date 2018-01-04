using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Prototype.NetworkLobby
{
    public class LobbyManager : Photon.PunBehaviour
    {
        [Header("Unity UI Lobby")]
        [Tooltip("Time in second between all players ready & match start")]

        [Space]
        [Header("UI Reference")]
        [SerializeField]
        private RectTransform mainMenu;
        [SerializeField]
        private RectTransform singlePanel;
        [SerializeField]
        private RectTransform multiPanel;
        [SerializeField]
        private RectTransform friendsPanel;
        [SerializeField]
        private LobbyInfoPanel infoPanel;

        protected RectTransform currentPanel;

        [SerializeField]
        private InputField inputNick;
        [SerializeField]
        private GameObject LoadPanel;
        [SerializeField]
        private RoomManager roomManager;
        [SerializeField]
        private GameObject rewardButton;
        [SerializeField]
        private PurseUI topPanelUI;
        [SerializeField]
        private LobbyController lobbyController;
        [SerializeField]
        private ServerSelect serverSelect;

        private bool cancelClicked = false;
        private bool joinClicked = false;
        public const string currentVersion = "mobile_1.01";
        private bool connectClicked = false;
        private bool changeRegion = false;

        MatchController matchController;

        void Start()
        {
            PhotonNetwork.BackgroundTimeout = 120f;
            matchController = FindObjectOfType<MatchController>();

            currentPanel = mainMenu;
            backDelegate = LeaveLobby;

            PhotonNetwork.player.NickName = LoadNickName();
            inputNick.text = PhotonNetwork.player.NickName;

            if (StateManager.State >= 1)
            {
                LoadMultiplayer();
                OnMultiPlayerPanelClick();
            }
        }

        public void LoadMultiplayer()
        {
            if (matchController == null)
                matchController = FindObjectOfType<MatchController>();
            // matchController.ReloadLobbyGame();
            matchController.GameEnabled = false;

            StateManager.State = 1;
            PhotonNetwork.offlineMode = false;

            inputNick.text = PhotonNetwork.player.NickName;
            joinClicked = false;
            cancelClicked = false;
            topPanelUI.Move(0);
            mainMenu.gameObject.SetActive(false);
            topPanelUI.SetTopPanelName("MULTIPLAYER");

            if (PhotonNetwork.connected == false)
            {
                Connect();
            }
        }

        public void OnSinglePlayerClick()
        {
            rewardButton.SetActive(true);
            topPanelUI.SetStart();
            topPanelUI.SetTopPanelName("SINGLEPLAYER");
            OpenCarSelectionPanel();
        }

        public void OpenCarSelectionPanel()
        {
            ChangeTo(singlePanel);
        }

        public void OnMultiPlayerPanelClick()
        {
            ChangeTo(multiPanel);
        }

        public void OnMultiPlayerClick()
        {
            multiPanel.gameObject.SetActive(false);
            rewardButton.SetActive(false);
            roomManager.JoinRandomRoom();
        }

        public void OnFriendsClick()
        {
            ChangeTo(friendsPanel);
        }

        public void ChangeRegionPanel()
        {
            ChangeTo(mainMenu);
            mainMenu.gameObject.SetActive(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentPanel != mainMenu)
                {
                    GoBackButton();
                }
            }
        }

        public void OnEndEditNickName()
        {
            if (string.IsNullOrEmpty(inputNick.text.Trim(' ')) == true)
            {
                PhotonNetwork.player.NickName = "DefaultPlayer";
                inputNick.text = "DefaultPlayer";
            }
            else
            {
                inputNick.text = inputNick.text.Trim(' ');
                PhotonNetwork.player.NickName = inputNick.text;
                SaveNickName(inputNick.text);

                if (PhotonNetwork.offlineMode == false && PhotonNetwork.room != null)
                {
                    lobbyController.OnNameChanged();
                }
            }
        }

        private void SaveNickName(string input)
        {
            PlayerPrefs.SetString("nickName", input);
            PlayerPrefs.Save();
        }

        private string LoadNickName()
        {
            string nick = "Player";

            if (PlayerPrefs.HasKey("nickName"))
            {
                nick = PlayerPrefs.GetString("nickName");
            }

            PhotonNetwork.player.NickName = nick;
            return nick;
        }

        public void ChangeTo(RectTransform newPanel)
        {
            if (currentPanel != null)
            {
                currentPanel.gameObject.SetActive(false);
            }

            if (newPanel != null)
            {
                newPanel.gameObject.SetActive(true);
            }

            currentPanel = newPanel;

            if (currentPanel == singlePanel)
            {
                backDelegate = LeaveLobby;
            }

            if (currentPanel == multiPanel)
            {
                backDelegate = LeaveLobby;
            }

            if (currentPanel == friendsPanel)
            {
                backDelegate = LeaveRoom;
            }
                    }

        public void DisplayIsConnecting()
        {
            var _this = this;
            infoPanel.Display("Connecting...", false);
        }

        public void DisplayIsSearching()
        {
            var _this = this;
            infoPanel.Display("Searching for players");
        }

        public void StartConnecting()
        {
            infoPanel.Display("Connecting to the Server", false);
        }

        public delegate void BackButtonDelegate();
        private BackButtonDelegate backDelegate;

        public void GoBackButton()
        {
            backDelegate();
        }

        public void SetDelegate(BackButtonDelegate setDelegate)
        {
            backDelegate = setDelegate;
        }

        // ----------------- Server management

        public void LeaveLobby()
        {
            CheckRoom();
            if (PhotonNetwork.offlineMode == false)
            {
                PhotonNetwork.Disconnect();
            }
            StateManager.State = 0;
            connectClicked = false;
            topPanelUI.Move(1);
            ChangeTo(mainMenu);
        }

        public void LeaveRoom()
        {
            CheckRoom();
            connectClicked = false;
            if (PhotonNetwork.offlineMode)
                ChangeTo(mainMenu);
            else
                ChangeTo(multiPanel);
        }

        private void CheckRoom()
        {
            if (PhotonNetwork.offlineMode == false)
            {
                if (PhotonNetwork.room != null)
                    PhotonNetwork.LeaveRoom();
                else
                    cancelClicked = true;
                string[] keys = { "playerColor", "nickName", "ready" };
                PhotonNetwork.RemovePlayerCustomProperties(keys);
            }
        }

        public override void OnDisconnectedFromPhoton()
        {
            if (changeRegion == false)
            {
                PhotonNetwork.offlineMode = true;

                if (StateManager.State == 0)
                    matchController.ReloadLobbyGame();
            }
            else
            {
                changeRegion = false;
                Connect();
            }
        }

        public override void OnFailedToConnectToPhoton(DisconnectCause cause)
        {
            backDelegate = LeaveLobby;
            infoPanel.Display(cause.ToString(), () => backDelegate());
        }

        public void KickedMessage()
        {
            infoPanel.Display("Kicked by Server", () => ChangeTo(multiPanel));
        }
        //===================

        private void Connect()
        {
            StartConnecting();
            PhotonNetwork.ConnectUsingSettings(currentVersion);
        }

        public override void OnJoinedLobby()
        {
            if (joinClicked == false)
            {
                DisableInfoPanel();
                OnMultiPlayerPanelClick();
                serverSelect.SetRegion(PhotonNetwork.CloudRegion);
            }
        }       
              
        public void DisableInfoPanel()
        {
            infoPanel.Disable();
        }

        public bool JoinClicked
        {
            get
            {
                return joinClicked;
            }

            set
            {
                joinClicked = value;
            }
        }

        public bool CancelClicked
        {
            get
            {
                return cancelClicked;
            }
            set
            {
                cancelClicked = value;
            }
        }

        public void EnableLoadPanel()
        {
            LoadPanel.SetActive(true);
        }

        public void DisableLoadPanel()
        {
            LoadPanel.SetActive(false);
        }

        public bool ConnectClicked
        {
            set
            {
                connectClicked = value;
            }
        }

        public bool ChangeRegion
        {
            set
            {
                changeRegion = value;
            }
        }
    }
}
