using System.Collections;
using UnityEngine;

public class Bonus : Photon.PunBehaviour
{
    //nitro 0
    //rocket 1
    //oil 2
    //bomb 3
    //flame 4
    //laser 5
    //ice 6

    private bool[] bonuses = new bool[7];
    private int nitroLimit = 3;
    private int flameLimit = 3;
    private bool nitroEnabled = false;
    private bool flameEnabled = false;
    private GameObject gun;
    private Buttons buttons;
    [SerializeField]
    private GameObject stain;
    private ObjectManager objectManager;
    private CarScript carScript;
    private AiButtons aiButtons;
    private float remainingTime = 0f;
    private float remainingTimeFlame = 0f;
    [SerializeField]
    private GameObject flame;
    [SerializeField]
    private AudioSource flameSound;
    [SerializeField]
    private AudioSource laserShot;
    private int gunOrder = 5;
    private bool isColliding = false;
    private Collider2D carCollider;
    private ScoreManager scoreManager;
    [SerializeField]
    private IceLaunch[] ices;
    [SerializeField]
    private LaserLaunch[] lasers;
    private MatchController matchController;

    // Use this for initialization
    void Start()
    {
        objectManager = FindObjectOfType<ObjectManager>();
        matchController = FindObjectOfType<MatchController>();
        scoreManager = FindObjectOfType<ScoreManager>();
        carScript = gameObject.GetComponent<CarScript>();
        carCollider = gameObject.GetComponent<Collider2D>();

        for (int i = 0; i < bonuses.Length; i++)
        {
            bonuses[i] = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        FlameSelector();
        isColliding = false;
        if (!photonView.isMine)
            return;

        if (!carScript.AI)
        {
            if (buttons != null)
            {
                if (bonuses[0] == true)
                {
                    buttons.EnableNitro();

                    if (buttons.GetNitro())
                    {
                        AddScore();
                        buttons.DisableNitro();
                        ActivateNitro();
                    }
                }
                if (bonuses[1] == true)
                {
                    buttons.EnableRocket();

                    if (buttons.GetRocket())
                    {
                        AddScore();
                        buttons.DisableRocket();
                        ActivateRocket();
                    }
                }
                if (bonuses[2] == true)
                {
                    buttons.EnableOil();

                    if (buttons.GetOil())
                    {
                        AddScore();
                        buttons.DisableOil();
                        ActivateOil();
                    }
                }
                if (bonuses[3] == true)
                {
                    buttons.EnableBomb();

                    if (buttons.GetBomb())
                    {
                        AddScore();
                        buttons.DisableBomb();
                        ActivateBomb();
                    }
                }
                if (bonuses[4] == true)
                {
                    buttons.EnableFlame();

                    if (buttons.GetFlame())
                    {
                        AddScore();
                        buttons.DisableFlame();
                        ActivateFlame();
                    }
                }
                if (bonuses[5] == true)
                {
                    buttons.EnableLaser();

                    if (buttons.GetLaser())
                    {
                        AddScore();
                        buttons.DisableLaser();
                        ActivateLaser();
                    }
                }
                if (bonuses[6] == true)
                {
                    buttons.EnableIce();

                    if (buttons.GetIce())
                    {
                        AddScore();
                        buttons.DisableIce();
                        ActivateIce();
                    }
                }
            }
            else
            {
                buttons = CarScript.ButtonsInit();
            }
        }
        else
        {
            if (aiButtons)
            {
                if (bonuses[0] == true)
                {
                    if (aiButtons.GetNitro())
                    {
                        ActivateNitro();
                    }
                }
                if (bonuses[1] == true)
                {
                    if (aiButtons.GetRocket())
                    {
                        ActivateRocket();
                    }
                }
                if (bonuses[2] == true)
                {
                    if (aiButtons.GetOil())
                    {
                        ActivateOil();
                    }
                }
                if (bonuses[3] == true)
                {
                    if (aiButtons.GetBomb())
                    {
                        ActivateBomb();
                    }
                }
                if (bonuses[4] == true)
                {
                    if (aiButtons.GetFlame())
                    {
                        ActivateFlame();
                    }
                }
                if (bonuses[5] == true)
                {
                    if (aiButtons.GetLaser())
                    {
                        ActivateLaser();
                    }
                }
                if (bonuses[6] == true)
                {
                    if (aiButtons.GetIce())
                    {
                        ActivateIce();
                    }
                }
            }
            else
            {
                AiButtonsInit();
            }
        }
    }

    private void AiButtonsInit()
    {
        aiButtons = gameObject.GetComponent<AiButtons>();
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        if (photonView.isMine)
        {
            if (isColliding) return;
            isColliding = true;

            if (c.gameObject.tag == "Bonus")
            {
                if (carCollider.isTrigger == false)
                {
                    BonusNode bonusNode = c.GetComponent<BonusNode>();
                    if (bonusNode != null)
                    {
                        PhotonView id = bonusNode.gameObject.GetPhotonView();

                        if (id != null)
                        {
                            if (bonuses[bonusNode.Index] == false)
                                if (!matchController.LobbyGame)
                                    photonView.RPC("RPCOnTriggerEnter2D", PhotonTargets.All, gameObject.GetPhotonView().viewID, id.viewID);
                                else
                                    RPCOnTriggerEnter2D(gameObject.GetPhotonView().viewID, id.viewID);
                        }
                    }
                }
            }
        }
    }

    [PunRPC]
    void RPCOnTriggerEnter2D(int idCar, int id)
    {
        GameObject car = PhotonView.Find(idCar).gameObject;

        Bonus carBonus = car.GetComponent<Bonus>();

        if (PhotonView.Find(id) == null)
            return;

        GameObject g = PhotonView.Find(id).gameObject;

        Collider2D c = g.GetComponent<Collider2D>();
        int bonusIndex = -1;
        if (c.GetComponent<BonusNode>() != null)
            bonusIndex = c.GetComponent<BonusNode>().Index;

        if (bonusIndex == 0)
        {
            if ((carBonus.bonuses[0] == false && carBonus.bonuses[4] == false) && (flameEnabled == false))
            {
                carBonus.bonuses[0] = true;
                objectManager.DeleteObject(c.gameObject, true);
            }
        }
        else if (bonusIndex == 1)
        {
            if (carBonus.bonuses[1] == false && carBonus.bonuses[5] == false && carBonus.bonuses[6] == false)
            {
                AddGun(1, c, car, carBonus, -89);
            }
        }
        else if (bonusIndex == 2)
        {
            if (carBonus.bonuses[2] == false && carBonus.bonuses[3] == false)
            {
                carBonus.bonuses[2] = true;
                objectManager.DeleteObject(c.gameObject, true);
            }
        }
        else if (bonusIndex == 3)
        {
            if (carBonus.bonuses[2] == false && carBonus.bonuses[3] == false)
            {
                carBonus.bonuses[3] = true;
                objectManager.DeleteObject(c.gameObject, true);
            }
        }
        else if (bonusIndex == 4)
        {
            if ((carBonus.bonuses[0] == false && carBonus.bonuses[4] == false) && (nitroEnabled == false))
            {
                carBonus.bonuses[4] = true;
                objectManager.DeleteObject(c.gameObject, true);
            }
        }
        else if (bonusIndex == 5)
        {
            if (carBonus.bonuses[1] == false && carBonus.bonuses[5] == false && carBonus.bonuses[6] == false)
            {
                AddGun(5, c, car, carBonus, -179);
            }
        }
        else if (bonusIndex == 6)
        {
            if (carBonus.bonuses[1] == false && carBonus.bonuses[5] == false && carBonus.bonuses[6] == false)
            {
                AddGun(6, c, car, carBonus, -179);
            }
        }
    }

    private void AddGun(int index, Collider2D col, GameObject car, Bonus carBonus, float rotation)
    {
        GunId localGun = col.gameObject.GetComponent<GunId>();
        if (localGun.Id == -1)
        {
            GameObject gunObj = col.gameObject;
            PhotonView carView = car.GetPhotonView();
            localGun.Id = carView.viewID;
            carBonus.bonuses[index] = true;

            objectManager.SpawnObject();

            gunObj.GetComponent<SpriteRenderer>().sortingOrder = gunOrder;

            if (PhotonNetwork.isMasterClient)
            {
                PhotonView gunView = gunObj.GetPhotonView();
                if (gunView.ownerId != carView.ownerId)
                    gunView.TransferOwnership(carView.ownerId);
            }

            Collider2D rocketCollider = col;
            if (index == 1)
            {
                gunObj.GetComponent<RocketLaunch>().IdCar = carView.viewID;
                Physics2D.IgnoreCollision(rocketCollider, car.GetComponent<Collider2D>());
                rocketCollider.isTrigger = false;
            }
            else
            {
                rocketCollider.enabled = false;
            }

            gunObj.transform.parent = car.transform;
            gunObj.transform.position = car.transform.position;

            Vector3 temp = transform.rotation.eulerAngles;

            temp.z = rotation + car.transform.rotation.eulerAngles.z;
            gunObj.transform.rotation = Quaternion.Euler(0, 0, temp.z);
            carBonus.gun = gunObj;
        }
    }

    private void AddScore()
    {
        scoreManager.AddScore();
    }

    #region Nitro

    void ActivateNitro()
    {
        if (bonuses[0] == true)
        {
            int currentTime = PhotonNetwork.ServerTimestamp + 1000 * nitroLimit;

            photonView.RPC("RPCActivateNitro", PhotonTargets.All, gameObject.GetPhotonView().viewID, currentTime);
        }
    }

    IEnumerator NitroSwitcher(GameObject g, float time)
    {
        yield return StartCoroutine(NitroSwitcher(time));
        g.GetComponent<Bonus>().nitroEnabled = false;
        yield return null;
    }

    IEnumerator NitroSwitcher(float time)
    {
        remainingTime = time;

        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            yield return null;
        }

        if (remainingTime < 0)
            remainingTime = 0;
    }

