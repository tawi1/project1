using System.Collections;
using UnityEngine;

public class Frozen : Photon.PunBehaviour
{
    private float spawnTime = 3f;
    private bool freezed = false;
    private bool abort = false;
    private Atlas atlas;
    ScoreManager scoreManager;

    public void Awake()
    {
        atlas = FindObjectOfType<Atlas>();
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    public void Freeze(int ownerId)
    {
        float endTime = PhotonNetwork.ServerTimestamp + spawnTime * 1000;
        photonView.RPC("RPCFreeze", PhotonTargets.All, ownerId, gameObject.GetPhotonView().viewID, endTime);
    }

    [PunRPC]
    private void RPCFreeze(int ownerId, int idCar, float time)
    {
        StartCoroutine(RespawnTimerTimer(ownerId, idCar, time));
    }

    private IEnumerator RespawnTimerTimer(int ownerId, int idCar, float time)
    {
        GameObject car = PhotonView.Find(idCar).gameObject;
        CarScript carScript = car.GetComponent<CarScript>();
        Frozen frozen = car.GetComponent<Frozen>();
        if (!frozen.Freezed)
        {
            frozen.freezed = true;
            float currentTime = PhotonNetwork.ServerTimestamp;

            float finishTime = time - currentTime;
            finishTime /= 1000;

            if (finishTime < 0)
                finishTime = 0;

            car.GetComponent<Rigidbody2D>().drag = 0.15f;
            Sprite carSprite = atlas.GetFrozenCar(idCar);
            carScript.GetComponent<CarSkin>().SetModel(carSprite);
            car.GetComponent<StepsController>().DisableSmoke();
            carScript.DisableNitro();
            carScript.GetComponent<CarSound>().DisableSound();
            carScript.DisableRpm();
            carScript.Fragile = true;

            if (carScript.AI)
            {
                car.GetComponent<PathFollower>().Stuck = 0;
                car.GetComponent<PathFollower>().DisableReset();
            }

            carScript.Control = false;

            if (ownerId != car.GetPhotonView().ownerId)
            {
                if (PhotonNetwork.player.ID == ownerId)
                {
                    scoreManager.AddFrozeScore();
                }
            }

            if (finishTime > 0)
                yield return StartCoroutine(Timer(finishTime));
            Spawn(car);
        }
        yield return null;
    }

    private void Spawn(GameObject car)
    {
        CarScript carScript = car.GetComponent<CarScript>();

        Sprite carSprite = atlas.GetSpriteCar(car.GetPhotonView().viewID);
        carScript.GetComponent<CarSkin>().SetModel(carSprite);
        car.GetComponent<Rigidbody2D>().drag = 1f;
        car.GetComponent<Frozen>().freezed = false;

        carScript.Fragile = false;
        if (abort == false)
        {
            carScript.EnableRpm();
            carScript.GetComponent<CarSound>().EnableSound();
            if (!carScript.Finished)
                carScript.Control = true;
            else
                carScript.Control = false;
        }
    }

    private IEnumerator Timer(float time)
    {
        float remainingTime = time;

        while (remainingTime > 0)
        {
            if (abort == true)
                break;
            remainingTime -= Time.deltaTime;
            yield return null;
        }
    }

    public bool Freezed
    {
        get
        {
            return freezed;
        }
    }

    public bool Abort
    {
        set
        {
            abort = value;
        }
    }
}
