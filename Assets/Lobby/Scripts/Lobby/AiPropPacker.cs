using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AiPropPacker : MonoBehaviour
{
    public struct aiProp
    {
        public int idCar;
        public int idColor;
        public int aiLevel;

        public aiProp(int idCar, int idColor, int aiLevel)
        {
            this.idCar = idCar;
            this.idColor = idColor;
            this.aiLevel = aiLevel;
        }
    }

    public static string Pack(int aiCount, int aiLevel, List<int> chosenColors)
    {
        List<Dictionary<string, object>> props = new List<Dictionary<string, object>>();

        for (int i = 0; i < aiCount; i++)
        {
            int newColor = GetColor(chosenColors);
            chosenColors.Add(newColor);

            Dictionary<string, object> prop = CreateProp(new aiProp(TextureLoader.GetRandom(), newColor, aiLevel));

            props.Add(prop);
        }

        string json = MiniJSON.Json.Serialize(props);

        return json;
    }

    private static Dictionary<string, object> CreateProp(aiProp aiprop)
    {
        Dictionary<string, object> prop = new Dictionary<string, object>();
        prop.Add("idCar", aiprop.idCar);
        prop.Add("idColor", aiprop.idColor);
        prop.Add("aiLevel", aiprop.aiLevel);
        return prop;
    }

    public static string AddProp(int aiCount, int aiLevel, List<int> chosenColors, string jsonProps)
    {
        var propsList = Unpack(jsonProps);

        for (int i = 0; i < aiCount; i++)
        {
            int newColor = GetColor(chosenColors);
            chosenColors.Add(newColor);

            propsList.Add(new aiProp(TextureLoader.GetRandom(), newColor, aiLevel));
        }

        List<Dictionary<string, object>> props = new List<Dictionary<string, object>>();

        foreach (aiProp item in propsList)
        {
            props.Add(CreateProp(item));
        }

        string json = MiniJSON.Json.Serialize(props);
        return json;
    }

    public static string DeleteProp(int index, string jsonProps)
    {
        var propsList = Unpack(jsonProps);

        List<Dictionary<string, object>> props = new List<Dictionary<string, object>>();

        int i = 0;

        foreach (aiProp item in propsList)
        {
            if (i != index)
                props.Add(CreateProp(item));
            i++;
        }

        string json = MiniJSON.Json.Serialize(props);
        return json;
    }

    public static string ChangeProp(int index, int newLevel, string jsonProps)
    {
        var propsList = Unpack(jsonProps);

        List<Dictionary<string, object>> props = new List<Dictionary<string, object>>();

        int i = 0;

        foreach (aiProp item in propsList)
        {
            if (i != index)
            {
                props.Add(CreateProp(item));
            }
            else
            {
                aiProp newItem;
                newItem = item;
                newItem.aiLevel = newLevel;
                props.Add(CreateProp(newItem));
            }

            i++;
        }

        string json = MiniJSON.Json.Serialize(props);
        return json;
    }

    public static List<aiProp> Unpack(string json)
    {
        List<aiProp> props = new List<aiProp>();

        if (string.IsNullOrEmpty(json) == false)
        {
            var objectList = (List<object>)MiniJSON.Json.Deserialize(json);

            foreach (Dictionary<string, object> prop in objectList)
            {
                int idCar = Convert.ToInt32((long)prop["idCar"]);
                int idColor = Convert.ToInt32((long)prop["idColor"]);
                int aiLevel = Convert.ToInt32((long)prop["aiLevel"]);

                props.Add(new aiProp(idCar, idColor, aiLevel));
            }
        }

        return props;
    }

    private static int GetColor(List<int> chosenColors)
    {
        bool cond = false;

        int i = 0;
        while (cond == false)
        {
            if (chosenColors.Contains(i) == false)
            {
                cond = true;
            }
            else
            {
                i++;
            }
        }

        return i;
    }
}