    [PunRPC]
    void RPCActivateNitro(int idCar, int endTime)
    {
        GameObject car = PhotonView.Find(idCar).gameObject;
        Bonus carBonus = car.GetComponent<Bonus>();
        carBonus.nitroEnabled = true;
        carBonus.bonuses[0] = false;
        StopAllCoroutines();

        float newTime = endTime - PhotonNetwork.ServerTimestamp;
        newTime /= 1000;
        float time = newTime + carBonus.remainingTime;

        StartCoroutine(NitroSwitcher(car, time));
    }
    #endregion

    void ActivateRocket()
    {
        photonView.RPC("RPCActivateRocket", PhotonTargets.All, gameObject.GetPhotonView().viewID);
    }

    [PunRPC]
    void RPCActivateRocket(int idCar)
    {
        GameObject car = PhotonView.Find(idCar).gameObject;

        Bonus carBonus = car.GetComponent<Bonus>();
        carBonus.bonuses[1] = false;
        if (carBonus.gun != null)
        {
            carBonus.gun.GetComponent<Transform>().parent = null;
            carBonus.gun.GetComponent<RocketLaunch>().SetLaunch();
        }
    }

    void ActivateOil()
    {
        photonView.RPC("RPCActivateOil", PhotonTargets.All, gameObject.GetPhotonView().viewID);

        objectManager.CreateArea(AreaObject.AreaType.Stain, gameObject.transform.position + gameObject.transform.TransformDirection(new Vector2(-(gameObject.GetComponent<CapsuleCollider2D>().size.x / 3), 0)), gameObject.GetPhotonView().ownerId);
    }

