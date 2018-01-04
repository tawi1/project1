using System.Collections;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    private float oilTime = 2f;
    private float oil = 1;
    private float oilMult = 0.3f;
    private ScoreManager scoreManager;
    [SerializeField]
    private PhotonView car;

    private void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.tag == "Area")
            if (c.name.Contains("Stain") == true)
            {
                int ownerId = c.GetComponent<StainId>().OwnerId;
                if (ownerId != car.ownerId)
                {
                    if (PhotonNetwork.player.ID == ownerId)
                    {
                        scoreManager.AddOilScore();
                    }
                }

                oil = oilMult;
                StopCoroutine(AddOilTimer());
                StartCoroutine(AddOilTimer());
            }
    }

    private IEnumerator AddOilTimer()
    {
        yield return new WaitForSeconds(oilTime);
        DisableOil();
        yield return null;
    }

    public void DisableOil()
    {
        oil = 1f;
    }

    public float GetOil()
    {
        return oil;
    }
}
