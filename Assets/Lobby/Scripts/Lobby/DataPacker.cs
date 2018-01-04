using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DataPacker : MonoBehaviour
{
    public static string PackData()
    {
        int credit = Purse.GetCredit();
        List<int> cars = Purse.GetCars();

        Dictionary<string, object> data = new Dictionary<string, object>();

        data.Add("credit", credit);
        data.Add("cars", cars);

        string json = MiniJSON.Json.Serialize(data);

        json = Encrypt.EncryptDecrypt(json);

        return json;
    }

    public static void UnpackData()
    {
        int credit = 1000000;
        List<int> cars = new List<int>();

        string json = SaveData.Load();

        if (json.Length > 0)
        {
            json = Encrypt.EncryptDecrypt(json);

            var q = MiniJSON.Json.Deserialize(json);

            Dictionary<string, object> arr = q as Dictionary<string, object>;

            if (arr != null)
            {
                credit = Convert.ToInt32(arr["credit"]);

                var carsArray = (List<object>)arr["cars"];

                for (int i = 0; i < carsArray.Count; i++)
                {
                    cars.Add(Convert.ToInt32((long)carsArray[i]));
                }
            }
        }

        Purse.SetPurse(credit, cars);
    }
}
