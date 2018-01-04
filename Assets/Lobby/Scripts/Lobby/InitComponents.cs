using UnityEngine;

public class InitComponents : MonoBehaviour
{
    [SerializeField]
    private Car_select_controller carsController;
    [SerializeField]
    private Map_Select_Controller mapController;
    [SerializeField]
    private GameObject LoadPanel;
    [SerializeField]
    private GameObject singlePlayerPanel;
    [SerializeField]
    private LobbyController lobby;
    [SerializeField]
    private ServerSelect server;

    void Start()
    {
        LoadPanel.SetActive(true);
        TextureLoader.Init();
        DataPacker.UnpackData();
        singlePlayerPanel.SetActive(true);
        carsController.Init();
        mapController.Init();
        singlePlayerPanel.SetActive(false);
        lobby.Init();
        server.Init();
        LoadPanel.SetActive(false);
    }
}
