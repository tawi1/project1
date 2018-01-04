using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField]
    private GameObject menu;
    [SerializeField]
    private RectTransform mainPanel;
    [SerializeField]
    private RectTransform settingsPanel;
    [SerializeField]
    private RectTransform cameraSelection;
    [SerializeField]
    private Button exitButton;
    [SerializeField]
    private GameObject backgroundPanel;
    [SerializeField]
    private GameObject scorePanel;

    private void Start()
    {
        scorePanel.GetComponent<ScoreMenu>().Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (scorePanel.activeSelf == false)
                if (menu.activeSelf)
                {
                    if (settingsPanel.gameObject.activeSelf)
                    {
                        DisableSettingsPanel();
                    }
                    else
                        DisableMenu();
                }
                else
                {
                    EnableMenu(false);
                }
        }
    }

    public void OnResumeClick()
    {
        DisableMenu();
    }

    public void EnableMenu(bool matchMenu)
    {
        foreach (Image img in GetComponentsInChildren<Image>())
        {
            img.raycastTarget = true;
        }

        backgroundPanel.SetActive(true);
        if (matchMenu)
        {
            scorePanel.GetComponent<ScoreMenu>().Launch();
            menu.SetActive(false);
        }
        else
        {
            if (scorePanel.activeSelf == false)
            {
                if (PhotonNetwork.offlineMode)
                {
                    Time.timeScale = 0f;
                    AudioListener.pause = true;
                }

                menu.SetActive(true);
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(OnExitClick);
            }
        }
    }

    public void DisableMenu()
    {
        if (PhotonNetwork.offlineMode)
        {
            Time.timeScale = 1f;
            if (Prototype.NetworkLobby.SettingsController.GetSound())
                AudioListener.pause = false;
        }

        backgroundPanel.SetActive(false);

        menu.SetActive(false);
        scorePanel.SetActive(false);

        foreach (Image img in GetComponentsInChildren<Image>())
        {
            img.raycastTarget = false;
        }
    }

    void OnExitClick()
    {
        MatchMenu.OnExitClick();
    }

    public void OnCameraSelectionEnable()
    {
        //if (FindObjectOfType<MatchController>().Finished == false)      
        backgroundPanel.SetActive(false);
        cameraSelection.gameObject.SetActive(true);
    }

    public void EnableSettingsPanel()
    {
        mainPanel.gameObject.SetActive(false);
        settingsPanel.gameObject.SetActive(true);
    }

    public void DisableSettingsPanel()
    {
        mainPanel.gameObject.SetActive(true);
        settingsPanel.gameObject.SetActive(false);
    }

    public void OnRateClick()
    {
        ShareAndRate.RateUs();
    }
}