    [PunRPC]
    void RPCActivateOil(int idCar)
    {
        GameObject car = PhotonView.Find(idCar).gameObject;
        car.GetComponent<Bonus>().bonuses[2] = false;
    }

    void ActivateBomb()
    {
        photonView.RPC("RPCActivateBomb", PhotonTargets.All, gameObject.GetPhotonView().viewID);

        objectManager.CreateArea(AreaObject.AreaType.Bomb, gameObject.transform.position + gameObject.transform.TransformDirection(new Vector2(-(gameObject.GetComponent<CapsuleCollider2D>().size.x / 2.5f), 0)), gameObject.GetPhotonView().ownerId);
    }

    [PunRPC]
    void RPCActivateBomb(int idCar)
    {
        GameObject car = PhotonView.Find(idCar).gameObject;
        car.GetComponent<Bonus>().bonuses[3] = false;
    }

    void ActivateFlame()
    {
        if (bonuses[4] == true)
        {
            int currentTime = PhotonNetwork.ServerTimestamp + 1000 * flameLimit;
            photonView.RPC("RPCActivateFlame", PhotonTargets.All, gameObject.GetPhotonView().viewID, currentTime);
        }
    }

    [PunRPC]
    void RPCActivateFlame(int idCar, int endTime)
    {
        GameObject car = PhotonView.Find(idCar).gameObject;
        Bonus carBonus = car.GetComponent<Bonus>();
        carBonus.flameEnabled = true;
        carBonus.bonuses[4] = false;
        StopAllCoroutines();

        float newTime = endTime - PhotonNetwork.ServerTimestamp;
        newTime /= 1000;
        float time = newTime + carBonus.remainingTimeFlame;

        StartCoroutine(FlameSwitcher(car, time));
    }

