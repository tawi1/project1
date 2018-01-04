using UnityEngine;
using System.Collections.Generic;

public class CarColors
{
    public static Color[] Colors = new Color[] {
        Color.red,
        Color.green,
        Color.yellow,
        Color.HSVToRGB(0.11f, 1.00f, 1.0f, false),
        Color.HSVToRGB(0.75f, 0.68f, 0.92f, false),
        Color.blue,
        Color.cyan,
        Color.magenta,
        Color.grey, };

    public static List<int> GetChosenColors()
    {
        List<int> colorInUse = new List<int>();
        foreach (PhotonPlayer p in PhotonNetwork.playerList)
        {
            if (p.CustomProperties.ContainsKey("playerColor"))
                colorInUse.Add((int)p.CustomProperties["playerColor"]);
        }
        return colorInUse;
    }

    public static int CheckColor(int id, int color)
    {
        if (color == -1)
            color = 0;

        bool check = false;

        foreach (PhotonPlayer p in PhotonNetwork.playerList)
        {
            if (p.ID != id)
            {
                if (p.CustomProperties["playerColor"] != null)
                {
                    if (color != (int)p.CustomProperties["playerColor"])
                    {
                        check = true;
                    }
                    else
                    {
                        check = false;
                        break;
                    }
                }
                else
                {
                    check = true;
                }
            }
            else
            {
                check = true;
            }
        }

        if (check)
        {
            return color;
        }
        else
        {
            int i = -1;
            check = false;
            while (check == false)
            {
                i++;
                foreach (PhotonPlayer p in PhotonNetwork.playerList)
                {
                    if (p.ID != id)
                    {
                        if (p.CustomProperties["playerColor"] != null)
                        {
                            if (i != (int)p.CustomProperties["playerColor"])
                            {
                                check = true;
                            }
                            else
                            {
                                check = false;
                                break;
                            }
                        }
                        else
                        {
                            check = true;
                        }
                    }
                    else
                    {
                        check = true;
                    }
                }
            }
            return i;
        }
    }
}