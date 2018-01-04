using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreSlot : MonoBehaviour
{
    [SerializeField]
    private Text playerName;
    [SerializeField]
    private Text scoreValue;
    [SerializeField]
    private Image readyButton;
    [SerializeField]
    private Sprite readySprite;

    public void SetPlayer(string playerName, string timeValue, bool ready, bool readyButtonEnabled)
    {
        this.playerName.text = playerName;
        this.scoreValue.text = timeValue;

        if (readyButtonEnabled == false)
        {
            readyButton.gameObject.SetActive(false);
        }
        else
        {
            if (ready)
            {
                readyButton.overrideSprite = readySprite;
            }
        }
    }
}
