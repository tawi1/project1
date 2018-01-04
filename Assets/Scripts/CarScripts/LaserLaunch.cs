using UnityEngine;
using System.Collections;

public class LaserLaunch : Photon.PunBehaviour
{
    private bool launch = false;
    private float laserTimer = 2f;
    private float time = 2f;
    private int currentReflect = 0;
    private int maxReflect = 2;
    public static float laserSpeed = 50f;
    private ObjectManager objectManager;
    [SerializeField]
    private Rigidbody2D rb;
    private Vector2 dir = Vector2.zero;
    private bool lockHit = false;
    private int ownerId = 0;
    [SerializeField]
    private PhotonView car;
    private int viewId = -1;

    private void Start()
    {
        if (objectManager == null)
            objectManager = FindObjectOfType<ObjectManager>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (photonView.isMine)
        {
            if (launch == true)
            {
                rb.velocity = dir * laserSpeed;

                laserTimer -= Time.fixedDeltaTime;
                if (laserTimer < 0)
                {
                    if (launch == true)
                        objectManager.ReturnBullet(gameObject, viewId);
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
                if (photonView.isMine)
                {
                    if (lockHit == false)
                    {
                        if (currentReflect < maxReflect)
                        {
                            lockHit = true;
                            currentReflect++;

                            Vector3 normal = c.contacts[0].normal;
                            Vector3 vel = rb.velocity;
                            vel = vel.normalized;

                            float dotNormal = Vector2.Dot(normal, vel);

                            if (dotNormal > 0)
                                normal *= -1;

                            dir = Vector2.Reflect(vel, normal);

                            float dot = Vector2.Dot(dir, vel);
                            if (dot <= 0)
                            {
                                dir = Vector2.Reflect(vel, normal * -1);
                            }

                            float direction = Quaternion.FromToRotation(Vector2.right, dir).eulerAngles.z;
                            gameObject.transform.eulerAngles = new Vector3(0, 0, direction);

                            StartCoroutine(AddTimer());
                        }
                        else
                        {
                            objectManager.ReturnBullet(gameObject, viewId);
                        }
                    }
                }
            }
            else if (c.gameObject.tag == "Player")
            {
                if (photonView.isMine)
                {
                    rb.velocity = Vector2.zero;
                    c.gameObject.GetComponent<Health>().Destroyed(ownerId);

                    launch = false;

                    objectManager.ReturnBullet(gameObject, viewId);
                }
            }
    }

    IEnumerator AddTimer()
    {
        yield return new WaitForSeconds(0.1f);
        lockHit = false;
        yield return null;
    }

    public void SetLaunch()
    {
        if (objectManager == null)
        {
            objectManager = FindObjectOfType<ObjectManager>();
        }
        laserTimer = time;
        currentReflect = 0;
        Vector2 startPos = car.transform.position + car.transform.TransformDirection(new Vector2(car.GetComponent<CapsuleCollider2D>().size.x * car.transform.localScale.x / 2 + 0.1f, 0));
        int laserLength = 4;
        Vector2 endPos = car.GetComponent<CarScript>().GetCentralAxis().normalized;
        endPos = endPos * laserLength;
        endPos = startPos + endPos;
        Vector2 center = (startPos + endPos) / 2;
        transform.position = center;

        Vector2 dir = endPos - startPos;
        float direction = Quaternion.FromToRotation(Vector2.right, (endPos - startPos)).eulerAngles.z;
        transform.eulerAngles = new Vector3(0, 0, direction);
        transform.parent = null;

        launch = true;
    }

    public float LaserSpeed
    {
        get
        {
            return laserSpeed;
        }
    }

    public Vector2 Dir
    {
        set
        {
            dir = value;
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

    public int ViewId
    {
        get
        {
            return viewId;
        }
        set
        {
            viewId = value;
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
