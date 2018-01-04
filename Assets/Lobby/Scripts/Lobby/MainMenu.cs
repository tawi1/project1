using UnityEngine;
using System.Collections;

namespace Prototype.NetworkLobby
{
    public class MainMenu : Photon.MonoBehaviour
    {
        [SerializeField]
        private RectTransform Settings;
        [SerializeField]
        private LobbyManager lobbyManager;
        MatchController matchController;
        [SerializeField]
        private GameObject backGround;

        void Start()
        {
            SettingsController.InitSound();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Settings.gameObject.activeSelf == false)
                    Application.Quit();
                Settings.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (matchController == null)
            {
                matchController = FindObjectOfType<MatchController>();
            }
        }

        public void OnSingleplayerClick()
        {
            lobbyManager.OnSinglePlayerClick();
        }

        public void OnMultiplayerClick()
        {
            if (PhotonNetwork.room != null)
                PhotonNetwork.LeaveRoom();

            //  backGround.SetActive(true);
            lobbyManager.ConnectClicked = true;
            lobbyManager.LoadMultiplayer();
        }

        public void OnSettingsClick()
        {
            Settings.gameObject.SetActive(true);
        }

        public void OnQuitClick()
        {
            Application.Quit();
        }

        public void OnRateClick()
        {
            ShareAndRate.RateUs();
        }
    }
}