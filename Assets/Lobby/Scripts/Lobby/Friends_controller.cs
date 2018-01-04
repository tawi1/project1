using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Friends_controller : MonoBehaviour
{
    [SerializeField]
    private RoomManager roomManager;
    [SerializeField]
    private Prototype.NetworkLobby.LobbyManager lobbyManager;
    [SerializeField]
    private GameObject grid;
    [SerializeField]
    private GameObject item;
    [SerializeField]
    private GameObject inputPanel;
    [SerializeField]
    private InputField inputField;

    private int count = 0;
    private bool deleteMode = false;

    [SerializeField]
    List<string> currentList = new List<string>();

    [SerializeField]
    private GameObject[] panels;
    [SerializeField]
    private Text status;

    [SerializeField]
    private GameObject disconnectButton;
    private string friends = "";

    void OnEnable()
    {
        Refresh(true);
    }

    void Start()
    {
        /*  if (PlayerPrefs.HasKey("friends"))
          {
              PlayerPrefs.DeleteKey("friends");
          }*/
    }

    private void InitFriend(string nick, int status, string room, UnityEngine.Events.UnityAction deleteAction)
    {
        FriendSlot slot = Instantiate(item, grid.transform).GetComponent<FriendSlot>();

        slot.SetFriend(nick, status, () => { JoinRoom(room); }, deleteAction);
    }

    public void Refresh(bool firstLoad)
    {
        FillFriends(firstLoad);
    }

    public void AddFriend()
    {
        if (inputField.text.Trim() != "")
        {
            string[] arr = AddFriend(inputField.text.Trim().ToLower());

            inputPanel.SetActive(false);
            count++;

            PhotonNetwork.FindFriends(arr);

            if (PhotonNetwork.Friends == null)
            {
                Refresh(true);
            }
            else
                Refresh(false);

            inputField.text = "";
        }
        else
        {
            inputPanel.SetActive(false);
        }
    }

    public void OnAddClick()
    {
        inputPanel.SetActive(true);
    }

    private string[] AddFriend(string friend)
    {
        Debug.Log(friend);
        string[] arr = new string[1];

        if (PlayerPrefs.HasKey("friends"))
        {
            friends = PlayerPrefs.GetString("friends");

            if (string.IsNullOrEmpty(friends.Trim()) == false)
            {
                friends = FriendPropPacker.AddProp(friend, friends);

                SaveFriends(friends);

                arr = FriendPropPacker.Unpack(friends).ToArray();

                return arr;
            }
            else
            {
                friends = FriendPropPacker.AddProp(friend, friends);

                SaveFriends(friends);
                arr[0] = friend;
                return arr;
            }
        }
        else
        {
            arr[0] = friend;
            friends = FriendPropPacker.AddProp(friend, friends);

            SaveFriends(friends);
            return arr;
        }
    }

    private void DeleteFriend(int index)
    {
        if (PlayerPrefs.HasKey("friends"))
        {
            count--;
            friends = PlayerPrefs.GetString("friends");

            if (string.IsNullOrEmpty(friends.Trim()) == false)
            {
                friends = FriendPropPacker.Delete(index, friends);
                //currentList.Remove(friend);

                Debug.Log(friends);
                SaveFriends(friends);
            }
            Refresh(false);
        }
    }

    private void FillFriends(bool firstLoad)
    {
        if (PlayerPrefs.HasKey("friends"))
        {
            friends = PlayerPrefs.GetString("friends");
            if (string.IsNullOrEmpty(friends.Trim()) == false)
            {
                string[] arr = FriendPropPacker.Unpack(friends).ToArray();

                if (arr.Length > 0)
                {
                    PhotonNetwork.FindFriends(arr);

                    StartCoroutine(LoadFriends(firstLoad));
                }
                else
                {
                    friends = "";
                    Clear();
                }
            }
            else
            {
                Clear();
            }
        }
        else
        {
            Clear();
        }
    }

    private void Clear()
    {
        ClearList();
        currentList = new List<string>();
        if (PhotonNetwork.Friends != null)
            PhotonNetwork.Friends.Clear();
    }

    private IEnumerator WaitForFriends()
    {
        while (PhotonNetwork.Friends == null)
        {
            yield return null;
        }
    }

    private IEnumerator WaitEditFriends()
    {
        bool cond = false;

        while (cond == false)
        {
            List<string> newList = new List<string>();
            foreach (var info in PhotonNetwork.Friends)
            {
                newList.Add(info.Name);
            }

            if (ScrambledEquals(newList, currentList) == false)
            {
                cond = true;
            }

            yield return null;
        }
    }

    private IEnumerator LoadFriends(bool firstLoad)
    {
        if (firstLoad)
            yield return StartCoroutine(WaitForFriends());
        else
            yield return StartCoroutine(WaitEditFriends());

        ClearList();
        count = PhotonNetwork.Friends.Count;

        currentList = new List<string>();

        int i = 0;
        foreach (var info in PhotonNetwork.Friends)
        {
            currentList.Add(info.Name);
            int status = 0;

            if (info.IsInRoom)
            {
                status = 2;
            }
            else if (info.IsOnline)
            {
                status = 1;
            }

            int index = i;
            InitFriend(info.Name, status, info.Room, () => { DeleteFriend(index); });
            i++;
        }
    }

    private void SaveFriends(string input)
    {
        PlayerPrefs.SetString("friends", input);
        PlayerPrefs.Save();
    }

    private void ClearList()
    {
        var items = grid.GetComponentsInChildren<Transform>();
        for (int i = 1; i < items.Length; i++)
        {
            if (items[i] != null)
                Destroy(items[i].gameObject);
        }
    }

    public void OnRefreshClick()
    {
        Refresh(true);
    }

    public void OnBackgroundInputClick()
    {
        inputPanel.SetActive(false);
        inputField.text = "";
    }

    public void CreateParty()
    {
        roomManager.CreateFriendRoom();
        lobbyManager.DisplayIsConnecting();
        gameObject.SetActive(false);
    }

    private void JoinRoom(string name)
    {
        //Exp_controller.friend = true;
        /*panels[0].SetActive(false);
        panels[1].SetActive(true);
        disconnectButton.SetActive(false);
        //status.text = LocalizationText.GetText("connection");*/
        roomManager.JoinFriendRoom(name);
        lobbyManager.DisplayIsConnecting();
        gameObject.SetActive(false);
    }

    public void EnableDisconnect()
    {
        disconnectButton.SetActive(true);
    }

    public void SetStatus(string input)
    {
        status.text = input;
    }

    public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
    {
        var cnt = new Dictionary<T, int>();
        foreach (T s in list1)
        {
            if (cnt.ContainsKey(s))
            {
                cnt[s]++;
            }
            else
            {
                cnt.Add(s, 1);
            }
        }
        foreach (T s in list2)
        {
            if (cnt.ContainsKey(s))
            {
                cnt[s]--;
            }
            else
            {
                return false;
            }
        }
        return cnt.Values.All(c => c == 0);
    }
}
