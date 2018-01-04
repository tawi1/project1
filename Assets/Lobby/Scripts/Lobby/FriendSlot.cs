using UnityEngine;
using UnityEngine.UI;

public class FriendSlot : MonoBehaviour
{
    [SerializeField]
    private Text nickName;
    [SerializeField]
    private Text status;
    [SerializeField]
    private Button joinButton;
    [SerializeField]
    private Button deleteButton;
    private string[] statuses = { "OFFLINE", "ONLINE", "IN ROOM" };

    public void SetFriend(string nickName, int status, UnityEngine.Events.UnityAction joinAction, UnityEngine.Events.UnityAction deleteAction)
    {
        this.nickName.text = nickName;
        this.status.text = statuses[status];
        this.status.color = new Color32(255, 0, 0, 255);

        if (status == 0)
        {
            joinButton.gameObject.SetActive(false);
        }
        else if (status == 1)
        {
            this.status.color = new Color32(127, 255, 0, 255);
            joinButton.gameObject.SetActive(false);
        }
        else if (status == 2)
        {
            joinButton.onClick.AddListener(delegate { joinAction(); });
        }

        deleteButton.onClick.AddListener(delegate { deleteAction(); });
    }

}
