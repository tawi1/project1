using UnityEngine;
using System.Collections;

public class carScript : MonoBehaviour
{

    public float speed = 0f;
    public float maxspeed = 0.3f;
    public float maxSideSpeed = 0.15f;
    public float acceleration = 0.1f;
    public float deacceleration = 0.0005f;
    public float brake = 100f;
    public float gas = 0f;
    public float rotator = 0f;
    public float sideSpeed = 2f;
    public int checker = 0;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

      /*  Vector3 Placement = new Vector3(transform.position.x, transform.position.y, transform.position.x);
        CharacterController controller = GetComponent<CharacterController>();*/

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
    }

    
    void OnCollisionEnter(Collision c)
    {

        if (c.gameObject.tag == "Barrier")
        {
            Debug.Log("BOOM");
            checker += 1;
        }
    }
}
