using System.Collections;
using UnityEngine;

public class Health : Photon.PunBehaviour
{
    private float spawnTime = 2f;

    ScoreManager scoreManager;

    private void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    public void Destroyed(int ownerId)
    {
        float endTime = PhotonNetwork.ServerTimestamp + spawnTime * 1000;

        photonView.RPC("RPCDestroyed", PhotonTargets.All, ownerId, gameObject.GetPhotonView().viewID, endTime);
    }

    [PunRPC]
    private void RPCDestroyed(int ownerId, int idCar, float time)
    {
        StartCoroutine(RespawnTimerTimer(ownerId, idCar, time));
    }

    private IEnumerator RespawnTimerTimer(int ownerId, int idCar, float time)
    {
        GameObject car = PhotonView.Find(idCar).gameObject;
        CarScript carScript = car.GetComponent<CarScript>();
        CarSkin carSkin = car.GetComponent<CarSkin>();

        if (!carSkin.CarHided())
        {
            carSkin.HideCar();

            float currentTime = PhotonNetwork.ServerTimestamp;

            float finishTime = time - currentTime;
            finishTime /= 1000;

            if (finishTime < 0)
                finishTime = 0;

            gameObject.GetComponent<Bonus>().ResetCar();

            Frozen frozen = car.GetComponent<Frozen>();
            car.GetComponent<StepsController>().DisableSmoke();
            carScript.DisableNitro();
            carScript.GetComponent<CarSound>().DisableSound();
            carScript.DisableRpm();

            if (carScript.AI)
            {
                car.GetComponent<PathFollower>().Stuck = 0;
                car.GetComponent<PathFollower>().DisableReset();
            }

            if (frozen.Freezed == true)
            {
                frozen.Abort = true;
            }

            carScript.Control = false;

            if (ownerId != car.GetPhotonView().ownerId)
            {
                if (PhotonNetwork.player.ID == ownerId)
                {
                    scoreManager.AddExplodeScore();
                }
            }

            if (car.GetPhotonView().isMine)
                car.GetComponent<CarCheckpoint>().Respawn();
            if (finishTime > 0)
                yield return new WaitForSeconds(finishTime);
            Spawn(carScript, frozen);
        }
        yield return null;
    }

    private void Spawn(CarScript carScript, Frozen frozen)
    {
        frozen.Abort = false;
        carScript.GetComponent<CarSkin>().ShowCar();
        carScript.GetComponent<CarSound>().EnableSound();
        if (!carScript.Finished)
            carScript.Control = true;
        else
            carScript.Control = false;
    }
}
