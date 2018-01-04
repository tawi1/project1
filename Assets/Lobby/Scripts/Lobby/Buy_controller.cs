using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Buy_controller : MonoBehaviour
{
    [SerializeField]
    private Message message;

    private int selectedId;
    private Action<int> selectedCallBack;

    private void Buy(int id, Action<int> callback)
    {
        int price = CarSpecification.GetPrice(id);

        bool success = Purse.Purchase(price);

        if (success)
        {
            Purse.AddCar(id);
            callback(id);
            SaveData.Save();
        }
        else
        {
            string textMessage = "Not enough money";
            message.ShowMessage(textMessage, null, false, false);
        }
    }

    public void OnBuyClick(int id, Action<int> callback)
    {
        selectedId = id;
        selectedCallBack = callback;

        string textMessage = "Are you sure buy for " + CarSpecification.GetPrice(id).ToString();
        message.ShowMessage(textMessage, OnAcceptClick, true, true);
    }

    public void OnAcceptClick(bool accept)
    {
        if (accept)
        {
            Buy(selectedId, selectedCallBack);
        }
    }

}
