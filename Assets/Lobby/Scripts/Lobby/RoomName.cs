public class RoomName
{
    public static string Rename(string roomName)
    {
        string sym = roomName.Substring(roomName.Length - 1, 1);
        int number;
        if (int.TryParse(sym, out number))
        {
            number++;
            roomName = roomName.Substring(0, roomName.Length - 1) + number;
        }
        else
        {
            roomName += "1";
        }

        return roomName;
    }

    public static string GetServerName(string newName)
    {
        var matches = PhotonNetwork.GetRoomList();
        int j = 0;

        bool match = true;
        string roomName = newName;
        while (match == true)
        {
            match = false;
            for (int i = 0; i < matches.Length; i++)
            {
                if (matches[i].Name == newName)
                {
                    match = true;
                    break;
                }
            }

            if (match == true)
                j++;

            if (j > 0)
            {
                newName = roomName + j.ToString();
            }
        }
        return newName;
    }
}
