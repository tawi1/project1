using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [SerializeField]
    private Atlas atlas;
    [SerializeField]
    private MatchController matchController;
    [SerializeField]
    private Transform[] m_SpawnPoint;
    private List<int> spawnList = new List<int>();

    public void Spawn()
    {
        if (!matchController.LobbyGame)
        {
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
            {
                spawnList.Add(p.ID);
            }
        }
        else
        {
            spawnList.Add(1);
        }

        int j = matchController.MaxId;

        for (int i = 0; i < matchController.AiCount; i++)
        {
            spawnList.Add(j);
            j++;
        }

        Shuffle();

        SpawnCars();
    }

    private void Shuffle()
    {
        for (int i = 0; i < spawnList.Count; i++)
        {
            int temp = spawnList[i];
            int randomIndex = Random.Range(i, spawnList.Count);
            spawnList[i] = spawnList[randomIndex];
            spawnList[randomIndex] = temp;
        }
    }

    private void SpawnCars()
    {
        int j = 1;
        int id = matchController.MaxId;
        for (int i = 0; i < spawnList.Count; i++)
        {
            if (spawnList[i] < matchController.MaxId)
            {
                SpawnPlayerCar(i, spawnList[i]);
            }
            else
            {
                SpawnAICar(i, j, id);
                j++;
                id++;
            }
        }
    }

    private void SpawnPlayerCar(int pos, int id)
    {
        GameObject car = PhotonNetwork.InstantiateSceneObject("Car", m_SpawnPoint[pos].position, m_SpawnPoint[pos].rotation, 0, null);

        int idCar = car.GetPhotonView().viewID;

        PhotonPlayer plr = PhotonPlayer.Find(id);
        if (plr.CustomProperties.ContainsKey("playerColor"))
        {
            atlas.PaintCar(idCar, (int)plr.CustomProperties["playerColor"], (int)plr.CustomProperties["car"]);
        }
        else
        {
            int indexCar = TextureLoader.GetRandom();

            atlas.PaintCar(idCar, 0, indexCar);
        }

        int viewId = car.GetPhotonView().viewID;

        car.GetPhotonView().TransferOwnership(plr);
        car.GetComponent<CarScript>().photonView.RPC("SetupPlayer", PhotonTargets.AllBuffered, plr.NickName, viewId);

        MatchController.SetCustomProperties(plr, false, false);
    }

    private List<int> pickedColors = new List<int>();

    private void SpawnAICar(int pos, int num, int id)
    {
        GameObject car = PhotonNetwork.InstantiateSceneObject("Car", m_SpawnPoint[pos].position, m_SpawnPoint[pos].rotation, 0, null);

        int idCar = car.GetPhotonView().viewID;

        int aiCar = 0;
        int aiLevel = 0;
        int aiColor = 0;

        if (matchController.LobbyGame == false)
        {
            int index = num - 1;

            string aiPropsStr = (string)PhotonNetwork.room.CustomProperties["aiProps"];

            var aiProps = AiPropPacker.Unpack(aiPropsStr);
            aiCar = aiProps[index].idCar;
            aiLevel = aiProps[index].aiLevel;

            if (pickedColors.Count == 0)
                pickedColors = CarColors.GetChosenColors();

            for (int j = 0; j < CarColors.Colors.Length; j++)
            {
                if (pickedColors.Contains(j) == false)
                {
                    aiColor = j;
                    break;
                }
            }

            pickedColors.Add(aiColor);
        }
        else
        {
            aiCar = TextureLoader.GetRandom();
            aiColor = num;
        }

        atlas.PaintCar(idCar, aiColor, aiCar);

        PhotonView carAi = car.GetComponent<CarScript>().photonView;

        carAi.RPC("SetAI", PhotonTargets.All, aiLevel);
        carAi.RPC("SetupPlayer", PhotonTargets.All, "AI" + (num), 0);
        carAi.RPC("SetLocalId", PhotonTargets.All, id);
    }

    public Vector2 GetSpawnPosition(int pos)
    {
        return m_SpawnPoint[pos].position;
    }

    public Quaternion GetSpawnRotation(int pos)
    {
        return m_SpawnPoint[pos].rotation;
    }

    public string GetPlayerList()
    {
        string list = "";
        for (int i = 0; i < spawnList.Count; i++)
        {
            list += spawnList[i] + ";";
        }
        return list;
    }
}
