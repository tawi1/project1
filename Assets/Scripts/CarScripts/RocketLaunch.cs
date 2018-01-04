using UnityEngine;

public class RocketLaunch : Photon.PunBehaviour
{
    private bool launch = false;
    private float rocketTimer = 0.4f;
    private float time = 0.4f;
    private float rocketSpeed = 80f;
    [SerializeField]
    private GameObject Engine;
    private ObjectManager objectManager;
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    private Rigidbody2D rb;
    private int idCar = 0;

    private void Start()
    {
        objectManager = FindObjectOfType<ObjectManager>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (launch == true)
        {
            if (photonView.isMine)
            {
                rb.velocity = transform.up * rocketSpeed;
            }

            rocketTimer -= Time.deltaTime;
            if (rocketTimer < 0)
            {
                if (photonView.isMine)
                    Explode();
                DeleteRocket();
                launch = false;
            }
        }
        else if (photonView.isMine)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (launch == true)
            if ((c.gameObject.tag == "Barrier"))
            {
                launch = false;
                if (photonView.isMine)
                    Explode();
                DeleteRocket();
            }
            else if (c.gameObject.tag == "Player")
            {
                launch = false;
                if (photonView.isMine)
                {
                    rb.velocity = Vector2.zero;
                    Explode();
                    c.gameObject.GetComponent<Health>().Destroyed(gameObject.GetPhotonView().ownerId);
                }

                DeleteRocket();
            }
    }

    private void Explode()
    {
        objectManager.CreateArea(AreaObject.AreaType.FireArea, true, transform.position, gameObject.GetPhotonView().ownerId);
    }

    private void DeleteRocket()
    {
        if (photonView.isMine)
            objectManager.DeleteRocket(gameObject.GetPhotonView().viewID, idCar);
    }

    public void SetLaunch()
    {
        launch = true;
        rocketTimer = time;
        Engine.SetActive(true);
        audioSource.Play();
    }

    public void ResetLaunch()
    {
        launch = false;
        Engine.SetActive(false);
        idCar = 0;
        audioSource.Stop();
    }

    public float RocketSpeed
    {
        get
        {
            return rocketSpeed;
        }
    }

    public int IdCar
    {
        set
        {
            idCar = value;
        }
    }
}