    IEnumerator FlameSwitcher(GameObject g, float time)
    {
        yield return StartCoroutine(FlameSwitcher(time));
        g.GetComponent<Bonus>().flameEnabled = false;
        yield return null;
    }

    IEnumerator FlameSwitcher(float time)
    {
        remainingTimeFlame = time;

        while (remainingTimeFlame > 0)
        {
            remainingTimeFlame -= Time.deltaTime;
            yield return null;
        }

        if (remainingTimeFlame < 0)
            remainingTimeFlame = 0;
    }

    private void FlameSelector()
    {
        if (flameEnabled)
        {
            if (!flame.activeSelf)
            {
                flame.SetActive(true);
                if (flameSound != null)
                    flameSound.Play();
            }
        }
        else
        {
            if (flame.activeSelf)
            {
                flame.SetActive(false);
                if (flameSound != null)
                    flameSound.Stop();
            }
        }
    }

    void ActivateLaser()
    {
        photonView.RPC("RPCActivateLaser", PhotonTargets.All, gameObject.GetPhotonView().viewID);
    }

    [PunRPC]
    void RPCActivateLaser(int idCar)
    {
        GameObject car = PhotonView.Find(idCar).gameObject;

        Bonus carBonus = car.GetComponent<Bonus>();
        carBonus.bonuses[5] = false;

        int laserIndex = 0;
        for (int i = 0; i < lasers.Length; ++i)
        {
            if (lasers[i].Launched == false)
            {
                laserIndex = i;
                break;
            }
        }

        LaserLaunch laserLaunch = lasers[laserIndex];
        LineRenderer laserRender = lasers[laserIndex].GetComponent<LineRenderer>();

        laserLaunch.gameObject.SetActive(true);
        Color32 tmp = laserRender.startColor;
        tmp.a = 255;
        laserRender.startColor = tmp;
        laserRender.endColor = tmp;
        carBonus.PlayLaserShot();
        carBonus.lasers[laserIndex].GetComponent<BoxCollider2D>().isTrigger = false;
        carBonus.lasers[laserIndex].transform.parent = null;
        laserRender.sortingOrder = 3;

        if (car.GetPhotonView().isMine)
        {
            objectManager.DeleteObject(carBonus.Rocket.gameObject, false);
            laserLaunch.Dir = car.transform.right;
        }

        laserLaunch.SetLaunch();
    }

