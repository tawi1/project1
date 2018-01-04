using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendPropPacker : MonoBehaviour
{
    public static string AddProp(string newNick, string jsonProps)
    {
        var propsList = Unpack(jsonProps);

        if (propsList.Contains(newNick) == false)
            propsList.Add(newNick);

        string json = MiniJSON.Json.Serialize(propsList);
        return json;
    }

    public static string Delete(int index, string jsonProps)
    {
        var propsList = Unpack(jsonProps);

        List<string> props = new List<string>();

        int i = 0;

        foreach (string item in propsList)
        {
            if (i != index)
                props.Add(item);
            i++;
        }

        string json = MiniJSON.Json.Serialize(props);
        return json;
    }

    public static List<string> Unpack(string json)
    {
        List<string> props = new List<string>();

        if (string.IsNullOrEmpty(json) == false)
        {
            var objectList = (List<object>)MiniJSON.Json.Deserialize(json);

            foreach (string nick in objectList)
            {
                if (string.IsNullOrEmpty(nick) == false)
                    props.Add(nick);
            }
        }

        return props;
    }
}
