using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Message : MonoBehaviour
{
    [SerializeField]
    private GameObject messagePanel;
    [SerializeField]
    private HorizontalLayoutGroup layout;
    [SerializeField]
    private GameObject dollarIcon;
    [SerializeField]
    private Text messageText;
    [SerializeField]
    private Button yesButton;
    [SerializeField]
    private Button noButton;

    public void ShowMessage(string message, Action<bool> callback, bool showDollarIcon, bool showYesButton)
    {
        messagePanel.SetActive(true);
        messageText.text = message;
        yesButton.gameObject.SetActive(true);
        dollarIcon.SetActive(false);

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        if (callback != null)
        {
            yesButton.onClick.AddListener(delegate { callback(true); HideMessage(); });
            noButton.onClick.AddListener(delegate { callback(false); HideMessage(); });
        }
        else
        {
            yesButton.onClick.AddListener(delegate { HideMessage(); });
            noButton.onClick.AddListener(delegate { HideMessage(); });
        }

        if (showYesButton)
        {
            yesButton.gameObject.SetActive(true);
            noButton.GetComponentInChildren<Text>().text = "No";
        }
        else
        {
            yesButton.gameObject.SetActive(false);
            noButton.GetComponentInChildren<Text>().text = "Ok";
        }

        if (showDollarIcon == true)
        {
            dollarIcon.SetActive(true);
        }
        else
        {
            dollarIcon.SetActive(false);
        }
      
       StartCoroutine(ChangeLayout());
    }

    private IEnumerator ChangeLayout()
    {
        layout.childForceExpandHeight = true;
        yield return new WaitForSeconds(0.01f);
        layout.childForceExpandHeight = false;
    }

    private void HideMessage()
    {
        messagePanel.SetActive(false);
    }
}
