using UnityEngine;
using UnityEngine.UI;

namespace Prototype.NetworkLobby
{
    public class LobbyInfoPanel : MonoBehaviour
    {
        [SerializeField]
        private Text infoText;
        [SerializeField]
        private GameObject infoPanel;
        [SerializeField]
        private GameObject backButton;
        [SerializeField]
        private Button okButton;

        public void Display(string info)
        {
            infoPanel.SetActive(true);
            infoText.text = info;
        }

        public void Display(string info, bool showButton)
        {
            Display(info);
            backButton.SetActive(showButton);
        }

        public void Display(string info, UnityEngine.Events.UnityAction action)
        {
            Display(info);
            okButton.gameObject.SetActive(true);
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(delegate { action(); okButton.gameObject.SetActive(false); Disable(); });
        }

        public void Disable()
        {
            infoPanel.SetActive(false);
            backButton.SetActive(true);
        }
    }
}