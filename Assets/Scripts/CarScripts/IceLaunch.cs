using UnityEngine;

public class IceLaunch : Photon.PunBehaviour
{
    private bool launch = false;
    private float iceTimer = 0.5f;
    private float time = 0.5f;
    public static float iceSpeed = 70f;
    private ObjectManager objectManager;
    [SerializeField]
    private Rigidbody2D rb;
    private Vector2 dir = Vector2.zero;
    private int ownerId = 0;
    [SerializeField]
    private PhotonView car;
    private int viewId = 0;

    private void Start()
    {
        if (car != null)
            viewId = car.viewID;
        if (objectManager == null)
        {
            objectManager = FindObjectOfType<ObjectManager>();
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (photonView.isMine)
        {
            if (launch == true)
            {
                rb.velocity = transform.right * iceSpeed;

                iceTimer -= Time.fixedDeltaTime;
                if (iceTimer < 0)
                {
                    CreateArea();
                    if (launch == true)
                        ReturnBullet();
                    launch = false;
                }
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (launch == true)
            if ((c.gameObject.tag == "Barrier"))
            {
                launch = false;
                if (photonView.isMine)
                {
                    CreateArea();
                    ReturnBullet();
                }
            }
            else if (c.gameObject.tag == "Player")
            {
                launch = false;
                if (photonView.isMine)
                {
                    rb.velocity = Vector2.zero;
                    CreateArea();
                    c.gameObject.GetComponent<Frozen>().Freeze(ownerId);
                    ReturnBullet();
                }
            }
    }

    private void CreateArea()
    {
        objectManager.CreateArea(AreaObject.AreaType.IceArea, transform.position, ownerId);
    }

    private void ReturnBullet()
    {
        objectManager.ReturnBullet(gameObject, viewId);
    }

    public void SetLaunch()
    {
        if (objectManager == null)
        {
            objectManager = FindObjectOfType<ObjectManager>();
        }
        iceTimer = time;
        Vector2 startPos = car.transform.position + car.transform.TransformDirection(new Vector2(car.GetComponent<CapsuleCollider2D>().size.x * car.transform.localScale.x / 2 + 1f, 0));

        float direction = Quaternion.FromToRotation(Vector2.right, (car.GetComponent<CarScript>().GetCentralAxis().normalized)).eulerAngles.z;
        transform.eulerAngles = new Vector3(0, 0, direction);
        transform.position = startPos;
        transform.parent = null;

        launch = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.mass = 100;
        rb.drag = 0;
        rb.angularDrag = 0;
    }

    public float IceSpeed
    {
        get
        {
            return iceSpeed;
        }
    }

    public bool Launched
    {
        get
        {
            return launch;
        }
        set
        {
            launch = value;
        }
    }

    public int OwnerId
    {
        get
        {
            return ownerId;
        }
        set
        {
            ownerId = value;
        }
    }
}
