using UnityEngine;
using System.Text;

public class TimeLib : MonoBehaviour
{
    public static string GetTime(int input)
    {
        if (input > 100)
        {
            int minutesInt = (int)Mathf.Ceil(input / 60000);
            string minutes = minutesInt.ToString();

            string seconds = ((Mathf.Ceil(input / 1000) - minutesInt * 60)).ToString();
            string mili = input.ToString().Substring(input.ToString().Length - 3, 3);

            StringBuilder sb = new StringBuilder(minutes);
            sb.Append(":").Append(seconds).Append(".").Append(mili);

            string total = sb.ToString();

            return total;
        }
        else
        {
            return "0:00.000";
        }
    }

    public static string GetSeconds(int input)
    {
        if (input > 100)
        {
            int minutesInt = (int)Mathf.Ceil(input / 60000);
            string minutes = minutesInt.ToString();
            string seconds = ((Mathf.Ceil(input / 1000) - minutesInt * 60)).ToString();
            StringBuilder sb = new StringBuilder(minutes);
            sb.Append(":").Append(seconds);
            string total = sb.ToString();

            return total;
        }
        else
        {
            return "0:00";
        }
    }

    public static int GetTotalSeconds(float input)
    {
        if (input > 1000)
        {
            float seconds = Mathf.Ceil(input / 1000);
            return (int)seconds;
        }
        else
        {
            return 0;
        }
    }
}
