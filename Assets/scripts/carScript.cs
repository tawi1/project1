using UnityEngine;
using System.Collections;

public class carScript : MonoBehaviour
{

    public float speed = 0f;
    public float maxspeed = 0.3f;
    public float maxSideSpeed = 0.15f;
    public float acceleration = 0.1f;
    public float deacceleration = 0.0005f;
    public float maxWallSpeed = 0.15f;
    public float brake = 100f;
    public float gas = 0f;
    public float rotator = 0f;
    public float sideSpeed = 2f;
    public bool haveGun = false;
    public int laps = 0;
    public int Score = 0;
    Vector3 lastPosition;
    Vector3 direction;

    // Use this for initialization
    void Start()
    {
        maxWallSpeed = 0.5f * maxspeed;
    }

    // Update is called once per frame
    void Update()
    {
        rotator = Input.GetAxis("Horizontal");

        gas = Input.GetAxis("Vertical");

        if (gas == 0)
        {
            speed = Mathf.Clamp(speed - deacceleration * Time.deltaTime, 0f, maxspeed);
        }
        if (gas < 0)
        {
            speed = Mathf.Clamp(speed - brake * Time.deltaTime, 0f, maxspeed);
        }
        else
        if ((gas > 0) && (rotator == 0))
        {
            speed = Mathf.Clamp(speed + acceleration * Time.deltaTime, 0f, maxspeed);
        }
        if (gas > 0)
        {
            if ((speed > maxSideSpeed) && (rotator != 0))
            {
                speed = Mathf.Clamp(speed - brake * Time.deltaTime, 0f, maxspeed);
            }
            else
            {
                speed = Mathf.Clamp(speed + acceleration * Time.deltaTime, 0f, maxspeed);
            }
        }

        transform.Translate(new Vector3(speed, 0f, 0f));

        if ((rotator > 0) && (speed > 0.01f))
        {
            transform.Rotate(new Vector3(0f, 0f, -sideSpeed));
        }
        else
            if ((rotator < 0) && (speed > 0.01f))
        {
            transform.Rotate(new Vector3(0f, 0f, sideSpeed));

        }

        direction = transform.position - lastPosition;
        lastPosition = transform.position;

        if (Input.GetKeyDown("space"))
        {
            if (haveGun == true)
            {
                fire();
            }
        }
    }

    void OnGUI()
    {
        GUI.Box(new Rect(0, 0, 100, 100), "Laps: " + laps);
    }

    void fire()
    {
        haveGun = false;
        Destroy(gameObject.transform.FindChild("minigun").gameObject);
    }


    void OnTriggerEnter2D(Collider2D c)
    {
        if (c.gameObject.name == "minigun")
        {
            if (haveGun == false)
            {
                c.gameObject.transform.SetParent(transform);
                c.transform.position = transform.position;
                Vector3 temp = transform.rotation.eulerAngles;

                temp.z = 226f + transform.rotation.eulerAngles.z;
                c.gameObject.layer = 4;
                c.transform.rotation = Quaternion.Euler(0, 0, temp.z);
                //Destroy(c.gameObject.collider2D);
                haveGun = true;
            }
        }
    }

    void OnTriggerExit2D(Collider2D c)
    {
        if (c.gameObject.name == "Start")
        {     
                laps++;        
        }
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (c.gameObject.tag == "Barrier")
        {
            float result = 0;
            foreach (ContactPoint2D contact in c.contacts)
            {
                result = 90 - (180 - Vector2.Angle(direction, contact.normal));
            }
            if (result > 70)
            {
                speed = 0;
            }
        }
    }

    void OnCollisionStay2D(Collision2D c)
    {
        if (c.gameObject.tag == "Barrier")
        {
            float result = 0;
            foreach (ContactPoint2D contact in c.contacts)
            {
                result = 90 - (180 - Vector2.Angle(direction, contact.normal));
            }
            if ((result > 70) && (rotator == 0))
            {
                speed = 0;
            }
            if (speed > maxWallSpeed)
            {
                speed = Mathf.Clamp(speed - brake * Time.deltaTime, 0f, speed);
            }
        }
    }
}
