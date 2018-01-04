using UnityEngine.UI;
using UnityEngine;

public class MatchMenu : MonoBehaviour
{
    [SerializeField]
    private Button acceptButton;
    [SerializeField]
    private Button declineButton;
    private TotalScoreController totalScoreController;

    void Awake()
    {
        declineButton.onClick.RemoveAllListeners();
        declineButton.onClick.AddListener(OnExitClick);
        acceptButton.onClick.RemoveAllListeners();
        if (totalScoreController == null)
            totalScoreController = FindObjectOfType<TotalScoreController>();
        acceptButton.onClick.AddListener(totalScoreController.onReadyClick);
    }

    void OnEnable()
    {
        if (totalScoreController == null)
            totalScoreController = FindObjectOfType<TotalScoreController>();
        totalScoreController.UpdateTotalScore();
    }

    public static void OnExitClick()
    {
        string[] keys = { "playerColor", "nickName", "ready" };
        Time.timeScale = 1f;
        PhotonNetwork.RemovePlayerCustomProperties(keys);

        FlexibleMusicManager.instance.Pause();

        if (PhotonNetwork.room != null & PhotonNetwork.connected)
        {
            if (PhotonNetwork.LeaveRoom())
            {
                PhotonNetwork.LoadLevel("Lobby");
            }
        }
        else
        {
            PhotonNetwork.LoadLevel("Lobby");
        }
    }
}
