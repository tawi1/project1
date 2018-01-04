using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectManager : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject m_ObjectSpawnPoint;
    private Transform[] spawnPoints;
    [SerializeField]
    private GameObject[] Bonunes;
    [SerializeField]
    private GameObject objectPool;
    [SerializeField]
    private GameObject areaObjectPool;
    [SerializeField]
    private List<BonusNode> mapBonuses = new List<BonusNode>();

    private List<BonusNode> objects = new List<BonusNode>();

    private List<AreaObject> areaObjects = new List<AreaObject>();
    [SerializeField]
    int spawnLimit = 3;
    [SerializeField]
    int spawned = 0;
    int spawnTimer = 1;
    int oilTime = 10;
    int bombTime = 10;
    private int[] weaponIndexes = { 2, 3, 4, 5, 6 };

    // Use this for initialization
    public void SpawnObjects()
    {
        spawnPoints = m_ObjectSpawnPoint.GetComponentsInChildren<Transform>();
        if (spawnLimit > spawnPoints.Length - 1)
        {
            spawnLimit = spawnPoints.Length - 1;
        }

        if (PhotonNetwork.isMasterClient)
        {
            GeneratePool();
            GenerateAreaPool();
            StartCoroutine(WaitForSpawn());
        }
    }

    private IEnumerator WaitForSpawn()
    {
        while (objects.Count < Bonunes.Length * 4)
        {
            yield return null;
        }

        for (int i = 0; i < spawnLimit; i++)
        {
            Spawn();
        }
    }

    private void GeneratePool()
    {
        for (int i = 0; i < Bonunes.Length; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                var g = PhotonNetwork.InstantiateSceneObject(Bonunes[i].name, Vector2.zero, Quaternion.identity, 0, null);
            }
        }
    }

    private void GenerateAreaPool()
    {
        if (FindObjectOfType<MatchController>().LobbyGame == false)
        {
            for (int j = 0; j < 5; j++)
            {
                var g = PhotonNetwork.InstantiateSceneObject("Stain", Vector2.zero, Quaternion.identity, 0, null);
            }

            for (int j = 0; j < 5; j++)
            {
                var g = PhotonNetwork.InstantiateSceneObject("IceArea", Vector2.zero, Quaternion.identity, 0, null);
            }

            var data = new object[2];

            data[0] = 0.7f;
            data[1] = 1.3f;

            for (int j = 0; j < 4; j++)
            {
                var g = PhotonNetwork.InstantiateSceneObject("Explosion", Vector2.zero, Quaternion.identity, 0, data);
            }

            var data2 = new object[2];

            data2[0] = 0f;
            data2[1] = 0f;

            for (int j = 0; j < 4; j++)
            {
                var g = PhotonNetwork.InstantiateSceneObject("Explosion", Vector2.zero, Quaternion.identity, 0, data2);
            }

            for (int j = 0; j < 7; j++)
            {
                var g = PhotonNetwork.InstantiateSceneObject("ActivatedBomb", Vector2.zero, Quaternion.identity, 0, null);
            }
        }
        else
        {
            var soundData = new object[1];
            soundData[0] = FindObjectOfType<MatchController>().LobbyGame;

            for (int j = 0; j < 3; j++)
            {
                var g = PhotonNetwork.InstantiateSceneObject("Stain", Vector2.zero, Quaternion.identity, 0, soundData);
            }

            for (int j = 0; j < 3; j++)
            {
                var g = PhotonNetwork.InstantiateSceneObject("IceArea", Vector2.zero, Quaternion.identity, 0, soundData);
            }

            var data = new object[3];

            data[0] = 0.7f;
            data[1] = 1.3f;
            data[2] = true;

            for (int j = 0; j < 2; j++)
            {
                var g = PhotonNetwork.InstantiateSceneObject("Explosion", Vector2.zero, Quaternion.identity, 0, data);
            }

            var data2 = new object[3];

            data2[0] = 0f;
            data2[1] = 0f;
            data2[2] = true;

            for (int j = 0; j < 2; j++)
            {
                var g = PhotonNetwork.InstantiateSceneObject("Explosion", Vector2.zero, Quaternion.identity, 0, data2);
            }

            for (int j = 0; j < 3; j++)
            {
                var g = PhotonNetwork.InstantiateSceneObject("ActivatedBomb", Vector2.zero, Quaternion.identity, 0, null);
            }
        }
    }

    public void CreateArea(AreaObject.AreaType type, Vector2 pos, int ownerId)
    {
        int viewId = 0;
        for (int i = 0; i < areaObjects.Count; i++)
        {
            if (areaObjects[i].Spawned == false && areaObjects[i].Type == type)
            {
                viewId = areaObjects[i].gameObject.GetPhotonView().viewID;
                break;
            }
        }

        int randomAngle = Random.Range(0, 361);
        Quaternion q = Quaternion.Euler(0, 0, randomAngle);

        int time = PhotonNetwork.ServerTimestamp;
        photonView.RPC("RPCCreateArea", PhotonTargets.All, viewId, pos, q, ownerId, time);
    }

    public void CreateArea(AreaObject.AreaType type, bool rocketExplode, Vector2 pos, int ownerId)
    {
        int viewId = 0;
        for (int i = 0; i < areaObjects.Count; i++)
        {
            if (areaObjects[i].Spawned == false && areaObjects[i].Type == type)
            {
                if (areaObjects[i].GetComponent<ExplosionController>().RocketExplode == rocketExplode)
                {
                    viewId = areaObjects[i].gameObject.GetPhotonView().viewID;
                    break;
                }
            }
        }

        int time = PhotonNetwork.ServerTimestamp;
        photonView.RPC("RPCCreateArea", PhotonTargets.All, viewId, pos, Quaternion.identity, ownerId, time);
    }

    [PunRPC]
    private void RPCCreateArea(int viewId, Vector2 pos, Quaternion rot, int ownerId, int time)
    {
        PhotonView areaView = PhotonView.Find(viewId);
        if (areaView != null)
        {
            GameObject area = areaView.gameObject;

            AreaObject areaObject = area.GetComponent<AreaObject>();
            areaObject.Spawned = true;
            areaObject.transform.position = pos;

            if (areaObject.Type == AreaObject.AreaType.IceArea)
            {
                if (areaObject.gameObject.GetPhotonView().ownerId != ownerId)
                {
                    areaObject.gameObject.GetPhotonView().TransferOwnership(ownerId);
                }
            }
            areaObject.Launch(ownerId);
            if (areaObject.Type == AreaObject.AreaType.Stain)
            {
                areaObject.transform.rotation = rot;
                float currentTime = PhotonNetwork.ServerTimestamp - time;
                currentTime /= 1000;
                currentTime = oilTime - currentTime;
                StartCoroutine(AddOilTimer(currentTime, viewId));
            }
            else if (areaObject.Type == AreaObject.AreaType.Bomb)
            {
                float currentTime = PhotonNetwork.ServerTimestamp - time;
                currentTime /= 1000;
                currentTime = bombTime - currentTime;
                StartCoroutine(AddBombTimer(currentTime, viewId, areaObject));
            }
        }
    }

    public void RemoveArea(int viewId)
    {
        photonView.RPC("RPCRemoveArea", PhotonTargets.All, viewId);
    }

    [PunRPC]
    public void RPCRemoveArea(int viewId)
    {
        var g = PhotonView.Find(viewId).transform;
        g.transform.position = Vector2.zero;

        AreaObject areaObject = g.GetComponent<AreaObject>();
        areaObject.Spawned = false;

        if (areaObject.Type == AreaObject.AreaType.FireArea)
        {
            ExplosionController explosionController = g.GetComponent<ExplosionController>();
            explosionController.Reset();
        }
        else if (areaObject.Type == AreaObject.AreaType.IceArea)
        {
            IceAreaController iceAreaController = g.GetComponent<IceAreaController>();
            iceAreaController.Reset();
        }
        else if (areaObject.Type == AreaObject.AreaType.Bomb)
        {
            BombController bombController = g.GetComponent<BombController>();
            bombController.OwnerId = 0;
        }
        else if (areaObject.Type == AreaObject.AreaType.Stain)
        {
            StainId stainId = g.GetComponent<StainId>();
            stainId.OwnerId = 0;
        }

        g.gameObject.SetActive(false);
    }

    public void DeleteObject(GameObject g, bool respawn)
    {
        if (g.GetPhotonView().isMine)
        {
            if (g.GetComponent<BonusNode>().Spawned == true)
            {
                if (respawn)
                    SpawnObject();

                photonView.RPC("RPCDeleteObject", PhotonTargets.All, g.GetPhotonView().viewID, respawn);
            }
        }
        else
        {
            g.SetActive(false);
        }
    }

    [PunRPC]
    public void RPCDeleteObject(int viewId, bool respawn)
    {
        var g = PhotonView.Find(viewId).transform;

        BonusNode bonusNode = g.GetComponent<BonusNode>();

        bonusNode.Spawned = false;

        g.transform.position = Vector2.zero;
        g.transform.rotation = Quaternion.identity;
        g.GetComponent<Collider2D>().enabled = true;
        g.GetComponent<Collider2D>().isTrigger = true;
        g.transform.parent = objectPool.transform;

        if (bonusNode.Index == 5)
        {
            g.GetComponent<GunId>().Id = -1;
            g.transform.localScale = new Vector2(0.75f, 0.75f);
        }
        else if (bonusNode.Index == 6)
        {
            g.GetComponent<GunId>().Id = -1;
            g.transform.localScale = new Vector2(1, 1);
        }

        RefreshMap(g.gameObject);
        g.gameObject.SetActive(false);

    }

    public void DeleteRocket(int viewID, int idCar)
    {
        photonView.RPC("RPCDeleteRocket", PhotonTargets.All, viewID, idCar);
    }

    [PunRPC]
    public void RPCDeleteRocket(int viewId, int idCar)
    {
        var g = PhotonView.Find(viewId).transform;

        var carView = PhotonView.Find(idCar);

        if (carView != null)
        {
            var car = carView.transform;

            if (g != null)
            {
                Collider2D rocketCollider = g.GetComponent<Collider2D>();
                if (car != null)
                {
                    Physics2D.IgnoreCollision(rocketCollider, car.GetComponent<Collider2D>(), false);
                }

                g.transform.position = Vector2.zero;
                g.transform.rotation = Quaternion.identity;

                rocketCollider.enabled = true;
                rocketCollider.isTrigger = true;
                g.transform.parent = objectPool.transform;
                BonusNode bonusNode = g.GetComponent<BonusNode>();
                g.transform.localScale = new Vector2(0.625f, 0.625f);

                RocketLaunch rocketLaunch = g.GetComponent<RocketLaunch>();
                rocketLaunch.ResetLaunch();

                g.GetComponent<GunId>().Id = -1;
                bonusNode.Spawned = false;
                RefreshMap(g.gameObject);
                g.gameObject.SetActive(false);
            }
        }
    }

    public void ReturnBullet(GameObject g, int carId)
    {
        if (g.GetPhotonView().isMine)
        {
            photonView.RPC("RPCReturnBullet", PhotonTargets.All, g.GetPhotonView().viewID, carId);
        }
    }

    [PunRPC]
    public void RPCReturnBullet(int viewId, int carId)
    {
        PhotonView car = PhotonView.Find(carId);
        if (car != null)
        {
            var g = PhotonView.Find(viewId).transform;
            g.parent = car.transform;
            g.localPosition = Vector2.zero;

            IceLaunch ice = GetComponent<IceLaunch>();
            if (ice != null)
            {
                ice.Launched = false;
            }
            else
            {
                LaserLaunch laser = GetComponent<LaserLaunch>();
                if (laser != null)
                {
                    laser.Launched = false;
                }
            }

            g.gameObject.SetActive(false);
        }
    }

    private void RefreshMap(GameObject g)
    {
        mapBonuses.Remove(g.GetComponent<BonusNode>());
    }

    private void Spawn()
    {
        Vector2 pos = Vector2.zero;
        bool match = true;
        int index = -1;
        if (spawned > 0)
        {
            while (match == true)
            {
                int randomNumber = Random.Range(1, spawnPoints.Length);
                pos = spawnPoints[randomNumber].position;
                index = GetSpawnIndex(randomNumber);

                for (int i = 0; i < objects.Count; i++)
                {
                    if (objects[i] != null)
                    {
                        if ((objects[i].gameObject.transform.position.x.AlmostEquals(pos.x, 1f)) && (objects[i].gameObject.transform.position.y.AlmostEquals(pos.y, 1f)))
                        {
                            match = true;
                            break;
                        }
                        else
                        {
                            match = false;
                        }
                    }
                    else
                    {
                        match = true;
                        break;
                    }
                }
            }
        }
        else
        {
            int randomNumber = Random.Range(1, spawnPoints.Length);
            pos = spawnPoints[randomNumber].position;
            index = GetSpawnIndex(randomNumber);
        }

        if (index < 0 || index >= Bonunes.Length)
        {
            index = Random.Range(0, Bonunes.Length);
        }

        int bonusIndex = -1;

        bonusIndex = FindIndex(objects, index);

        if (bonusIndex == -1)
        {
            while (bonusIndex == -1)
            {
                int newIndex = Random.Range(0, Bonunes.Length);
                bonusIndex = FindIndex(objects, newIndex);
            }
        }

        photonView.RPC("ClientSpawn", PhotonTargets.All, objects[bonusIndex].gameObject.GetPhotonView().viewID, pos);
    }

    private int FindIndex(List<BonusNode> boxes, int index)
    {
        int bonusIndex = -1;
        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i].Index == index && boxes[i].Spawned == false)
            {
                bonusIndex = i;
                break;
            }
        }
        return bonusIndex;
    }

    [PunRPC]
    private void ClientSpawn(int id, Vector2 pos)
    {
        GameObject g = PhotonView.Find(id).gameObject;

        g.GetComponent<BonusNode>().Spawned = true;
        g.SetActive(true);
        g.transform.position = pos;
        if (mapBonuses.Contains(g.GetComponent<BonusNode>()) == false)
            mapBonuses.Add(g.GetComponent<BonusNode>());
        spawned++;
    }

    private int GetSpawnIndex(int randomNumber)
    {
        int index = -1;
        ObjectSpawnNode obj = spawnPoints[randomNumber].gameObject.GetComponent<ObjectSpawnNode>();
        if (obj != null)
        {
            if (obj.Weapon == false)
            {
                index = obj.Index;
            }
            else
            {
                int num = Random.Range(0, weaponIndexes.Length);
                index = weaponIndexes[num];
            }
        }
        return index;
    }

    public void SpawnObject()
    {
        if (PhotonNetwork.isMasterClient)
            StartCoroutine(SpawnObjectCycle());
    }

    private IEnumerator SpawnObjectCycle()
    {
        yield return new WaitForSeconds(spawnTimer);
        Spawn();
        yield return null;
    }

    IEnumerator AddOilTimer(float time, int viewId)
    {
        yield return new WaitForSeconds(time);
        RemoveArea(viewId);
        yield return null;
    }

    IEnumerator AddBombTimer(float time, int viewId, AreaObject areaObject)
    {
        yield return StartCoroutine(BombTimer(time, areaObject));
        RemoveArea(viewId);
        yield return null;
    }

    IEnumerator BombTimer(float time, AreaObject areaObject)
    {
        float remainingTime = time;

        while (remainingTime > 0)
        {
            if (areaObject.Spawned == false)
                break;
            remainingTime -= Time.deltaTime;
            yield return null;
        }
    }

    public List<BonusNode> GetMap()
    {
        return mapBonuses;
    }

    public Transform GetObjectPool()
    {
        return objectPool.transform;
    }

    public Transform GetAreaObjectPool()
    {
        return areaObjectPool.transform;
    }

    public void AddBonus(BonusNode bonus)
    {
        if (objects.Contains(bonus) == false)
            objects.Add(bonus);
    }

    public void AddArea(AreaObject area)
    {
        if (areaObjects.Contains(area) == false)
            areaObjects.Add(area);
    }
}