    void ActivateIce()
    {
        photonView.RPC("RPCActivateIce", PhotonTargets.All, gameObject.GetPhotonView().viewID);
    }

    [PunRPC]
    void RPCActivateIce(int idCar)
    {
        GameObject car = PhotonView.Find(idCar).gameObject;

        Bonus carBonus = car.GetComponent<Bonus>();
        carBonus.bonuses[6] = false;

        int iceIndex = 0;
        for (int i = 0; i < ices.Length; ++i)
        {
            if (ices[i].Launched == false)
            {
                iceIndex = i;
                break;
            }
        }
        IceLaunch iceLaunch = ices[iceIndex];
        SpriteRenderer iceRender = ices[iceIndex].GetComponent<SpriteRenderer>();

        Color32 tmp = iceRender.color;
        tmp.a = 255;
        iceRender.color = tmp;

        iceLaunch.gameObject.SetActive(true);
        carBonus.PlayLaserShot();
        carBonus.ices[iceIndex].GetComponent<Collider2D>().isTrigger = false;
        carBonus.ices[iceIndex].transform.parent = null;

        if (car.GetPhotonView().isMine)
        {
            objectManager.DeleteObject(carBonus.Rocket.gameObject, false);
        }

        iceLaunch.SetLaunch();
    }

    public void ResetCar()
    {
        for (int i = 0; i < bonuses.Length; i++)
        {
            bonuses[i] = false;
        }
        StopAllCoroutines();
        nitroEnabled = false;
        flameEnabled = false;
        remainingTimeFlame = 0;
        remainingTime = 0;

        if (objectManager == null)
            objectManager = FindObjectOfType<ObjectManager>();

        if (lasers != null)
            for (int i = 0; i < lasers.Length; ++i)
            {
                if (lasers[i] != null)
                    if (lasers[i].ViewId == -1)
                    {
                        lasers[i].ViewId = gameObject.GetPhotonView().viewID;
                        lasers[i].OwnerId = gameObject.GetPhotonView().ownerId;
                        if (lasers[i].gameObject.GetPhotonView().ownerId != lasers[i].OwnerId)
                        {
                            lasers[i].gameObject.GetPhotonView().TransferOwnership(lasers[i].OwnerId);
                        }
                    }

                objectManager.ReturnBullet(lasers[i].gameObject, this.gameObject.GetPhotonView().viewID);
            }

        if (gun != null)
        {
            if (gun.GetComponent<GunId>().Id != -1)
            {
                if (gun.GetComponent<RocketLaunch>() == null)
                {
                    objectManager.DeleteObject(gun, false);
                }
                else
                {
                    objectManager.DeleteRocket(gun.GetPhotonView().viewID, gameObject.GetPhotonView().viewID);
                }
            }
        }

        for (int i = 0; i < ices.Length; ++i)
        {
            objectManager.ReturnBullet(ices[i].gameObject, gameObject.GetPhotonView().viewID);
            ices[i].OwnerId = gameObject.GetPhotonView().ownerId;
            if (ices[i].gameObject.GetPhotonView().ownerId != ices[i].OwnerId)
            {
                ices[i].gameObject.GetPhotonView().TransferOwnership(ices[i].OwnerId);
            }
        }

        if (buttons != null)
        {
            buttons.Reset();
        }
    }

    public void PlayLaserShot()
    {
        if (laserShot != null)
            laserShot.Play();
    }

    public bool NitroState
    {
        get
        {
            return nitroEnabled;
        }
    }

    public GameObject Rocket
    {
        get
        {
            return gun;
        }
        set
        {
            gun = value;
        }
    }

    public bool[] GetBonus()
    {
        return bonuses;
    }

    public float RemainingTime
    {
        get
        {
            return remainingTime;
        }
    }
}